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
        if (GUILayout.Button("Transforms to points")) {
            Undo.RecordObject(city, "Transforms to points");
            foreach (CityDefinition.Street street in city.cityDefinition.streets) {
                street.points.Clear();
                street.length = 0;
                float streetLength = 0;
                Vector3 begin;
                if (street.transforms != null && street.transforms.Count > 0) {
                    Transform prev = null;
                    begin = street.transforms[0].position;
                    foreach (Transform t in street.transforms) {
                        if (prev != null) {
                            RoadMarker previousMarker = prev.gameObject.GetComponent<RoadMarker>();
                            RoadMarker currentMarker = t.gameObject.GetComponent<RoadMarker>();
                            if (previousMarker != null && currentMarker != null) {
                                previousMarker.nextMarker = currentMarker;
                            }
                        }
                        street.points.Add(t.position);
                        streetLength += (begin-t.position).magnitude;
                        begin = t.position;
                        prev = t;
                    }
                    street.length = streetLength;
                }
            }
        }
        if (GUILayout.Button("Place buildings")) {
            CityBuilder cityBuilder = new CityBuilder();
            if (city.cityDefinition.parent != null) {
                Undo.RegisterFullObjectHierarchyUndo(city.cityDefinition.parent, "Place buildings");
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
        if (GUILayout.Button("Paint road layer")) {
            CityBuilder cityBuilder = new CityBuilder();
            cityBuilder.PaintTerrain(city.cityDefinition);
        }
        if (GUILayout.Button("Repair alpha layers")) {
            CityBuilder cityBuilder = new CityBuilder();
            cityBuilder.RepairTerrainAlphamap(city.cityDefinition);
        }
    }
}
