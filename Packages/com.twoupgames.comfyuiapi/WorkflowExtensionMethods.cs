using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class WorkflowExtensionMethods
{
    public static string ToPrompt(this TextAsset textAsset, 
        string designImagePath, 
        string materialImagePath, 
        string positivePrompt = "", 
        float controlNetStrength = 0.8f)
    {
        var workflowObject = JsonConvert.DeserializeObject<Dictionary<string, ComfyNode>>(textAsset.text);

        List<ComfyNode> comfyNodes = new List<ComfyNode>();

        foreach (var comfyNode in workflowObject.Values)
        {
            if (comfyNode.ClassType == nameof(ComfyNodeClassType.LoadImage))
            {
                comfyNodes.Add(comfyNode);
            }
            else if (comfyNode.ClassType == nameof(ComfyNodeClassType.CLIPTextEncode))
            {
                comfyNodes.Add(comfyNode);
            }
        }

        foreach (var comfyNode in comfyNodes)
        {
            var nodeTitle = comfyNode.Meta.Title;
            var fileName = (string)comfyNode.Inputs["image"] ?? string.Empty;
            
            if (nodeTitle.Contains("Material"))
            {
                comfyNode.Inputs["image"] = materialImagePath;
            }
            else 
            if (nodeTitle.Contains("Design"))
            {
                comfyNode.Inputs["image"] = designImagePath;
            }
            else
            if (nodeTitle.Contains("Positive Prompt"))
            {
                comfyNode.Inputs["text"] = positivePrompt;
            }
        }

        string jsonText = JsonConvert.SerializeObject(workflowObject);

        // TODO: Do this via serialized object, like the above
        jsonText = jsonText.InjectControlNetStrength(controlNetStrength);

        return $"{{\"prompt\":{jsonText}}}";
    }

    public static string InjectControlNetStrength(this string jsonText, float controlNetStrength)
    {
        string strengthKey = "\"strength\":";

        int startIndex = jsonText.IndexOf(strengthKey) + strengthKey.Length;

        if (startIndex - strengthKey.Length < 1)
        {
            Debug.LogError($"Tried to InjectControlNetStrength but startIndex was {startIndex}");
            return jsonText;
        }

        int endIndex = -1;

        int i = startIndex + 1;

        while (endIndex == -1)
        {
            char nextChar = jsonText[i];
            if (nextChar == ',')
                endIndex = i;
            else
                i++;
        }

        jsonText = jsonText.Remove(startIndex, endIndex - startIndex);
        jsonText = jsonText.Insert(startIndex, controlNetStrength.ToString());

        return jsonText;
    }
}
