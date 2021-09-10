using System;
using System.Collections.Generic;
using UnityEngine;
using ProceduralStructures;

public class HullBuilder : MonoBehaviour {

    [Serializable]
    public class MeshConnector {
        public enum Side { Beginning, End }
        public CaveBuilderComponent connectedCave;
        public int tunnelIndex = 0;
        public Side side = Side.Beginning;
        public bool decimateVertices = false;
        public int maxVertices = 6;
    }

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
    public MeshConnector connection;

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
        if (addConnector && connection != null && connection.connectedCave != null) {
            float side = connection.side == MeshConnector.Side.Beginning ? 0 : 1;
            MeshObject other = connection.connectedCave.caveDefinition.GetConnection(connection.tunnelIndex, side);
            if (connection.decimateVertices) {
                other.Decimate(connection.maxVertices);
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
