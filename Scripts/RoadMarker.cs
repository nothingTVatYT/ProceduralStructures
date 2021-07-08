using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMarker : MonoBehaviour
{
    public List<RoadMarker> connections = new List<RoadMarker>();

    public void OnDrawGizmos() {
        //Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 1, 0), 2);
        if (connections != null) {
            foreach(RoadMarker r in connections) {
                if (r != null && r.gameObject != null)
                    Gizmos.DrawLine(transform.position, r.gameObject.transform.position);
            }
        }
    }
}
