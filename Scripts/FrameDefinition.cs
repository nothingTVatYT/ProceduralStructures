using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    [Serializable]
    public class FrameDefinition {
        [Serializable]
        public class Edge {
            public int a;
            public int b;
            public Edge(int a, int b) {
                this.a = a;
                this.b = b;
            }
            public override int GetHashCode() {
                if (a < b) return a + 16383*b;
                return b + 16383*a;
            }
            public override bool Equals(object obj) {
                if (obj is Edge) {
                    Edge other = obj as Edge;
                    return (this.a == other.a && this.b == other.b) || (this.b == other.a && this.a == other.b);
                }
                return false;
            }
            public override string ToString()
            {
                return string.Format("E[" + a + "," + b + "]");
            }
        }
        public List<Vector3> points;
        public List<Edge> edges;
    }
}