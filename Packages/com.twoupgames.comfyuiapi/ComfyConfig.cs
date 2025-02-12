using System;

using UnityEngine;

using NaughtyAttributes;

using JetBrains.Annotations;
using System.Linq;

[CreateAssetMenu(fileName = "ComfyConfig_", menuName = "ComfyConfig")]
public class ComfyConfig : ScriptableObject
{
    ComfyApiProvider _api;

    public ComfyApiProvider Api => _api;

    [SerializeField]
    string url, user, pass, downloadImage, _promptId;

    [SerializeField]
    ComfyImageFormat _uploadImageFormat = ComfyImageFormat.Jpeg;
    
    public ComfyImageFormat UploadImageFormat => _uploadImageFormat;
    
    [SerializeField]
    [Range(0f, 1f)]
    public float ControlNetStrength = 0.8f;

    [SerializeField]
    Texture inputImage;

    [SerializeField]
    public Sprite outputImage;

    [SerializeField]
    ComfyPrompt _prompt;

    public ComfyPrompt Prompt => _prompt;

    public void Init()
    {
        _api = new(url, user, pass);
    }
    
    [Button]
    [UsedImplicitly]
    void SendPrompt()
    {
        if (_prompt == null)
        {
            throw new NullReferenceException("_prompt is not set");
        }

        if (_api == null)
            Init();

        _api.SendPrompt(_prompt, x =>
        {
            _promptId = x.prompt_id;
        });
    }

    [Button]
    [UsedImplicitly]
    void GetPromptOutputFilename()
    {
        if (string.IsNullOrWhiteSpace(_promptId))
        {
            throw new NullReferenceException("_prompt is not set");
        }

        if (_api == null)
            Init();

        _api.GetPromptHistory(_promptId, x =>
        {
            downloadImage = x.outputs.First().Value.images[0].filename;
        });
    }

    [Button]
    [UsedImplicitly]
    void DownloadImage()
    {
        if (string.IsNullOrWhiteSpace(downloadImage))
            throw new NullReferenceException("downloadImage is not set");

        if (_api == null)
            Init();

        _api.GetImage(downloadImage, onSuccess: NewOutputImageReceived);
    }

    void NewOutputImageReceived(Sprite newImage)
    {
        outputImage = newImage;
    }
}
