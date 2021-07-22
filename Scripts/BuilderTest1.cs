using UnityEngine;
using ProceduralStructures;

public class BuilderTest1 : MonoBehaviour
{
    public float xDimension = 10f;
    public float yDimension = 5f;
    public float zDimension = 0.5f;
    [Range(0,3)]
    public int rotateVertices = 0;
    public Material material;
    public float uvScale = 1;
    public Transform cannon;
    public float holeWidth = 1.4f;
    public float holeHeight = 2.3f;
    public GameObject generatedMesh;
    Vector3 localOrigin;
    Vector3 localDirection;
    bool rebuildMesh = false;

    // Start is called before the first frame update
    void Start()
    {
        if (generatedMesh == null) {
            generatedMesh = gameObject;
        }
        rebuildMesh = true;
    }

    // Update is called once per frame
    void Update()
    {
        bool makeHole = false;
        if (Input.GetKeyDown(KeyCode.F)) {
            makeHole = true;
            rebuildMesh = true;
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            makeHole = false;
            rebuildMesh = true;
        }

        if (rebuildMesh) {
            Vector3 origin = cannon == null ? Vector3.zero : cannon.transform.position;
            Vector3 direction = cannon == null ? Vector3.forward : cannon.transform.TransformDirection(Vector3.forward);
            localOrigin = transform.InverseTransformPoint(origin);
            localDirection = transform.InverseTransformDirection(direction);
            Building building = new Building();
            BuildingObject wall = new BuildingObject();
            Vector3 a = new Vector3(-xDimension/2, 0, 0);
            Vector3 b = new Vector3(-xDimension/2, yDimension, 0);
            Vector3 c = new Vector3(xDimension/2, yDimension, 0);
            Vector3 d = new Vector3(xDimension/2, 0, 0);
            for (int i = 0; i < rotateVertices; i++) {
                Vector3 t = a;
                a = b;
                b = c;
                c = d;
                d = t;
            }
            Vector3 depth = Vector3.forward * zDimension;
            Vector3 e = a + depth;
            Vector3 f = b + depth;
            Vector3 g = c + depth;
            Vector3 h = d + depth;
            Face frontFace = new Face(a, b, c, d);
            Face backFace = new Face(h, g, f, e);
            Face bottomFace = new Face(e, a, d, h);
            Face topFace = new Face(b, f, g, c);
            Face rightFace = new Face(e, f, b, a);
            Face leftFace = new Face(d, c, g, h);
            wall.AddFace(frontFace);
            wall.AddFace(backFace);
            wall.AddFace(bottomFace);
            wall.AddFace(topFace);
            wall.AddFace(rightFace);
            wall.AddFace(leftFace);
            foreach (Face side in wall.faces) {
                side.SetUVForSize(uvScale);
            }

            if (makeHole) {
                wall.MakeHole(localOrigin, localDirection, Vector3.up, holeWidth, holeHeight, transform);
                Face opening = wall.FindFirstFaceByTag(Builder.CUTOUT);
                if (opening != null) {
                    wall.RemoveFace(opening);
                }
            }
            building.AddObject(wall, material);
            building.Build(generatedMesh);
            rebuildMesh = false;
        }
    }

}
