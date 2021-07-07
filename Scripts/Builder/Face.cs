using System.Collections;
using System.Collections.Generic;
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
        }
        public Face(Vector3 a, Vector3 b, Vector3 c) {
            this.a = a;
            this.b = b;
            this.c = c;
            isTriangle = true;
        }
        public Vector3 a,b,c,d;
        public Vector2 uvA,uvB,uvC,uvD;
        public Vector3 normal { get { return Vector3.Cross(b-a, d-a);} }
        public bool isTriangle = false;
        public int tags = 0;
        public Vector3[] GetVertices() {
            return new Vector3[] { a, b, c, d };
        }
        public Vector3[] GetVerticesCCW() {
            return new Vector3[] { a, d, c, b };
        }
        public override string ToString() {
            return "F(" + a +"," + b + "," + c + "," + d + ")";
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