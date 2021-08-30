using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveBuilderComponent : MonoBehaviour
{
    public Transform generatedMeshParent;

    public bool useTransforms;
    public Transform wayPointTransforms;
    public CaveDefinition caveDefinition;

    void Start()
    {
        
    }

    public void UpdateWayPoints() {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < wayPointTransforms.childCount; i++) {
            Vector3 v = wayPointTransforms.GetChild(i).position;
            points.Add(v);
        }
        caveDefinition.wayPoints = points;
    }

    public void OnDrawGizmosSelected() {
        if (caveDefinition.IsValid()) {
            Vector3 a = caveDefinition.wayPoints[0];
            int n = 0;
            Gizmos.color = Color.yellow;
            foreach (Vector3 v in caveDefinition.GetVertices()) {
                n++;
                Gizmos.DrawLine(a, v);
                Gizmos.DrawSphere(v, 0.1f);
                a = v;
            }
        }
    }
}
