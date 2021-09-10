using UnityEngine;

public class SwitchCollider : MonoBehaviour {
    void Start() {
        Collider collider = GetComponent<Collider>();
        if (collider != null) {
            collider.enabled = false;
        }
    }
}
