using UnityEngine;

public class CaveMarker : MonoBehaviour
{
    public Transform nextMarker;

    public void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(transform.position, 0.2f);
        if (nextMarker != null) {
            Gizmos.DrawLine(transform.position, nextMarker.position);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
