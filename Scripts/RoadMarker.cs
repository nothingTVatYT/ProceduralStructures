using UnityEngine;

public class RoadMarker : MonoBehaviour
{
    public void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 1, 0), 2);
    }
}
