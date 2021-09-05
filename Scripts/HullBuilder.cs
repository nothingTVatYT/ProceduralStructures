using System.Collections;
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

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Rebuild() {
        GameObject wrappedObject = toEnclose;
        if (wrappedObject == null) {
            wrappedObject = gameObject;
        }
        ConvexBody body = new ConvexBody();
        body.uvScale = uvScale;
        for (int i = 0; i < wrappedObject.transform.childCount; i++) {
            Transform tf = wrappedObject.transform.GetChild(i);
            HouseBuilder[] houseBuilders = tf.gameObject.GetComponentsInChildren<HouseBuilder>();
            if (houseBuilders != null) {
                foreach (HouseBuilder houseBuilder in houseBuilders) {
                    if (!ignoreInactive || houseBuilder.gameObject.activeInHierarchy) {
                        foreach (Vector3 v in GetCorners(houseBuilder.calculateCenter(), houseBuilder.calculateSize())) {
                            body.Add(transform.InverseTransformPoint(houseBuilder.transform.TransformPoint(v)));
                        }
                    }
                }
            }
            BoxCollider[] boxColliders = tf.gameObject.GetComponentsInChildren<BoxCollider>();
            if (boxColliders != null) {
                foreach (BoxCollider boxCollider in boxColliders) {
                    if (!ignoreInactive || boxCollider.gameObject.activeInHierarchy) {
                        foreach (Vector3 v in GetCorners(boxCollider.center, boxCollider.size)) {
                            body.Add(transform.InverseTransformPoint(boxCollider.transform.TransformPoint(v)));
                        }
                    }
                }
            } else {
                body.Add(tf.position);
            }
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
