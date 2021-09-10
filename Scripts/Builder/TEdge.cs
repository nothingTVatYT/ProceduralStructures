using System;
using System.Collections.Generic;

namespace ProceduralStructures {
    public class TEdge : IEquatable<TEdge> {
        public Vertex a;
        public Vertex b;
        public List<Triangle> triangles = new List<Triangle>();
        public TEdge(Vertex a, Vertex b, params Triangle[] t) {
            this.a = a;
            this.b = b;
            if (t != null) {
                triangles.AddRange(t);
            }
        }

        public bool Equals(TEdge other) {
            return this.a.Equals(other.a) && this.b.Equals(other.b) || this.a.Equals(other.b) && this.b.Equals(other.a);
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() + b.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is TEdge) return Equals(obj as TEdge);
            return false;
        }

        public override string ToString()
        {
            return "E[" + a + "," + b + "]";
        }

        public void Flip() {
            Vertex v = a;
            a = b;
            b = v;
        }
    }
}
