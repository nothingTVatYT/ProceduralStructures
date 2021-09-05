using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    [Serializable]
    public class CaveDefinition {
        public enum Shape { Tunnel, O_Shaped }
        [Tooltip("number of times interpolated vertices are added")]
        [Range(0,5)]
        public int shapeSmoothing;
        public MeshObject.Shading shading = MeshObject.Shading.Auto;
        public List<WayPointList> wayPointLists;
        [Tooltip("Shape of the cave")]
        public Shape crosscutShape;
        [Tooltip("unscaled height of the cave")]
        public float baseHeight;
        [Tooltip("unscaled width of the cave")]
        public float baseWidth;
        [Tooltip("average distance between vertices along the curve")]
        [Range(0.1f, 3f)]
        public float uResolution;
        public Material material;
        public float uvScale;
        public bool closeEnds;
        public bool randomizeVertices;
        public Vector3 randomDisplacement;

        private BezierSpline spline;

        public bool IsValid() {
            return wayPointLists != null && wayPointLists.Count > 0 && wayPointLists[0].Count >= 2;
        }

        public IEnumerable<Vector3> GetVertices(WayPointList list) {
            spline = new BezierSpline(list.wayPoints);
            float t = 0;
            if (uResolution < 0.1f) uResolution = 0.1f;
            float stepSize = uResolution / spline.EstimatedLength;
            while (t < (1f + stepSize)) {
                Vector3 v = GetVertex(t);
                t += stepSize;
                yield return v;
            }
        }

        public IEnumerable<Tangent> GetTangents(WayPointList list) {
            spline = new BezierSpline(list.wayPoints);
            float t = 0;
            if (uResolution < 0.1f) uResolution = 0.1f;
            float stepSize = uResolution / spline.EstimatedLength;
            while (t < (1f + stepSize)) {
                Tangent v = GetTangent(t);
                t += stepSize;
                yield return v;
            }
        }

        Vector3 GetVertex(float t) {
            return spline.GetVertex(t);
        }
        Tangent GetTangent(float t) {
            return spline.GetTangent(t);
        }
    }
}