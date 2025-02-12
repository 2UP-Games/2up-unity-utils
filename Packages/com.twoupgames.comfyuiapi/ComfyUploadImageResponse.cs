using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ComfyUploadImageResponse
{
    [JsonProperty("name")]
    public string Name { get; private set; }
    
    [JsonProperty("subfolder")]
    public string Subfolder { get; private set; }

    public string Path => string.IsNullOrWhiteSpace(Subfolder) ? Name : $"{Subfolder}/{Name}";
}
