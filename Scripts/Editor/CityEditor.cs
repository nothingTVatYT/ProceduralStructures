using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralStructures;

[CustomEditor(typeof(CityMarker))]
public class CityEditor : Editor
{
    public override void OnInspectorGUI() {
        CityMarker city = target as CityMarker;
        DrawDefaultInspector();
        if (GUILayout.Button("Update Streets")) {
            CityBuilder cityBuilder = new CityBuilder();
            cityBuilder.UpdateStreets(city.cityDefinition);
        }
        if (GUILayout.Button("Place buildings")) {
            CityBuilder cityBuilder = new CityBuilder();
            if (city.cityDefinition.parent != null) {
                Undo.RegisterFullObjectHierarchyUndo(city.cityDefinition.parent, "Place buildings");
            } else {
                Undo.RegisterFullObjectHierarchyUndo(city.gameObject, "Place buildings");
                city.cityDefinition.parent = ProceduralStructure.CreateEmptyChild(city.gameObject, "Generated Structures");
            }
            cityBuilder.PlaceHouses(city.cityDefinition);
        }
        if (GUILayout.Button("Remove buildings")) {
            CityBuilder cityBuilder = new CityBuilder();
            if (city.cityDefinition.parent != null) {
                Undo.RegisterFullObjectHierarchyUndo(city.cityDefinition.parent, "Remove buildings");
            }
            cityBuilder.RemoveHouses(city.cityDefinition);
        }
        if (city.cityDefinition.roadPainting.enabled && GUILayout.Button("Paint road layer")) {
            CityBuilder cityBuilder = new CityBuilder();
            cityBuilder.PaintTerrain(city.cityDefinition);
        }
        if (city.cityDefinition.roadPainting.enabled && GUILayout.Button("Repair alpha layers")) {
            CityBuilder cityBuilder = new CityBuilder();
            cityBuilder.RepairTerrainAlphamap(city.cityDefinition);
        }
    }
}
