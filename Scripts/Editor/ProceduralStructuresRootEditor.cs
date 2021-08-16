using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralStructuresRoot))]
public class ProceduralStructuresRootEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        ProceduralStructuresRoot root = (ProceduralStructuresRoot)target;
        if (GUILayout.Button("Rebuild all in hierarchy starting here")) {
            HouseBuilder[] builder = root.gameObject.GetComponentsInChildren<HouseBuilder>();

            if (builder != null && builder.Length > 0) {
                Debug.Log("Found " + builder.Length + " house builders.");
                ProceduralStructures.ProceduralHouse p = new ProceduralStructures.ProceduralHouse();
                ProceduralStructures.Building building = new ProceduralStructures.Building();
                foreach (HouseBuilder h in builder) {
                    Undo.RegisterFullObjectHierarchyUndo(h.gameObject, "Rebuild structures");
                    building.ClearMeshes(h.gameObject);
                    p.RebuildHouseWithInterior(h.houseDefinition, h.gameObject);
                }
            } else {
                Debug.Log("No builders were found.");
            }
        }

        if (GUILayout.Button("Remove generated objects in hierarchy starting here")) {
            HouseBuilder[] builder = root.gameObject.GetComponentsInChildren<HouseBuilder>();

            if (builder != null && builder.Length > 0) {
                Debug.Log("Found " + builder.Length + " house builders.");
                ProceduralStructures.Building building = new ProceduralStructures.Building();
                foreach (HouseBuilder h in builder) {
                    Undo.RegisterFullObjectHierarchyUndo(h.gameObject, "Remove structures");
                    building.ClearMeshes(h.gameObject);
                }
            } else {
                Debug.Log("No builders were found.");
            }
        }

        if (GUILayout.Button("Suggest street names on signs")) {
            AddressLabel[] labels = root.gameObject.GetComponentsInChildren<AddressLabel>();
            if (labels != null && labels.Length > 0) {
                Debug.Log("Found " + labels.Length + " street signs.");
                foreach (AddressLabel label in labels) {
                    Undo.RegisterCompleteObjectUndo(label, "Suggest name");
                    label.SuggestStreetName();
                }
            }
        }
    }
}
