using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuilderTest2))]
public class Buildertest2Editor : Editor
{
    public override void OnInspectorGUI() {
        BuilderTest2 test = target as BuilderTest2;
        DrawDefaultInspector();
        if (GUILayout.Button("init")) {
            test.Initialize();
            test.DrawTest();
        }
        if (GUILayout.Button("cut")) {
            test.Initialize();
            test.ClampToPlane();
            test.DrawTest();
        }
    }
}
