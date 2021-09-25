using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralStructures;

[CustomEditor(typeof(WallBuilder))]
public class WallBuilderEditor : Editor
{
    private WallBuilder wallBuilder;
    private WallDefinition wall;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        wallBuilder = (WallBuilder)target;
        wall = wallBuilder.wall;
        if (GUILayout.Button("Rebuild")) {
            wallBuilder.UpdatePoints();
            ProceduralStructure proc = new ProceduralStructure();
            proc.RebuildWall(wallBuilder.wall, wallBuilder.gameObject);
            EditorUtilities.CreateSecondaryUV(wallBuilder.gameObject.GetComponentsInChildren<MeshFilter>());
        }
        if (GUILayout.Button("Ground markers")) {
            foreach (Transform t in wall.points) {
                float y = Terrain.activeTerrain.SampleHeight(t.position);
                Vector3 groundPosition = new Vector3(t.position.x, y, t.position.z);
                t.position = groundPosition;
            }
        }
    }
}
