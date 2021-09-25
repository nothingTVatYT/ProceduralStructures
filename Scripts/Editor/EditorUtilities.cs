using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralStructures {
    public class EditorUtilities {

        public static void CreateSecondaryUV(MeshFilter meshFilter) {
            Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
        }
        public static void CreateSecondaryUV(MeshFilter[] meshFilters) {
            foreach (MeshFilter meshFilter in meshFilters)
                CreateSecondaryUV(meshFilter);
        }
    }
}