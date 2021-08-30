using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CaveBuilderComponent))]
public class CaveBuilderEditor : Editor {

    public override void OnInspectorGUI() {
        CaveBuilderComponent caveBuilder = target as CaveBuilderComponent;
        DrawDefaultInspector();
        if (GUILayout.Button("Update")) {
            caveBuilder.UpdateWayPoints();
        }
    }
}
