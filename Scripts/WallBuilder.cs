using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBuilder : MonoBehaviour
{
    public WallDefinition wall;

    void Start()
    {
        //UpdatePoints();
        //HideMarkers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrawGizmos() {
        if (wall.points.Count > 1) {
            Vector3 a = wall.points[0].position;
            for (int i = 1; i < wall.points.Count; i++) {
                Vector3 b = wall.points[i].position;
                Gizmos.DrawLine(a, b);
                a = b;
            }
            if (wall.closeLoop) {
                Gizmos.DrawLine(a, wall.points[0].position);
            }
        }
    }

    public void UpdatePoints() {
        if (wall.useChildren) {
            wall.points.Clear();
            foreach (Transform t in gameObject.transform) {
                if (!t.gameObject.name.StartsWith("LOD"))
                    wall.points.Add(t);
            }
        }
    }

    private void HideMarkers() {
        List<Transform> children;
        if (wall.useChildren) {
            children = new List<Transform>();
            foreach (Transform t in gameObject.transform) {
                if (!t.gameObject.name.StartsWith("LOD"))
                    children.Add(t);
            }
        } else {
            children = wall.points;
        }
        foreach (Transform t in children) {
            t.gameObject.SetActive(false);
        }
    }
}
