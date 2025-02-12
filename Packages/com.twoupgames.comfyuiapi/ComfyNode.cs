using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

public enum ComfyNodeClassType
{
    LoadImage,
    SaveImage,
    PreviewImage,
    CLIPTextEncode,
    IPAdapterStyleComposition,
    IPAdapterUnifiedLoader,
    ImageInvert,
    ControlNetApply,
    ControlNetLoader,
    VAEDecode,
    KSampler,
    EmptyLatentImage,
    CheckpointLoaderSimple,
    Canvas_Tab
}

[Serializable]
public class ComfyNode
{
    [JsonProperty("class_type")]
    public string ClassType { get; private set; }
    
    [JsonProperty("inputs")]
    public JObject Inputs { get; private set; }

    [JsonProperty("_meta")]
    public ComfyNodeMeta Meta { get; private set; }

    public class ComfyNodeMeta
    {
        [JsonProperty("title")]
        public string Title { get; private set; }
    }
}
