using System.Collections.Generic;
using UnityEngine;
using ProceduralStructures;
using ExtensionMethods;

public class BuilderTest2 : MonoBehaviour
{

/*
plane((0.1, 0.0, -1.7),(-0.7, 0.0, -0.6)),
current (-0.3, 0.0, -2.0),(0.0, 0.2, -3.2),(0.1, 0.4, -3.4),(0.1, 1.8, -3.5),(-0.1, 2.6, -3.1),(-0.2, 3.0, -2.5),(-0.3, 3.0, -2.0),(-0.5, 3.0, -1.5),(-0.6, 2.6, -1.0),(-0.7, 1.8, -0.6),(-0.7, 0.4, -0.5),(-0.6, 0.2, -0.8),
previous (0.5, 0.0, -1.5),(1.5, 0.2, -2.1),(1.7, 0.4, -2.2),(1.8, 1.8, -2.2),(1.4, 2.6, -2.0),(1.0, 3.0, -1.7),(0.5, 3.0, -1.4),(0.0, 3.0, -1.1),(-0.4, 2.6, -0.9),(-0.8, 1.8, -0.7),(-0.8, 0.4, -0.7),(-0.6, 0.2, -0.8)
*/

    public GameObject generatedMesh;
    public List<Vector3> current;
    public List<Vector3> previous;
    public Vector3 plane;
    public Vector3 planeNormal;
    public Material material;
    public Material cutPlaneMaterial;
    public bool omitPlane;
    public bool omitCurrent;
    public bool omitPrevious;
    public bool omitBridge;

    public void Initialize()
    {
        current = new List<Vector3> {
            new Vector3(-0.3f, 0.0f, -2.0f),
            new Vector3(0.0f, 0.2f, -3.2f),
            new Vector3(0.1f, 0.4f, -3.4f),
            new Vector3(0.1f, 1.8f, -3.5f),
            new Vector3(-0.1f, 2.6f, -3.1f),
            new Vector3(-0.2f, 3.0f, -2.5f),
            new Vector3(-0.3f, 3.0f, -2.0f),
            new Vector3(-0.5f, 3.0f, -1.5f),
            new Vector3(-0.6f, 2.6f, -1.0f),
            new Vector3(-0.7f, 1.8f, -0.6f),
            new Vector3(-0.7f, 0.4f, -0.5f),
            new Vector3(-0.6f, 0.2f, -0.8f)
        };
        previous = new List<Vector3> {
            new Vector3(0.5f, 0.0f, -1.5f),
            new Vector3(1.5f, 0.2f, -2.1f),
            new Vector3(1.7f, 0.4f, -2.2f),
            new Vector3(1.8f, 1.8f, -2.2f),
            new Vector3(1.4f, 2.6f, -2.0f),
            new Vector3(1.0f, 3.0f, -1.7f),
            new Vector3(0.5f, 3.0f, -1.4f),
            new Vector3(0.0f, 3.0f, -1.1f),
            new Vector3(-0.4f, 2.6f, -0.9f),
            new Vector3(-0.8f, 1.8f, -0.7f),
            new Vector3(-0.8f, 0.4f, -0.7f),
            new Vector3(-0.6f, 0.2f, -0.8f)
        };
        plane = new Vector3(0.1f, 0.0f, -1.7f);
        planeNormal = new Vector3(-0.7f, 0.0f, -0.6f);
    }

    public void DrawTest()
    {
        Building building = new Building();
        BuildingObject obj1 = new BuildingObject();
        obj1.material = material;
        if (!omitPlane) {
            Face quad = Face.QuadOnPlane(plane, -planeNormal, 5);
            quad.material = cutPlaneMaterial;
            obj1.AddFace(quad);
        }
        if (!omitCurrent) {
            obj1.AddFaces(Face.PolygonToTriangleFan(current));
        }
        if (!omitPrevious) {
            obj1.AddFaces(Face.PolygonToTriangleFan(previous));
        }
        if (!omitBridge) {
            obj1.AddFaces(Builder.BridgeEdgeLoopsPrepared(current, previous, 1));
        }
        building.AddObject(obj1);
        building.Build(generatedMesh);
    }

    public void ClampToPlane() {
        Builder.ClampToPlane(current, previous, plane, planeNormal);
    }
}
