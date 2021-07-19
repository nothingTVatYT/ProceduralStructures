using UnityEngine;
using UnityEditor;
using ProceduralStructures;

[CustomEditor(typeof(HouseBuilder))]
public class HouseBuilderEditor : Editor {

    private HouseBuilder houseBuilder;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        houseBuilder = (HouseBuilder)target;
        if (GUILayout.Button("Rebuild")) {
            ProceduralHouse p = new ProceduralHouse();
            p.RebuildHouse(houseBuilder.houseDefinition, houseBuilder.gameObject);
        }
        if (GUILayout.Button("Rebuild with interior")) {
            ProceduralHouse p = new ProceduralHouse();
            p.RebuildHouseWithInterior(houseBuilder.houseDefinition, houseBuilder.gameObject);
        }
        if (GUILayout.Button("Remove Meshes")) {
            new Building().ClearMeshes(houseBuilder.gameObject);
        }
    }
}
