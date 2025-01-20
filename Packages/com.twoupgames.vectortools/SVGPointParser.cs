using System;
using System.IO;

using UnityEngine;
using Unity.VectorGraphics;

using static Unity.VectorGraphics.SVGParser;

public class SVGPointParser
{
    const int _pixelsPerUnit = 1024;

    SceneInfo _sceneInfo;
    BezierPathSegment[] _segments;
    Vector3[] _points;

    public SceneInfo SceneInfo => _sceneInfo;
    public BezierPathSegment[] Segments => _segments;
    public Vector3[] Points => _points;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="svgFile"></param>
    /// <param name="pxSize">How many pixels across should the points be scaled to? I.e. if your UI reference size is 1920x1080 and you want the resulting data cloud to take up the maximum possible screen space, set this to 1080.0f.</param>
    public void Load(TextAsset svgFile, float pxSize)
    {
        var stringReader = new StringReader(svgFile.text);

        _sceneInfo = SVGParser.ImportSVG(stringReader, pixelsPerUnit: _pixelsPerUnit);

        _segments = LoadSegments(_sceneInfo);

        _segments = NormalizeSegments(_segments, pxSize);

        _points = InitialisePoints(_sceneInfo, _segments);
    }

    /// <summary>
    /// Transforms the point cloud into the space of the specified transform, accounting for position, rotation and scale.
    /// </summary>
    public void Transform(Transform relativeTo)
    {
        _points = TransformPointsToSpace(_points, relativeTo);
    }

    BezierPathSegment[] LoadSegments(SceneInfo sceneInfo)
    {
        Scene scene = sceneInfo.Scene;
        SceneNode rootNode = scene.Root;

        var activeNode = rootNode;

        while (activeNode != null)
        {
            if (activeNode.Shapes != null && activeNode.Shapes.Count > 0)
            {
                // This node has a shape, we can load segments from here
                BezierPathSegment[] segments = activeNode.Shapes[0].Contours[0].Segments;

                // Apply the graph's root transform
                var transformedSegments = VectorUtils.TransformBezierPath(segments, sceneInfo.Scene.Root.Transform);

                return transformedSegments;
            }
            else if (activeNode.Children == null || activeNode.Children.Count == 0)
            {
                // Failure point - we've reached a node with no shapes and no children, so there's nowhere else to go
                Debug.LogErrorFormat("Can't load segments: Reached node with no children");
                return null;
            }
            else
            {
                // This node doesn't have shapes but does have children, we can step down a level
                activeNode = activeNode.Children[0];
            }
        }

        // Failure point - no node was ever not null
        Debug.LogErrorFormat("Can't load segments: Root node is null");
        return null;
    }

    /// <summary>
    /// Transforms segments from unit space (0 to 1) to normalized space (-1 to 1)
    /// </summary>
    BezierPathSegment[] NormalizeSegments(BezierPathSegment[] segments, float pxSize)
    {
        segments = VectorUtils.TransformBezierPath(segments, -Vector2.one, 0, Vector2.one * 2);

        // Perform rotation as a discrete second step so it pivots around the new center
        segments = VectorUtils.TransformBezierPath(segments, Vector2.zero, Mathf.PI, Vector2.one);

        segments = VectorUtils.TransformBezierPath(segments, Vector2.zero, 0, Vector2.one * (pxSize / 2));

        return segments;
    }

    /// <summary>
    /// Creates a Vector3 array out of the P0 elements of a BezierPathSegment array
    /// </summary>
    Vector3[] InitialisePoints(SceneInfo sceneInfo, BezierPathSegment[] segments)
    {
        Vector3[] points = new Vector3[segments.Length];

        for (int i = 0; i < segments.Length; i++)
        {
            points[i] = segments[i].P0;
        }

        return points;
    }

    Vector3[] TransformPointsToSpace(Vector3[] points, Transform space)
    {
        Span<Vector3> span = new Span<Vector3>(points);
        space.TransformPoints(span);
        points = span.ToArray();
        return points;
    }
}
