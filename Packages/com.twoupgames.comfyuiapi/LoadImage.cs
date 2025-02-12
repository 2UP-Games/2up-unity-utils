using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class LoadImage
{
    [JsonProperty("input")]
    public NodeInput Input { get; private set; }
}

[Serializable]
public class NodeInput
{
    [JsonProperty("required")]
    public RequiredInput Required { get; private set; }
}

[Serializable]
public class RequiredInput
{
    [JsonProperty("image")]
    public JArray image { get; private set; }
    
    public List<string> Images => image.Where(x => x.Type == JTokenType.Array).SelectMany(x => x.ToObject<List<string>>()).ToList();
}

