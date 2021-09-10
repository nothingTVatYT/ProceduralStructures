using System.Collections.Generic;
using UnityEngine;
using ProceduralStructures;

public class HullBuilder : MonoBehaviour {

    public GameObject hullRoot;
    public GameObject toEnclose;
    public bool ignoreInactive = true;
    public bool flipNormals = true;
    public Material material;
    public float uvScale = 1;
    public MeshObject.Shading shading = MeshObject.Shading.Flat;

    public bool randomizeVertices = false;
    public Vector3 randomDisplacement;
    public bool addConnector = false;
    public CaveBuilderComponent connectedCave;
    public bool decimateVertices = false;
    public int maxVertices = 6;

    public void Rebuild() {
        ConvexHull body = new ConvexHull();
        body.uvScale = uvScale;
        body.transform = transform;
        // set this early for debugging
        body.targetGameObject = hullRoot;
        body.material = material;
        GameObject wrappedObject = toEnclose;
        if (wrappedObject == null) {
            wrappedObject = gameObject;
        }
        for (int i = 0; i < wrappedObject.transform.childCount; i++) {
            Transform tf = wrappedObject.transform.GetChild(i);
            HouseBuilder[] houseBuilders = tf.gameObject.GetComponentsInChildren<HouseBuilder>();
            BoxCollider[] boxColliders = tf.gameObject.GetComponentsInChildren<BoxCollider>();
            if (houseBuilders != null && houseBuilders.Length > 0) {
                foreach (HouseBuilder houseBuilder in houseBuilders) {
                    if (!ignoreInactive || houseBuilder.gameObject.activeInHierarchy) {
                        foreach (Vector3 v in GetCorners(houseBuilder.calculateCenter(), houseBuilder.calculateSize())) {
                            body.AddPoint(transform.InverseTransformPoint(houseBuilder.transform.TransformPoint(v)));
                        }
                    }
                }
            } else if (boxColliders != null && boxColliders.Length > 0) {
                foreach (BoxCollider boxCollider in boxColliders) {
                    if (!ignoreInactive || boxCollider.gameObject.activeInHierarchy) {
                        foreach (Vector3 v in GetCorners(boxCollider.center, boxCollider.size)) {
                            body.AddPoint(transform.InverseTransformPoint(boxCollider.transform.TransformPoint(v)));
                        }
                    }
                }
            } else {
                body.AddPoint(transform.InverseTransformPoint(tf.position));
            }
        }

        body.CalculateHull();

        if (randomizeVertices) {
            body.RandomizeVertices(randomDisplacement);
        }
        if (addConnector && connectedCave != null) {
            MeshObject other = connectedCave.caveDefinition.GetConnection(connectedCave.gameObject.transform, 1, 0);
            if (decimateVertices) {
                other.Decimate(maxVertices);
            }
            other.targetGameObject = hullRoot;
            body.AddConnector(other);
        }

        if (flipNormals) {
            body.FlipNormals();
        }
        body.shading = shading;
        body.SetUVBoxProjection(uvScale);
        body.Build(hullRoot, material);
    }

    public IEnumerable<Vector3> GetCorners(Vector3 center, Vector3 size) {
        float dx = size.x/2;
        float dy = size.y/2;
        float dz = size.z/2;
        yield return center + new Vector3(dx, dy, dz);
        yield return center + new Vector3(-dx, dy, dz);
        yield return center + new Vector3(dx, -dy, dz);
        yield return center + new Vector3(-dx, -dy, dz);
        yield return center + new Vector3(dx, dy, -dz);
        yield return center + new Vector3(-dx, dy, -dz);
        yield return center + new Vector3(dx, -dy, -dz);
        yield return center + new Vector3(-dx, -dy, -dz);
    }

}
