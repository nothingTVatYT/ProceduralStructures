using UnityEngine;
using ProceduralStructures;

public class HouseBuilder : MonoBehaviour
{
    public HouseDefinition houseDefinition;
    public string streetName;
    public int number;

    public void OnDrawGizmosSelected() {
        DrawGizmo(true);
    }

    public void OnDrawGizmos() {
        DrawGizmo(false);
    }

    void DrawGizmo(bool selected) {
        Gizmos.matrix = transform.localToWorldMatrix;
        if (houseDefinition != null) {
            if (GetComponentInChildren<MeshRenderer>() == null) {
                float h = houseDefinition.totalHeight;
                Vector3 center = calculateCenter();
                Vector3 front = center + new Vector3(0, 0, -houseDefinition.length/2);
                Bounds bounds = new Bounds(center, calculateSize());
                if (selected) {
                    Gizmos.color = new Color(1, 1, 1, 0.4f);
                    Gizmos.DrawCube(bounds.center, bounds.size);
                } else {
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
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
