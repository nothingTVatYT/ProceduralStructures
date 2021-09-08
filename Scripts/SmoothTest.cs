using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralStructures;

public class SmoothTest : MonoBehaviour
{
    public enum SmoothMethod { Linear, Cosine, Cubic, CornerCutting }
    public GameObject target;
    public Material material;
    public float uvScale = 1;
    public float updateInterval = 1f;
    public SmoothMethod smoothMethod = SmoothMethod.Linear;
    [Range(0,1)]
    public float factorMu = 0.5f;
    [Range(0,9)]
    public int insertPoints = 2;
    public bool keepOriginal = true;
    public int verticesBefore;
    public int verticesAfter;

    float updated;

    // Start is called before the first frame update
    void Start()
    {
        CreateMesh();
        updated = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - updated > updateInterval) {
            CreateMesh();
            updated = Time.time;
        }
    }

    public void CreateMesh() {
        MeshObject obj = new MeshObject();
        ProceduralStructure ps = new ProceduralStructure();
        List<Vector3> shape = ps.ConstructTunnelShape(4, 5);
        verticesBefore = shape.Count;
        List<Vector3> smoothedShape = SmoothVertices(shape);
        verticesAfter = smoothedShape.Count;
        List<Vertex> v = obj.AddRange(smoothedShape);
        obj.CreateTriangleFan(v);
        obj.FlipNormals();
        obj.SetUVBoxProjection(uvScale);
        obj.Build(target, material);
    }

    public List<Vector3> SmoothVertices(List<Vector3> l) {
        List<Vector3> newList = new List<Vector3>(l.Count*3);
        CircularReadonlyList<Vector3> ring = new CircularReadonlyList<Vector3>(l);
        for (int i = 0; i < l.Count; i++) {
            if (keepOriginal)
                newList.Add(ring[i]);
            if (smoothMethod == SmoothMethod.CornerCutting) {
                newList.Add(0.75f*ring[i] + 0.25f*ring[i+1]);
                newList.Add(0.25f*ring[i] + 0.75f*ring[i+1]);
            } else {
                for (int n = 0; n < insertPoints; n++) {
                    Vector3 interpolated = Interpolate(smoothMethod, ring[i-1], ring[i], ring[i+1], ring[i+2], (1f+n)/(1f+insertPoints));
                    newList.Add(interpolated);
                }
            }
        }
        return newList;
    }

    public Vector3 Interpolate(SmoothMethod method, Vector3 a, Vector3 b, Vector3 c, Vector3 d, float mu) {
        Vector3 result = (a+b)/2;
        switch (method) {
            case SmoothMethod.Linear:
            result = b*(1f-mu) + c*mu;
            break;
            case SmoothMethod.Cosine:
            float mu2 = (1-Mathf.Cos(mu*Mathf.PI))/2;
            result = b*(1f-mu2) + c*mu2;
            break;
            case SmoothMethod.Cubic:
            float mus2 = mu*mu;
            Vector3 a0 = d - c - a + b;
            Vector3 a1 = a - b - a0;
            Vector3 a2 = b - a;
            Vector3 a3 = b;

            result = a0*mu*mus2+a1*mus2+a2*mu+a3;
            break;
            case SmoothMethod.CornerCutting:
            break;
        }
        return result;
    }
}
