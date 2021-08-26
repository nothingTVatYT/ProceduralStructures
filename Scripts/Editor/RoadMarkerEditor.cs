using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadMarker))][CanEditMultipleObjects]
public class RoadMarkerEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Drop to terrain")) {
            if (targets != null) {
                foreach (Object obj in targets) {
                    DropToTerrain(obj as RoadMarker);
                }
            } else {
                RoadMarker marker = (RoadMarker)target;
                DropToTerrain(marker);
            }
        }
    }

    public void DropToTerrain(RoadMarker marker) {
        Undo.RecordObject(marker.gameObject.transform, "Drop to terrain");
        Vector3 pos = marker.gameObject.transform.position;
        pos.y = Terrain.activeTerrain.SampleHeight(pos) + 1;
        marker.gameObject.transform.position = pos;
    }
}
