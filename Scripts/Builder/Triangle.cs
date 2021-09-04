using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {

    public class Triangle {
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

        public void FlipNormal() {
            Vertex v = v1;
            v1 = v2;
            v2 = v;
        }

        public Vertex[] GetVertices() {
            return new Vertex[] { v0, v1, v2 };
        }

        public bool FacesPoint(Vector3 point) {
            return Vector3.Dot(normal, point-v0.pos) > Face.Epsilon;
        }

        public static bool FacesPoint(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 point) {
            return Vector3.Dot(Vector3.Cross(v1-v0, v2-v0), point-v0) > 0;
        }

        public bool SharesEdgeWith(Triangle other) {
            int commonVertices = 0;
            foreach (Vertex v in GetVertices()) {
                foreach (Vertex w in other.GetVertices()) {
                    if (v == w) {
                        commonVertices++;
                    }
                }
            }
            return commonVertices == 2;
        }

        public void RemoveTriangleLinks() {
            v0.triangles.Remove(this);
            v1.triangles.Remove(this);
            v2.triangles.Remove(this);
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

        public override string ToString()
        {
            return "T[" + v0 + "," + v1 + "," + v2 + "]";
        }
    }
}
