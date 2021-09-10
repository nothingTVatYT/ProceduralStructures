using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {

    public class Triangle : IEquatable<Triangle> {
        public Vertex v0, v1, v2;
        public Vector2 uv0, uv1, uv2;
        public Vector3 normal {
            get {
                Vector3 ab = v1.pos - v0.pos;
                Vector3 ac = v2.pos - v0.pos;
                return Vector3.Cross(ab, ac).normalized;
            }
        }

        public Vector3 center {
            get {
                return (v0.pos + v1.pos + v2.pos) / 3;
            }
        }

        public float area {
            get {
                return Vector3.Cross(v1.pos-v0.pos, v2.pos-v0.pos).magnitude/2;
            }
        }
        
        public Triangle(Vertex v0, Vertex v1, Vertex v2) {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v0.SetTriangle(this);
            this.v1.SetTriangle(this);
            this.v2.SetTriangle(this);
        }

        public override int GetHashCode()
        {
            return v0.GetHashCode() + v1.GetHashCode() + v2.GetHashCode();
        }

        public void FlipNormal() {
            Vertex v = v1;
            v1 = v2;
            v2 = v;
        }

        public Vertex[] GetVertices() {
            return new Vertex[] { v0, v1, v2 };
        }

        public void SetVertex(int idx, Vertex v) {
            RemoveTriangleLinks();
            switch (idx) {
                case 0: v0 = v; break;
                case 1: v1 = v; break;
                case 2: v2 = v; break;
                default: ResetTriangleLinks(); throw new System.InvalidOperationException("Index must be 0, 1 or 2 in SetVertex");
            }
            ResetTriangleLinks();
        }

        public bool Equals(Triangle other) {
            return GetCommonVertices(other) == 3;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Triangle) return Equals(obj as Triangle);
            return false;
        }

        public Triangle GetDuplicate() {
            Triangle other = v0.triangles.Find(t => t != this && Equals(t));
            if (other != null) return other;
            other = v1.triangles.Find(t => t != this && Equals(t));
            if (other != null) return other;
            other = v2.triangles.Find(t => t != this && Equals(t));
            if (other != null) return other;
            return null;
        }

        public bool PointIsAbove(Vector3 point) {
            // barycentric coordinate check
            // see https://gamedev.stackexchange.com/questions/28781/easy-way-to-project-point-onto-triangle-or-plane
            Vector3 u = v1.pos-v0.pos;
            Vector3 v = v2.pos-v0.pos;
            Vector3 n = Vector3.Cross(u, v);
            Vector3 w = point - v0.pos;
            float gamma = Vector3.Dot(Vector3.Cross(u, w), n) / Vector3.Dot(n, n);
            float beta = Vector3.Dot(Vector3.Cross(w, v), n) / Vector3.Dot(n, n);
            float alpha = 1 - gamma - beta;
            return ((0 <= alpha) && (alpha <= 1) &&
                (0 <= beta)  && (beta  <= 1) &&
                (0 <= gamma) && (gamma <= 1));
        }

        /// <summary>checks whether any vertex is contained by this triangle with a tolerance(+-) on the normal</summary>
        public bool ContainsAnyVertex(IEnumerable<Vertex> other, float heightTolerance = 0.2f) {
            Vector3 n = normal;
            foreach (Vertex v in other) {
                if (v0 == v || v1 == v || v2 == v) {
                    continue;
                }
                if (Vector3.Dot(Vector3.Cross(n, v1.pos-v0.pos), v.pos-v0.pos) < 0) {
                    continue;
                }
                if (Vector3.Dot(Vector3.Cross(n, v2.pos-v1.pos), v.pos-v1.pos) < 0) {
                    continue;
                }
                if (Vector3.Dot(Vector3.Cross(n, v0.pos-v2.pos), v.pos-v2.pos) < 0) {
                    continue;
                }
                if (Mathf.Abs(Vector3.Dot((v.pos-v0.pos), normal)) < heightTolerance) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>checks whether the point is in the half space above this triangle</summary>
        public bool FacesPoint(Vector3 point) {
            return Vector3.Dot(normal, point-v0.pos) > 0; //Face.Epsilon;
        }

        public static bool FacesPoint(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 point) {
            return Vector3.Dot(Vector3.Cross(v1-v0, v2-v0), point-v0) > 0; //Face.Epsilon;
        }

        public int GetCommonVertices(Triangle other) {
            int commonVertices = 0;
            foreach (Vertex v in GetVertices()) {
                foreach (Vertex w in other.GetVertices()) {
                    if (v.Equals(w)) {
                        commonVertices++;
                    }
                }
            }
            return commonVertices;
        }

        public bool SharesEdgeWith(Triangle other) {
            return GetCommonVertices(other) == 2;
        }

        public List<TEdge> GetNonManifoldEdges() {
            List<TEdge> result = new List<TEdge>();
            List<Triangle> v0v1 = v0.triangles.FindAll(t => v1.triangles.Contains(t));
            List<Triangle> v1v2 = v1.triangles.FindAll(t => v2.triangles.Contains(t));
            List<Triangle> v2v0 = v2.triangles.FindAll(t => v0.triangles.Contains(t));
            if (v0v1.Count == 1) {
                result.Add(new TEdge(v0, v1, this));
            }
            if (v1v2.Count == 1) {
                result.Add(new TEdge(v1, v2, this));
            }
            if (v2v0.Count == 1) {
                result.Add(new TEdge(v2, v0, this));
            }
            return result;
        }

        public float MaxAngle() {
            float phi = Vector3.Angle(v1.pos-v0.pos, v2.pos-v0.pos);
            phi = Mathf.Max(phi, Vector3.Angle(v2.pos-v1.pos, v0.pos-v1.pos));
            phi = Mathf.Max(phi, Vector3.Angle(v1.pos-v2.pos, v0.pos-v2.pos));
            return phi;
        }
        
        public void RemoveTriangleLinks() {
            v0.triangles.Remove(this);
            v1.triangles.Remove(this);
            v2.triangles.Remove(this);
        }

        public void ResetTriangleLinks() {
            v0.SetTriangle(this);
            v1.SetTriangle(this);
            v2.SetTriangle(this);
        }

        public List<Triangle> GetAdjacentTriangles() {
            HashSet<Triangle> result = new HashSet<Triangle>();
            v0.triangles.ForEach(t => result.Add(t));
            v1.triangles.ForEach(t => result.Add(t));
            v2.triangles.ForEach(t => result.Add(t));
            result.Remove(this);
            return new List<Triangle>(result);
        }

        public List<Triangle> GetAdjacentPlanarTriangles(float tolerance = 1f) {
            HashSet<Triangle> result = new HashSet<Triangle>();
            Vector3 n = normal;
            v0.triangles.ForEach(t => result.Add(t));
            v1.triangles.ForEach(t => result.Add(t));
            v2.triangles.ForEach(t => result.Add(t));
            result.Remove(this);
            result.RemoveWhere( t => Vector3.Angle(t.normal, n) > tolerance);
            return new List<Triangle>(result);
        }

        public Triangle GetNearestAdjacentTriangleByNormal(float tolerance = 5f) {
            List<Triangle> neighbors = GetAdjacentTriangles();
            if (neighbors.Count == 0) return null;
            int bestMatch = 0;
            float minAngle = float.MaxValue;
            for (int i = 0; i < neighbors.Count; i++) {
                float angle = Vector3.Angle(neighbors[i].normal, normal);
                if (angle < minAngle) {
                    minAngle = angle;
                    bestMatch = i;
                }
            }
            if (minAngle <= tolerance)
                return neighbors[bestMatch];
            return null;
        }

        public void SetUVProjected(float uvScale) {
            Vector3 n = normal;
            float dlr = Mathf.Abs(Vector3.Dot(n, Vector3.left));
            float dfb = Mathf.Abs(Vector3.Dot(n, Vector3.back));
            float dud = Mathf.Abs(Vector3.Dot(n, Vector3.up));
            //float dlr = Vector3.Dot(n, Vector3.left);
            //float dfb = Vector3.Dot(n, Vector3.back);
            //float dud = Vector3.Dot(n, Vector3.up);
            Vector3 a = v0.pos;
            Vector3 b = v1.pos;
            Vector3 c = v2.pos;
            uv0 = new Vector2((dlr*a.z + dfb*a.x + dud*a.x) * uvScale, (dlr*a.y + dfb*a.y + dud*a.z) * uvScale);
            uv1 = new Vector2((dlr*b.z + dfb*b.x + dud*b.x) * uvScale, (dlr*b.y + dfb*b.y + dud*b.z) * uvScale);
            uv2 = new Vector2((dlr*c.z + dfb*c.x + dud*c.x) * uvScale, (dlr*c.y + dfb*c.y + dud*c.z) * uvScale);
        }

        public Triangle SetUVCylinderProjection(Vector3 center, Vector3 direction, float uOffset, float uvScale) {
            uv0 = UVCylinderProjection(v0.pos, center, direction, uOffset, uvScale);
            uv1 = UVCylinderProjection(v1.pos, center, direction, uOffset, uvScale);
            uv2 = UVCylinderProjection(v2.pos, center, direction, uOffset, uvScale);
            return this;
        }

        private static Vector2 UVCylinderProjection(Vector3 vertex, Vector3 center, Vector3 direction, float uOffset, float uvScale) {
            float dot = Vector3.Dot(vertex - center, direction);
            Vector3 ms = center + dot*direction;
            Vector3 down = Vector3.down + Vector3.Cross(direction, Vector3.up)*0.05f;
            // this should be replaced with a v scale setting
            float r = 5;
            float u = (dot+uOffset) * uvScale;
            float v = Vector3.Angle(vertex - ms, down) / 180f * r * uvScale;
            //float v = Mathf.Atan2((vertex-ms).y, (vertex-ms).x) / 180f * r * uvScale;
            return new Vector2(u, v);
        }

        public bool RayHit(Vector3 origin, Vector3 direction, bool ignoreBack, out bool fromBack, out Vector3 intersection) {
            Vector3 edge1 = v1.pos-v0.pos;
            Vector3 edge2 = v2.pos-v0.pos;
            fromBack = false;
            intersection = Vector3.zero;
            float rayHitLength;
            if (GeometryTools.RayHitTriangle(origin, direction, v0.pos, v1.pos, v2.pos, out intersection, out rayHitLength)) {
                return true;
            }
            if (!ignoreBack) {
                fromBack = true;
                if (GeometryTools.RayHitTriangle(origin, direction, v0.pos, v2.pos, v1.pos, out intersection, out rayHitLength)) {
                    return true;
                }
            }
            return false;
        }

        public bool EdgeIntersection(Vector3 a, Vector3 b, out Vector3 intersection) {
            float rayHitLength;
            if (MeshObject.SameInTolerance(v0.pos, a) || MeshObject.SameInTolerance(v1.pos, a) || MeshObject.SameInTolerance(v2.pos, a)) {
                intersection = a;
                return true;
            }
            if (MeshObject.SameInTolerance(v0.pos, b) || MeshObject.SameInTolerance(v1.pos, b) || MeshObject.SameInTolerance(v2.pos, b)) {
                intersection = b;
                return true;
            }
            if (GeometryTools.PointOnEdge(v0.pos, a, b, out rayHitLength)) {
                intersection = v0.pos;
                return true;
            }
            if (GeometryTools.PointOnEdge(v1.pos, a, b, out rayHitLength)) {
                intersection = v1.pos;
                return true;
            }
            if (GeometryTools.PointOnEdge(v2.pos, a, b, out rayHitLength)) {
                intersection = v2.pos;
                return true;
            }
            if (GeometryTools.RayHitTriangle(a, b-a, v0.pos, v1.pos, v2.pos, out intersection, out rayHitLength)) {
                if (rayHitLength <= 1) {
                    return true;
                }
            }
            if (GeometryTools.RayHitTriangle(a, b-a, v0.pos, v2.pos, v1.pos, out intersection, out rayHitLength)) {
                if (rayHitLength <= 1) {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("T{0}[{1},{2},{3}]", GetHashCode(), v0, v1, v2);
        }
    }
}
