using System;
using UnityEngine;

namespace ProceduralStructures {
    [Serializable]
    public class WayPoint {
        public string name;
        public Vector3 position;
        public float scaleWidth;
        public float scaleHeight;
        public WayPoint(Vector3 position, float scaleWidth = 1, float scaleHeight = 1) {
            this.position = position;
            this.scaleWidth = scaleWidth;
            this.scaleHeight = scaleHeight;
        }
    }
}