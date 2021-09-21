using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class Vertex : IEquatable<Vertex> {
        public int id;
        public Vector3 pos;
        public List<Triangle> triangles = new List<Triangle>();
        public List<Vertex> connected = new List<Vertex>();
        public Vertex(Vector3 pos) {
            this.pos = pos;
            id = GetHashCode();
        }
        public void SetTriangle(Triangle triangle) {
            if (!triangles.Contains(triangle)) {
                triangles.Add(triangle);
            }
        }
        public void SetConnected(Vertex v) {
            if (!connected.Contains(v)) {
                connected.Add(v);
            }
            if (!v.connected.Contains(this)) {
                v.connected.Add(this);
            }
        }

        public void Unlink(Vertex v) {
            v.connected.Remove(this);
            connected.Remove(v);
        }

        public bool Equals(Vertex obj) {
            return MeshObject.SameInTolerance(pos, obj.pos);
        }

        public override int GetHashCode() {
            return pos.x.GetHashCode() + 3 * pos.y.GetHashCode() + 5 * pos.z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Vertex) return this.Equals(obj as Vertex);
            return false;
        }

        public override string ToString()
        {
            return string.Format("V{3}[{0:F4},{1:F4},{2:F4}]", pos.x, pos.y, pos.z, id);
        }
    }
}
