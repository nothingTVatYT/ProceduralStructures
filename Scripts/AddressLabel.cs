using UnityEngine;
using TMPro;

public class AddressLabel : MonoBehaviour
{
    public string text;
    public bool autoUpdate = true;

    void Start()
    {
        if (autoUpdate) {
            SuggestStreetName();
        }
        TextMesh textMesh = GetComponent<TextMesh>();
        if (textMesh != null) {
            textMesh.text = text;
        } else {
            TextMeshPro textMeshPro = GetComponent<TextMeshPro>();
            if (textMeshPro != null) {
                textMeshPro.text = text;
            }
        }
    }

    public void SuggestStreetName() {
        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, 30);
        if (colliders != null && colliders.Length > 0) {
            float hitDistance = float.MaxValue;
            Vector3 position = gameObject.transform.position;
            foreach (Collider collider in colliders) {
                HouseBuilder hb = collider.gameObject.GetComponentInParent<HouseBuilder>();
                if (hb != null) {
                    float distance = Vector3.Distance(collider.bounds.center, position);
                    if (distance < hitDistance) {
                        hitDistance = distance;
                        text = hb.streetName;
                    }
                }
            }
        }
    }
}
