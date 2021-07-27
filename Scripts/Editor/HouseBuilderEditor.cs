using UnityEngine;
using UnityEditor;
using ProceduralStructures;

[CustomEditor(typeof(HouseBuilder))]
public class HouseBuilderEditor : Editor {

    private HouseBuilder houseBuilder;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        houseBuilder = (HouseBuilder)target;
        if (GUILayout.Button("Rebuild with interior")) {
            Undo.RegisterFullObjectHierarchyUndo(houseBuilder.gameObject, "Rebuild with interior");
            ProceduralHouse p = new ProceduralHouse();
            p.RebuildHouseWithInterior(houseBuilder.houseDefinition, houseBuilder.gameObject);
        }
        if (GUILayout.Button("Remove Meshes")) {
            Undo.RegisterFullObjectHierarchyUndo(houseBuilder.gameObject, "Remove meshes");
            new Building().ClearMeshes(houseBuilder.gameObject);
        }
    }
}
