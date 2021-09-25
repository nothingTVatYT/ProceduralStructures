using UnityEngine;
using UnityEditor;
using ProceduralStructures;

[CustomEditor(typeof(StructureBuilder))]
[CanEditMultipleObjects]
public class StructureBuilderEditor : Editor {

    private StructureBuilder structureBuilder;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        structureBuilder = (StructureBuilder)target;
        if (GUILayout.Button("Rebuild")) {
            Undo.RegisterFullObjectHierarchyUndo(structureBuilder.gameObject, "Rebuild structure");
            ProceduralStructure p = new ProceduralStructure();
            p.RebuildLadder(structureBuilder.ladderDefinition, structureBuilder.gameObject);
            EditorUtilities.CreateSecondaryUV(structureBuilder.gameObject.GetComponentsInChildren<MeshFilter>());
        }
        if (GUILayout.Button("Remove Meshes")) {
            Undo.RegisterFullObjectHierarchyUndo(structureBuilder.gameObject, "Remove meshes");
            Building.ClearMeshes(structureBuilder.gameObject);
        }
    }
}
