using System;
using System.Collections;

using UnityEngine;

[RequireComponent(typeof(WalkAroundGraph))]
public class WalkAroundGraph_AutoPlay : MonoBehaviour 
{
    [SerializeField]
    WalkAroundGraph _walkComponent;

    [SerializeField]
    RectTransform _relativeTo;

    [SerializeField]
    TextAsset[] _svgFiles;

    [SerializeField]
    [Range(0.1f, 10f)]
    float _endDelay = 2;
    /*
    [SerializeField]
    [Range(1, 2160)]
    float _pixelSize = 1080;
    */
    SVGPointParser _parser;

    void OnValidate()
    {
        _walkComponent = GetComponent<WalkAroundGraph>();
    }

    void Start()
    {
        _parser = new SVGPointParser();

        StartCoroutine(LoopThroughAllFiles());
    }

    IEnumerator LoopThroughAllFiles()
    {
        while (enabled)
        {
            foreach (var svgFile in _svgFiles)
            {
                LoadGraph(svgFile);

                yield return _walkComponent.BeginWalk(_parser.Points);

                yield return new WaitForSeconds(_endDelay);
            }
        }
    }

    void LoadGraph(TextAsset svgFile)
    {
        var pxSize = Mathf.Max(_relativeTo.rect.width, _relativeTo.rect.height);

        _parser.Load(svgFile, pxSize);

        _parser.Transform(_relativeTo);
    }
}
