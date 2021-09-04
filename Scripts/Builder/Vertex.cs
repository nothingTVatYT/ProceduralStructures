using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class Vertex {
        public Vector3 pos;
        public List<Triangle> triangles = new List<Triangle>();
        public Vertex(Vector3 pos) {
            this.pos = pos;
        }
        public void SetTriangle(Triangle triangle) {
            if (!triangles.Contains(triangle)) {
                triangles.Add(triangle);
            }
        }

        public override string ToString()
        {
            return "V[" + pos + "]";
        }
    }
}
