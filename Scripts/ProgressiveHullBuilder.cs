using UnityEngine;
using ProceduralStructures;

public class ProgressiveHullBuilder : MonoBehaviour
{
    public VertexListRecorder vertexList;
    public GameObject target;
    public GameObject center;
    public Material material;
    public bool fixSlivers = true;
    public int pointsUsed = 0;
    public int trianglesCreated = 0;

    private ConvexBody body;

    // Start is called before the first frame update
    void Start()
    {
        body = new ConvexBody();
        body.targetGameObject = target;
        body.material = material;
        if (vertexList != null) vertexList.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (vertexList == null || target == null) return;
        body.fixSlivers = fixSlivers;
        if (Input.GetKeyDown(KeyCode.N)) {
            if (vertexList.HasNext()) {
                Vector3 nextPoint = vertexList.Next();
                DebugLocalPoint(nextPoint);
                body.AddPoint(nextPoint);
                UpdateBody();
                if (center != null) {
                    center.transform.position = transform.TransformPoint(body.GetCenter());
                }
            } else {
                Debug.Log("End of list reached.");
            }
        } else if (Input.GetKeyDown(KeyCode.R)) {
            vertexList.Reset();
            body.Clear();
            UpdateBody();
        }
    }

    void DebugLocalPoint(Vector3 pos) {
        Debug.DrawLine(transform.TransformPoint(pos-Vector3.left), transform.TransformPoint(pos-Vector3.right), Color.blue, 5);
        Debug.DrawLine(transform.TransformPoint(pos-Vector3.up), transform.TransformPoint(pos-Vector3.down), Color.blue, 5);
    }

    void UpdateBody() {
        body.Build(target, material);
        pointsUsed = body.VerticesCount;
        trianglesCreated = body.TrianglesCount;
    }
}
