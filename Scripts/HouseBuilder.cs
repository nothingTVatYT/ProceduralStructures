using UnityEngine;

public class HouseBuilder : MonoBehaviour
{
    public HouseDefinition houseDefinition;

    public void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        if (houseDefinition != null) {
            if (GetComponentInChildren<MeshRenderer>() == null) {
                float h = houseDefinition.totalHeight;
                Vector3 center = new Vector3(0, h/2 + houseDefinition.heightOffset, 0);
                Vector3 front = center + new Vector3(0, 0, -houseDefinition.length/2);
                Gizmos.DrawWireCube(center, new Vector3(houseDefinition.width, h, houseDefinition.length));
                Gizmos.DrawRay(front, Vector3.back * 2);
            }
        } else {
            Gizmos.DrawCube(Vector3.zero, new Vector3(1, 1, 1));
        }
    }

    public Vector3 calculateCenter() {
        if (houseDefinition != null) {
            return new Vector3(0, houseDefinition.totalHeight/2 + houseDefinition.heightOffset, 0);
        } else {
            return transform.position;
        }
    }

    public Vector3 calculateSize() {
        if (houseDefinition != null) {
            return new Vector3(houseDefinition.width, houseDefinition.totalHeight, houseDefinition.length);
        } else {
            return new Vector3(4, 2, 4);
        }
    }
}
