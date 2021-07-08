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
            foreach (CityDefinition.Street street in city.cityDefinition.streets) {
                street.points.Clear();
                street.length = 0;
                float streetLength = 0;
                Vector3 begin;
                if (street.transforms != null && street.transforms.Count > 0) {
                    begin = street.transforms[0].position;
                    foreach (Transform t in street.transforms) {
                        street.points.Add(t.position);
                        streetLength += (begin-t.position).magnitude;
                        begin = t.position;
                    }
                    street.length = streetLength;
                }
            }
        }
        if (GUILayout.Button("Place buildings")) {
            CityBuilder cityBuilder = new CityBuilder();
            cityBuilder.PlaceHouses(city.cityDefinition);
        }
    }
}
