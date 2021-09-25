using System;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace ProceduralStructures {
    [CustomEditor(typeof(CaveBuilderComponent))]
    public class CaveBuilderEditor : Editor {

        public override void OnInspectorGUI() {
            CaveBuilderComponent caveBuilder = target as CaveBuilderComponent;
            DrawDefaultInspector();
            if (GUILayout.Button("Update")) {
                if (caveBuilder.generatedMeshParent == null) {
                    GameObject go = new GameObject("generatedMesh");
                    go.transform.parent = caveBuilder.gameObject.transform;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                    go.isStatic = caveBuilder.gameObject.isStatic;
                    caveBuilder.generatedMeshParent = go;
                }
                caveBuilder.UpdateWayPoints();
                ProceduralStructure ps = new ProceduralStructure();
                ps.RebuildCave(caveBuilder.caveDefinition, caveBuilder.generatedMeshParent);
                EditorUtilities.CreateSecondaryUV(caveBuilder.generatedMeshParent.GetComponentsInChildren<MeshFilter>());
            }
        }
    }
}