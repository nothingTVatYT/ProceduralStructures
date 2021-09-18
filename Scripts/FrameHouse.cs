using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class FrameHouse : MonoBehaviour {
        public GameObject constructionRoot;
        public FrameDefinition frame;
        public float beamThickness = 0.1f;
        public Material beamMaterial;
        public float uvScale = 1f;
    }
}
