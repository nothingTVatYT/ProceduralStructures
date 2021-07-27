using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadMarker))][CanEditMultipleObjects]
public class RoadMarkerEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Drop to terrain")) {
            RoadMarker marker = (RoadMarker)target;
            Undo.RecordObject(marker.gameObject.transform, "Drop to terrain");
            Vector3 pos = marker.gameObject.transform.position;
            pos.y = Terrain.activeTerrain.SampleHeight(pos) + 1;
            marker.gameObject.transform.position = pos;
        }
    }
}
