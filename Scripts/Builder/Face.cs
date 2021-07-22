using UnityEngine;

namespace ProceduralStructures {
    public class Face
    {
        public Face() {}
        public Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            isTriangle = false;
            // initialize UV
            SetUVFront(1, 1);
        }
        public Face(Vector3 a, Vector3 b, Vector3 c) {
            this.a = a;
            this.b = b;
            this.c = c;
            isTriangle = true;
            // initialize UV
            SetUVFront(1, 1);
        }

        public static Face CreateXZPlane(float width, float length) {
            return new Face(
                new Vector3(-width/2, 0, -length/2),
                new Vector3(-width/2, 0, length/2),
                new Vector3(width/2, 0, length/2),
                new Vector3(width/2, 0, -length/2));
        }

        public Vector3 a,b,c,d;
        public Vector2 uvA,uvB,uvC,uvD;
        public Vector3 normal { get { return Vector3.Cross(b-a, (isTriangle ? c : d)-a).normalized;} }
        public bool isTriangle = false;
        public int tags = 0;
        public float sortOrder = 0;
        public Vector3[] GetVertices() {
            return new Vector3[] { a, b, c, d };
        }
        public Vector3[] GetVerticesCCW() {
            return new Vector3[] { a, d, c, b };
        }

        public Face DeepCopy() {
            Face n = new Face();
            n.a = a;
            n.b = b;
            n.c = c;
            n.d = d;
            n.isTriangle = isTriangle;
            n.uvA = uvA;
            n.uvB = uvB;
            n.uvC = uvC;
            n.uvD = uvD;
            n.tags = tags;
            n.sortOrder = sortOrder;
            return n;
        }

        public Face Rotate(Quaternion rot) {
            a = rot * a;
            b = rot * b;
            c = rot * c;
            if (!isTriangle) {
                d = rot * d;
            }
            return this;
        }

        public Face InvertNormals() {
            if (isTriangle) {
                Vector3 t = a;
                a = c;
                c = t;
            } else {
                Vector3 t = b;
                b = d;
                d = t;
            }
            return this;
        }

        public static bool RayHitTriangle(Vector3 origin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 intersection) {
            Vector3 edge1 = v1-v0;
            Vector3 edge2 = v2-v0;
            Vector3 normal = Vector3.Cross(edge1, edge2);
            intersection = Vector3.zero;
            if (Vector3.Dot(normal, origin-v0) < 0) {
                // origin is on the backside
                return false;
            }
            Vector3 h = Vector3.Cross(direction, edge2);
            float aa = Vector3.Dot(edge1, h);
            if (aa > -1e-3 && aa < 1e-3) {
                // ray is parallel to triangle
                return false;
            }
            float ff = 1f/aa;
            Vector3 s = origin - v0;
            float uu = ff * Vector3.Dot(s, h);
            if (uu < 0 || uu > 1)
                return false;
            Vector3 q = Vector3.Cross(s, edge1);
            float vv = ff * Vector3.Dot(direction, q);
            if (vv < 0 || uu+vv > 1)
                return false;
            float tt = ff * Vector3.Dot(edge2, q);
            if (tt > 1e-3) {
                intersection = origin + direction * tt;
                return true;
            }
            return false;
        }

        public bool RayHit(Vector3 origin, Vector3 direction, bool ignoreBack, out bool fromBack, out Vector3 intersection) {
            Vector3 edge1 = b-a;
            Vector3 edge2 = c-a;
            fromBack = false;
            intersection = Vector3.zero;
            if (RayHitTriangle(origin, direction, a, b, c, out intersection)) {
                return true;
            }
            if (!isTriangle && RayHitTriangle(origin, direction, a, c, d, out intersection)) {
                return true;
            }
            if (!ignoreBack) {
                fromBack = true;
                if (RayHitTriangle(origin, direction, a, c, b, out intersection)) {
                    return true;
                }
                if (!isTriangle && RayHitTriangle(origin, direction, a, d, c, out intersection)) {
                    return true;
                }
            }
            return false;
        }

        public override string ToString() {
            // return "F(" + a +"," + b + "," + c + "," + d + ")";
            return string.Format("F(a={0},b={1},c={2},d={3},normal={4},tags={5})", a, b, c, d, normal, tags);
        }
        public bool IsTagged(int tag) {
            return (tags & tag) != 0;
        }
        public void Tag(int tag) {
            tags |= tag;
        }
        public void UnTag(int tag) {
            tags &= ~tag;
        }
        public Face MoveFaceBy(Vector3 direction) {
            a = a + direction;
            b = b + direction;
            c = c + direction;
            d = d + direction;
            return this;
        }

        public void SetUVForSize(float uvScale) {
            float width = Vector3.Distance(a, d);
            float height = Vector3.Distance(a, b);
            SetUVFront(width * uvScale, height * uvScale);
        }

        public void SetUVFront(float width, float height) {
            uvA = new Vector2(0, 0);
            if (isTriangle) {
                uvB = new Vector2(width/2, height);
                uvC = new Vector2(width, 0);
            } else {
                uvB = new Vector2(0, height);
                uvC = new Vector2(width, height);
                uvD = new Vector2(width, 0);
            }
        }
    }
}