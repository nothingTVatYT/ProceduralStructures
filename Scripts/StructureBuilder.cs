using UnityEngine;
using ProceduralStructures;

public class StructureBuilder : MonoBehaviour
{
    public LadderDefinition ladderDefinition;

    public void OnDrawGizmosSelected() {
        DrawGizmo(true);
    }

    public void OnDrawGizmos() {
        DrawGizmo(false);
    }

    void DrawGizmo(bool selected) {
        Gizmos.matrix = transform.localToWorldMatrix;
        if (ladderDefinition != null) {
            if (GetComponentInChildren<MeshRenderer>() == null) {
                float h = ladderDefinition.TotalHeight;
                Vector3 center = calculateCenter();
                Vector3 front = center + new Vector3(0, 0, -ladderDefinition.stepThickness/2);
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
        if (ladderDefinition != null) {
            return new Vector3(0, ladderDefinition.TotalHeight/2, 0);
        } else {
            return transform.position;
        }
    }

    public Vector3 calculateSize() {
        if (ladderDefinition != null) {
            return new Vector3(ladderDefinition.stepWidth + 2*ladderDefinition.stepThickness, ladderDefinition.TotalHeight, ladderDefinition.stepThickness);
        } else {
            return new Vector3(1, 4, 0.1f);
        }
    }
}
