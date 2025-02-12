using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class prompt_injection
{
    string testJson =
@"{
  ""69"": {
    ""inputs"": {
      ""strength"": 0.8,
      ""conditioning"": [
        ""62"",
        0
      ],
      ""control_net"": [
        ""68"",
        0
      ],
      ""image"": [
        ""71"",
        0
      ]
    },
    ""class_type"": ""ControlNetApply"",
    ""_meta"": {
      ""title"": ""Apply ControlNet""
    }
  }
}";

    [Test]
    public void inject_control_net_strength()
    {
        testJson = testJson.InjectControlNetStrength(0.2f);

        string expectedResult =
@"{
  ""69"": {
    ""inputs"": {
      ""strength"":0.2,
      ""conditioning"": [
        ""62"",
        0
      ],
      ""control_net"": [
        ""68"",
        0
      ],
      ""image"": [
        ""71"",
        0
      ]
    },
    ""class_type"": ""ControlNetApply"",
    ""_meta"": {
      ""title"": ""Apply ControlNet""
    }
  }
}";

        Assert.AreEqual(expectedResult, testJson);
    }
}
