using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CaveDefinition {

    public enum Shape { Tunnel, O_Shaped }
    public List<Vector3> wayPoints;
    [Tooltip("Shape of the cave")]
    public Shape crosscutShape;
    [Tooltip("unscaled height of the cave")]
    public float baseHeight;
    [Tooltip("unscaled width of the cave")]
    public float baseWidth;
    [Tooltip("average distance between vertices along the curve")]
    [Range(0.1f, 10f)]
    public float uResolution;

    private BezierSpline spline;

    public bool IsValid() {
        return wayPoints != null && wayPoints.Count >= 4;
    }

    public IEnumerable<Vector3> GetVertices() {
        spline = new BezierSpline(wayPoints);
        float t = 0;
        if (uResolution < 0.01f) uResolution = 1;
        float stepSize = uResolution / spline.EstimatedLength;
        while (t < (1f + stepSize)) {
            Vector3 v = GetVertex(t);
            t += stepSize;
            yield return v;
        }
    }

    Vector3 GetVertex(float t) {
        return spline.GetPoint(t);
    }
}
