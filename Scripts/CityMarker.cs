using UnityEngine;

public class CityMarker : MonoBehaviour
{
    public CityDefinition cityDefinition;

    public void OnDrawGizmosSelected() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0, 1.5f, 0), new Vector3(20, 3, 20));
    }
}
