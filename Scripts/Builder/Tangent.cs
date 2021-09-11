using System;
using UnityEngine;

namespace ProceduralStructures {
    [Serializable]
    public class Tangent {
        public Vector3 position;
        public Vector3 direction;
        public float relativePosition;
        public float scaleWidth;
        public float scaleHeight;
        public Tangent(Vector3 position, Vector3 direction, float relPos = 0, float scaleWidth = 1f, float scaleHeight = 1f) {
            this.position = position;
            this.direction = direction;
            this.relativePosition = relPos;
            this.scaleWidth = scaleWidth;
            this.scaleHeight = scaleHeight;
        }

        public static Tangent Lerp(Tangent t1, Tangent t2, float t) {
            Vector3 ipos = Vector3.Lerp(t1.position, t2.position, t);
            Vector3 idirection = Vector3.Lerp(t1.direction, t2.direction, t);
            float irelpos = Mathf.Lerp(t1.relativePosition, t2.relativePosition, t);
            float iscaleWidth = Mathf.Lerp(t1.scaleWidth, t2.scaleWidth, t);
            float iscaleHeight = Mathf.Lerp(t1.scaleHeight, t2.scaleHeight, t);
            return new Tangent(ipos, idirection, irelpos, iscaleWidth, iscaleHeight);
        }

        public override string ToString()
        {
            return string.Format("T[@" + position + "," + direction + "," + relativePosition + "]");
        }
    }
}