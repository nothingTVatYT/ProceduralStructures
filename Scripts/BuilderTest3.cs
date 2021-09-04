using System.Collections.Generic;
using UnityEngine;
using ProceduralStructures;

public class BuilderTest3 : MonoBehaviour
{
    [Range(0, 20)]
    public int children = 0;
    public GameObject target;
    public Material material;
    public float uvScale;
    public bool splitBigTriangles = false;
    public float maxRelativeSize = 0.2f;
    public float offset = 0.1f;

    private int prevChildren = 0;
    private ConvexBody body;

    public void Update()
    {
        if (children != prevChildren) {
            Rebuild();
        }
    }

    public void Rebuild() {
        if (body == null) {
                body = new ConvexBody();
        }
        body.uvScale = uvScale;
        prevChildren = children;
        body.Clear();
        for (int i = 0; i < transform.childCount; i++) {
            if (i < children) {
                Transform tf = transform.GetChild(i);
                BoxCollider boxCollider = tf.gameObject.GetComponent<BoxCollider>();
                if (boxCollider != null) {
                    foreach (Vector3 v in GetCorners(boxCollider)) {
                        body.Add(tf.TransformPoint(v));
                    }
                } else {
                    body.Add(transform.GetChild(i).position);
                }
            } else break;
        }
        if (splitBigTriangles) {
            body.SplitBigTriangles(maxRelativeSize, offset);
        }
        body.Build(target, material);
    }

    public IEnumerable<Vector3> GetCorners(BoxCollider c) {
        float dx = c.size.x/2;
        float dy = c.size.x/2;
        float dz = c.size.x/2;
        yield return c.center + new Vector3(dx, dy, dz);
        yield return c.center + new Vector3(-dx, dy, dz);
        yield return c.center + new Vector3(dx, -dy, dz);
        yield return c.center + new Vector3(-dx, -dy, dz);
        yield return c.center + new Vector3(dx, dy, -dz);
        yield return c.center + new Vector3(-dx, dy, -dz);
        yield return c.center + new Vector3(dx, -dy, -dz);
        yield return c.center + new Vector3(-dx, -dy, -dz);
    }
}
