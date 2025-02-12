using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Newtonsoft.Json;

using BestHTTP;
using BestHTTP.Authentication;
using System.Text;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class ComfyApiProvider
{
    string _baseUrl;
    string _user;
    string _pass;

    #region PUBLIC
    
    public ComfyApiProvider(string baseUrl, string user, string pass)
    {
        _baseUrl = baseUrl;
        _user = user;
        _pass = pass;
    }
    
    public async Task<T> GetNodeInfoAsync<T>()
    {
        string classTypeName = typeof(T).ToString();
        Debug.Log($"Calling GetNodeInfoAsync for '/object_info/{classTypeName}'");
        var source = new TaskCompletionSource<T>();
        GenerateRequest($"/object_info/{classTypeName}", HTTPMethods.Get, 
            response => 
            {
                var result = JsonConvert.DeserializeObject<Dictionary<string, T>>(response.DataAsText);
                Debug.Log($"GetNodeInfoAync for '/object_info/{classTypeName}' returned '{result.Count}' results");
                source.TrySetResult(result[classTypeName]);
            });
        return await source.Task;
    }

    public Task<ComfyUploadImageResponse> UploadImageAsync(byte[] imageData, ComfyImageFormat format, string filename, string subfolder = null)
    {
        var source = new TaskCompletionSource<ComfyUploadImageResponse>();
        
        var uri = new Uri(_baseUrl + "/upload/image");
        var request = new HTTPRequest(uri, HTTPMethods.Post,
            GetReturnDelegate(response =>
            {
                var castResponse = JsonConvert.DeserializeObject<ComfyUploadImageResponse>(response.DataAsText);
                source.TrySetResult(castResponse);
            }));
        
        var creds = new Credentials(AuthenticationTypes.Basic, _user, _pass);

        if (imageData == null || imageData.Length == 0)
        {
            throw new Exception($"invalid imageData: '{filename}'");
        }

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new Exception($"invalid filename: '{filename}'");
        }

        var mimeType = MimeTypeForFormat(format);
        request.AddBinaryData("image", imageData, filename, mimeType);

        if (!string.IsNullOrEmpty(subfolder))
        {
            request.AddField("subfolder", subfolder);
        }

        request.Credentials = creds;
        request.Send();
        
        // In editor cache uploaded image for debugging & testing purposes
        source.Task.ContinueWith(x => 
        {
            if (x.IsCompletedSuccessfully)
            {
                var extension = FileExtensionForFormat(format);
                var cacheFilename = Path.ChangeExtension(x.Result.Name, extension);
                
                SaveBytesToTextureAsset(
                    cacheFilename,
                    $"cache/upload/{x.Result.Subfolder}",
                    imageData);
            }
        });
        
        return source.Task;
    }

    public void GetImage(string filename, string type = "output", Action<Sprite> onSuccess = null)
    {
        var request = GenerateRequest($"/view?filename={filename}&type={type}", HTTPMethods.Get, 
            response => 
            {
                var bytes = response.Data;

                Texture2D tex2d = new(2, 2);
                tex2d.LoadImage(bytes);
                
                SaveBytesToTextureAsset(filename, $"cache/{type}", bytes);

                Sprite sprite = LoadTextureAsSprite(tex2d);

                onSuccess?.Invoke(sprite);
            });
    }

    public async Task<Texture2D> GetImageAsync(string filename, string type = "output")
    {
        var source = new TaskCompletionSource<Texture2D>();
        GenerateRequest($"/view?filename={filename}&type={type}", HTTPMethods.Get, 
            response => 
            {
                Debug.Log($"GetImageAsync({filename}) response IsSuccess:{response.IsSuccess} StatusCode:{response.StatusCode}");

                var bytes = response.Data;

                Texture2D tex2d = new(2, 2);
                tex2d.LoadImage(bytes);

                SaveBytesToTextureAsset(filename, $"cache/{type}", bytes);

                source.TrySetResult(tex2d);
            });

        return await source.Task;
    }

    public async Task<Sprite> GetSpriteAsync(string filename, string type = "output")
    {
        var result = await GetImageAsync(filename, type);
        var sprite = LoadTextureAsSprite(result);
        return sprite;
    }

    Sprite LoadBytesAsSprite(byte[] bytes)
    {
        var tex = new Texture2D(512, 512);
        tex.LoadImage(bytes);

        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    Sprite LoadTextureAsSprite(Texture2D tex)
    {
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    /// <summary>
    /// Save texture locally into Assets, for debugging purposes.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    void SaveBytesToTextureAsset(string filename, string type, byte[] bytes)
    {
#if UNITY_EDITOR
        var absoluteFilePath = Path.Combine(Application.dataPath, type, filename);
        var relativeFilePath = Path.Combine("Assets", type, filename);

        // Ensure folder exists 
        Directory.CreateDirectory(Path.Combine(Application.dataPath, type));

        File.WriteAllBytes(absoluteFilePath, bytes);
        AssetDatabase.ImportAsset(relativeFilePath);
#endif
    }

    public void GetPromptHistory(string promptId, Action<PromptHistory> onSuccess = null)
    {
        var request = GenerateRequest($"/history/{promptId}", HTTPMethods.Get,
            response =>
            {
                var obj = JsonConvert.DeserializeObject<Dictionary<string, PromptHistory>>(response.DataAsText);
                onSuccess?.Invoke(obj.First().Value);
            });
    }

    public async Task<PromptHistory> GetPromptHistoryAsync(string promptId)
    {
        var source = new TaskCompletionSource<PromptHistory>();

        Debug.Log($"Requesting history of promptId '{promptId}'...");
        
        GenerateRequest($"/history/{promptId}", HTTPMethods.Get,
            response =>
            {
                Debug.Log($"GetPromptHistoryAsync({promptId}) response: {response.DataAsText}");
                var obj = JsonConvert.DeserializeObject<Dictionary<string, PromptHistory>>(response.DataAsText);

                if (obj.Count < 1)
                {
                    source.SetResult(null);
                }
                else
                {
                    source.TrySetResult(obj.First().Value);
                }
            });

        return await source.Task;
    }

    public void SendPrompt(ComfyPrompt comfyPrompt, Action<PostPromptResponse> onSuccess = null)
    {
        var prompt = comfyPrompt.GetPrompt();

        var request = GenerateRequest($"/prompt", HTTPMethods.Post,
            response =>
            {
                var obj = JsonConvert.DeserializeObject<PostPromptResponse>(response.DataAsText);
                onSuccess?.Invoke(obj);
            }, prompt);
    }
    
    public async Task<PostPromptResponse> SendPromptAsync(string jsonData)
    {
        var source = new TaskCompletionSource<PostPromptResponse>();
        
        GenerateRequest($"/prompt", HTTPMethods.Post,
            response =>
            {
                Debug.Log($"SendPromptAsync response: {response.DataAsText}");
                var obj = JsonConvert.DeserializeObject<PostPromptResponse>(response.DataAsText);
                source.TrySetResult(obj);
            }, jsonData);

        return await source.Task;
    }

    #endregion PUBLIC

    HTTPRequest GenerateRequest(string uriSuffix, HTTPMethods method = HTTPMethods.Get, Action<HTTPResponse> onSuccess = null, string jsonData = null)
    {
        var uri = new Uri(_baseUrl + uriSuffix);
        var request = new HTTPRequest(uri, method, GetReturnDelegate(onSuccess));
        var creds = new Credentials(AuthenticationTypes.Basic, _user, _pass);

        if (jsonData != null)
        {
            request.RawData = Encoding.ASCII.GetBytes(jsonData);
            request.AddHeader("Content-Type", "application/json");
        }

        request.Credentials = creds;
        request.Send();
        
        return request;
    }
    
    OnRequestFinishedDelegate GetReturnDelegate(Action<HTTPResponse> onSuccess, Action<HTTPResponse> onError = null)
    {
        OnRequestFinishedDelegate d = (request, response) =>
        {
            if (response == null)
            {
                Debug.LogWarning($"Request failed - 'response' was null!\nURI: '{request.CurrentUri}'");
                onError?.Invoke(response);
            }
            else if (response.IsSuccess == false)
            {
                Debug.LogWarning($"Request failed, response returned:\nURI: '{request.CurrentUri}'\nStatus Code: {response.StatusCode}\nMessage: {response.Message}\nResponse Data:\n{response.DataAsText}");
                onError?.Invoke(response);
            }
            else
            {
                onSuccess?.Invoke(response);
            }
        };

        return d;
    }
    
    static string MimeTypeForFormat(ComfyImageFormat format)
    {
        return format switch
        {
            ComfyImageFormat.Png => "image/png",
            ComfyImageFormat.Jpeg => "image/jpeg",
        };
    }
    
    static string FileExtensionForFormat(ComfyImageFormat format)
    {
        return format switch
        {
            ComfyImageFormat.Png => "png",
            ComfyImageFormat.Jpeg => "jpg",
        };
    }

    public class PostPromptResponse
    {
        public string prompt_id;
        public int number;
    }

    public class PromptHistory
    {
        public class PromptOutputs
        {
            public class PromptImage
            {
                public string filename, subfolder, type;

                public string Path
                {
                    get
                    {
                        if (string.IsNullOrWhiteSpace(subfolder))
                            return filename;

                        return $"{subfolder}/{filename}";
                    }
                }
            }

            public PromptImage[] images;
        }

        public Dictionary<string, PromptOutputs> outputs;
    }
}