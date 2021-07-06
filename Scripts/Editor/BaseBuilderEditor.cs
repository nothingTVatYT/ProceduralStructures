using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BaseBuilderEditor : Editor
{

    protected void ClearMeshes(GameObject target) {
        for (int i = target.transform.childCount-1; i>=0; i--) {
            GameObject go = target.transform.GetChild(i).gameObject;
            DestroyImmediate(go);
        }
    }

    protected void AddMesh(GameObject target, Mesh mesh, Material material) {
        GameObject childByMaterial = null;
        foreach (Transform t in target.transform) {
            if (t.gameObject.name == "mat-"+material.name) {
                childByMaterial = t.gameObject;
                break;
            }
        }
        if (childByMaterial == null) {
            childByMaterial = new GameObject();
            childByMaterial.name = "mat-" + material.name;
            childByMaterial.transform.parent = target.transform;
            childByMaterial.transform.localPosition = Vector3.zero;
            childByMaterial.transform.localRotation = Quaternion.identity;
            childByMaterial.transform.localScale = Vector3.one;
        }
        MeshFilter meshFilter = childByMaterial.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = childByMaterial.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
        mesh.Optimize();
        MeshCollider meshCollider = childByMaterial.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    protected BuildResult CreateBoxSides(Vector3 center, float width, float length, float height,
     float slopeX, float slopeZ, float uvScale) {
        return CreateBoxFaces(center, width, length, height, slopeX, slopeZ, uvScale, false, false);
    }

    protected BuildResult CreateBoxFaces(Vector3 center, float width, float length, float height,
     float slopeX, float slopeZ, float uvScale, bool createBottom, bool createTop) {
        List<Face> faces = new List<Face>();
        float floorY = 0;
        Vector3 vHeight = new Vector3(0, height, 0);
        // corners cw from top
        Vector3 a = center + new Vector3(-width/2, floorY, -length/2);
        Vector3 b = center + new Vector3(-width/2, floorY, length/2);
        Vector3 c = center + new Vector3(width/2, floorY, length/2);
        Vector3 d = center + new Vector3(width/2, floorY, -length/2);
        Vector3 a1 = a + new Vector3(slopeX*height, height, slopeZ*height);
        Vector3 b1 = b + new Vector3(slopeX*height, height, -slopeZ*height);
        Vector3 c1 = c + new Vector3(-slopeX*height, height, -slopeZ*height);
        Vector3 d1 = d + new Vector3(-slopeX*height, height, slopeZ*height);
        Face frontface = new Face(a, a1, d1, d);
        frontface.SetUVFront(width * uvScale, height * uvScale);
        faces.Add(frontface);
        Face rightFace = new Face(d, d1, c1, c);
        rightFace.SetUVFront(length * uvScale, height * uvScale);
        faces.Add(rightFace);
        Face backFace = new Face(c, c1, b1, b);
        backFace.SetUVFront(width * uvScale, height * uvScale);
        faces.Add(backFace);
        Face leftFace = new Face(b, b1, a1, a);
        leftFace.SetUVFront(length * uvScale, height * uvScale);
        faces.Add(leftFace);
        if (createTop) {
            Face topFace = new Face(a1, b1, c1, d1);
            topFace.SetUVFront(width * uvScale, length * uvScale);
            faces.Add(topFace);
        }
        if (createBottom) {
            Face bottomFace = new Face(b, a, d, c);
            bottomFace.SetUVFront(width * uvScale, length * uvScale);
            faces.Add(bottomFace);
        }
        return new BuildResult(faces, width - (2*slopeX*height), length - (2*slopeZ*height));
    }

    protected BuildResult CreatePrism(Vector3 center, float width, float length, float height, bool gableAlongWidth, float uvScale) {
        List<Face> faces = new List<Face>();
        Vector3 a = center + new Vector3(-width/2, 0, -length/2);
        Vector3 b = center + new Vector3(-width/2, 0, length/2);
        Vector3 c = center + new Vector3(width/2, 0, length/2);
        Vector3 d = center + new Vector3(width/2, 0, -length/2);
        if (gableAlongWidth) {
            Vector3 e1 = center + new Vector3(-width/2, height, 0);
            Vector3 e2 = center + new Vector3(width/2, height, 0);
            Face frontFace = new Face(a, e1, e2, d);
            frontFace.SetUVFront(width, height);
            faces.Add(frontFace);
            Face backFace = new Face(c, e2, e1, b);
            backFace.SetUVFront(width, height);
            faces.Add(backFace);
            Face leftFace = new Face(b, e1, a);
            leftFace.SetUVFront(length, height);
            faces.Add(leftFace);
            Face rightFace = new Face(d, e2, c);
            rightFace.SetUVFront(length, height);
            faces.Add(rightFace);
        } else {
            Vector3 e1 = center + new Vector3(0, height, -length/2);
            Vector3 e2 = center + new Vector3(0, height, length/2);
            Face frontFace = new Face(a, e1, d);
            frontFace.SetUVFront(width, height);
            faces.Add(frontFace);
            Face backFace = new Face(c, e2, b);
            backFace.SetUVFront(width, height);
            faces.Add(backFace);
            Face leftFace = new Face(b, e2, e1, a);
            leftFace.SetUVFront(length, height);
            faces.Add(leftFace);
            Face rightFace = new Face(d, e1, e2, c);
            rightFace.SetUVFront(length, height);
            faces.Add(rightFace);
        }
        return new BuildResult(faces, width, length);
    }

    protected Mesh BuildMesh(List<Face> faces) {
        Mesh mesh = new Mesh();
        int triangles;
        int verticesInFaces = CountVertices(faces, out triangles);
        Vector3[] vertices = new Vector3[verticesInFaces];
        Vector2[] uv = new Vector2[verticesInFaces];
        int[] tris = new int[6 * (faces.Count - triangles) + 3 * triangles];
        int index = 0;
        int trisIndex = 0;
        foreach (Face face in faces) {
            vertices[index] = face.a;
            uv[index] = face.uvA;
            index++;
            vertices[index] = face.b;
            uv[index] = face.uvB;
            index++;
            vertices[index] = face.c;
            uv[index] = face.uvC;
            index++;
            if (!face.isTriangle) {
                vertices[index] = face.d;
                uv[index] = face.uvD;
                index++;
                tris[trisIndex++] = index - 4; // A
                tris[trisIndex++] = index - 3; // B
                tris[trisIndex++] = index - 2; // C
                tris[trisIndex++] = index - 4; // A
                tris[trisIndex++] = index - 2; // C
                tris[trisIndex++] = index - 1; // D
            } else {
                tris[trisIndex++] = index - 3; // A
                tris[trisIndex++] = index - 2; // B
                tris[trisIndex++] = index - 1; // C
            }
        }
        
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uv;
        mesh.name = "generated mesh";
        return mesh;
    }

    protected int CountVertices(List<Face> faces, out int triangles) {
        int vertices = 0;
        triangles = 0;
        foreach (Face face in faces) {
            if (face.isTriangle) {
                vertices += 3;
                triangles++;
            } else {
                vertices += 4;
            }
        }
        return vertices;
    }

    protected List<Face> ExtrudeEdges(List<Vector3> vertices, Vector3 direction) {
        List<Face> faces = new List<Face>();
        Vector3 prev = Vector3.zero;
        bool firstVertex = true;
        float el = direction.magnitude;
        foreach (Vector3 v in vertices) {
            if (!firstVertex) {
                Face face = new Face(prev, prev + direction, v + direction, v);
                face.SetUVFront(Vector3.Distance(prev, v), el);
                faces.Add(face);
            }
            prev = v;
            firstVertex = false;
        }
        return faces;
    }

/*
    Face defined by four corners in clockwise order
    (A bottom left, B top left, C top right, D bottom right)
*/
    protected class Face {
        public Face() {}
        public Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            isTriangle = false;
        }
        public Face(Vector3 a, Vector3 b, Vector3 c) {
            this.a = a;
            this.b = b;
            this.c = c;
            isTriangle = true;
        }
        public Vector3 a,b,c,d;
        public Vector2 uvA,uvB,uvC,uvD;
        public bool isTriangle = false;
        public void SetUVFront(float width, float height) {
            uvA = new Vector2(0, 0);
            if (isTriangle) {
                uvB = new Vector2(width/2, height);
                uvC = new Vector2(width, 0);
            } else {
                uvB = new Vector2(0, height);
                uvC = new Vector2(width, height);
                uvD = new Vector2(width, 0);
            }
        }
    }

    protected class BuildResult {
        public List<Face> faces;
        public float topWidth;
        public float topLength;
        public BuildResult(List<Face> faces, float newWidth, float newLength) {
            this.faces = faces;
            this.topWidth = newWidth;
            this.topLength = newLength;
        }
    }

}
