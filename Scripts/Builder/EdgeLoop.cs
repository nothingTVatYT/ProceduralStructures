using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProceduralStructures {
    public class EdgeLoop : IEquatable<EdgeLoop> {
        List<Vertex> vertices;
        int hashcode;

        public ReadOnlyCollection<Vertex> Vertices {
            get { return vertices.AsReadOnly(); }
        }

        public int Count { get { return vertices != null ? vertices.Count : 0; }}

        /// <summary>This is an unordered edge loop, i.e. vertices in the opposite order is considered the same</summary>
        /// The list of vertices is copied because it's reordered and could be reversed
        public EdgeLoop(List<Vertex> vertices) {
            this.vertices = new List<Vertex>(vertices);
            ReorderList();
            CalculateHashCode();
        }

        void ReorderList() {
            int idxFirst = 0;
            float xyz = float.MaxValue;
            for (int i = 0; i < vertices.Count; i++) {
                Vertex v = vertices[i];
                float sum = v.pos.x + v.pos.y + v.pos.z;
                if (sum < xyz) {
                    xyz = sum;
                    idxFirst = i;
                }
            }
            CircularReadonlyList<Vertex> c = new CircularReadonlyList<Vertex>(vertices);
            c.indexOffset = idxFirst;
            if (c.Count > 1) {
                if (c[-1].id > c[1].id) {
                    c.Reverse();
                }
            }
            // copy the list from the circular view using the index operator to get the specified order
            List<Vertex> reordered = new List<Vertex>(c.Count);
            for (int i = 0; i < c.Count; i++) {
                reordered.Add(c[i]);
            }
            vertices = reordered;
        }

        void CalculateHashCode() {
            int hash = 0;
            foreach (Vertex v in vertices) {
                hash = (hash << 1) + v.GetHashCode();
            }
            this.hashcode = hash;
        }

        public override int GetHashCode() {
            return hashcode;
        }

        bool ItemsMatch(EdgeLoop other) {
            if (this.Count != other.Count) {
                return false;
            }
            for (int i = 0; i < vertices.Count; i++) {
                if (vertices[i] != other.vertices[i]) {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj) {
            return obj is EdgeLoop && Equals(obj as EdgeLoop);
        }

        public bool Equals(EdgeLoop other) {
            return GetHashCode() == other.GetHashCode() && ItemsMatch(other);
        }
    }
}