using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HullBuilder))]
[CanEditMultipleObjects]
public class HullBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HullBuilder hull = target as HullBuilder;
        DrawDefaultInspector();
        if (GUILayout.Button("Rebuild")) {
            hull.Rebuild();
        }
    }
}
