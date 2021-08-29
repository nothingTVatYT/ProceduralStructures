using UnityEngine;

public class RoadMarker : MonoBehaviour
{
    public RoadMarker nextMarker;

    public void OnDrawGizmosSelected() {
        //Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 1, 0), 2);
        if (nextMarker != null) {
            Gizmos.DrawLine(transform.position, nextMarker.transform.position);
        }
    }
}
