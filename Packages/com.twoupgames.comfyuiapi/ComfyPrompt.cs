using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = "ComfyPrompt_", menuName = "ComfyPrompt")]
public class ComfyPrompt : ScriptableObject
{
    [SerializeField]
    public string[] keys = { "{{POS_PROMPT}}", "{{NEG_PROMPT}}", "{{OUTPUT_PREFIX}}" };

    [SerializeField]
    public string[] values = { "three ducks", "water", "duck" };

    [SerializeField]
    [TextArea(3, 50)]
    string _prompt;

    [Button]
    public void TestPrintPrompt()
    {
        Debug.Log(GetPrompt());
    }

    Dictionary<string, string> GetDictionary()
    {
        var parameters = new Dictionary<string, string>();

        for (int i = 0; i < keys.Length; i++)
        {
            parameters.Add(keys[i], values[i]);
        }

        return parameters;
    }

    public string GetPrompt()
    {
        var prompt = _prompt;
        var parameters = GetDictionary();

        foreach (var param in parameters)
        {
            var index = prompt.IndexOf(param.Key);

            if (index > -1)
            {
                prompt = prompt.Replace(param.Key, param.Value);
            }
        }
        
        // For whatever reason, everything in the JSON needs to be parented under a "prompt" key.
        // This key is not present in the "API format" JSON export from the Comfy interface.
        // It's annoying and error-prone to have to manually put in the "prompt" key to the exported text.
        // Instead, now, if the data isn't parented under "prompt", make it so.
        // I.e. this works with both old-style (hand-edited) and new-style (raw export) JSON.
        
        var jsonTree = JsonConvert.DeserializeObject<Dictionary<string,object>>(prompt);

        if (jsonTree.ContainsKey("prompt") == false)
        {
            jsonTree = new Dictionary<string, object> { { "prompt", jsonTree } };
        }
        
        var prettyPrinted = JsonConvert.SerializeObject(jsonTree, Formatting.Indented);

        return prettyPrinted;
    }
}
