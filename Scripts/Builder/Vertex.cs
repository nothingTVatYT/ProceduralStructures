using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class Vertex {
        public int id;
        public Vector3 pos;
        public List<Triangle> triangles = new List<Triangle>();
        public Vertex(Vector3 pos) {
            this.pos = pos;
            id = GetHashCode();
        }
        public void SetTriangle(Triangle triangle) {
            if (!triangles.Contains(triangle)) {
                triangles.Add(triangle);
            }
        }

        public override string ToString()
        {
            return string.Format("V{3}[{0:F4},{1:F4},{2:F4}]", pos.x, pos.y, pos.z, id);
        }
    }
}
