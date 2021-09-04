using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuilderTest3))]
public class BuildTest3Editor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Rebuild")) {
            BuilderTest3 test3 = target as BuilderTest3;
            test3.Rebuild();
        }
    }
}
