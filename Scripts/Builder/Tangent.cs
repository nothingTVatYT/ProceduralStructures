using System;
using UnityEngine;

namespace ProceduralStructures {
    [Serializable]
    public class Tangent {
        public Vector3 position;
        public Vector3 direction;
        public float relativePosition;
        public Tangent(Vector3 position, Vector3 direction, float relPos = 0) {
            this.position = position;
            this.direction = direction;
            this.relativePosition = relPos;
        }
    }
}