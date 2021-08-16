using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AddressLabel))]
public class AddressLabelEditor : Editor {
    public override void OnInspectorGUI() {
        AddressLabel label = target as AddressLabel;
        DrawDefaultInspector();
        if (GUILayout.Button("Fetch street name from nearest house")) {
            label.SuggestStreetName();
        }
    }
}