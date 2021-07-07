using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BaseBuilderEditor : Editor
{

    public static int CUTOUT = 1;


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


    protected List<Face> ExtrudeEdges(List<Vector3> vertices, Vector3 direction, float uvScale=1f) {
        List<Face> faces = new List<Face>();
        Vector3 prev = Vector3.zero;
        bool firstVertex = true;
        float el = direction.magnitude;
        foreach (Vector3 v in vertices) {
            if (!firstVertex) {
                Face face = new Face(prev, prev + direction, v + direction, v);
                face.SetUVFront(Vector3.Distance(prev, v) * uvScale, el * uvScale);
                faces.Add(face);
            }
            prev = v;
            firstVertex = false;
        }
        return faces;
    }

    protected List<Face> IndentFace(Face face, Vector3 direction, float uvScale=1f) {
        List<Face> faces = new List<Face>();
        Vector3 prev = Vector3.zero;
        bool firstVertex = true;
        float el = direction.magnitude;
        Vector3[] vertices;
        vertices = new Vector3[] { face.a, face.d, face.c, face.b, face.a };
        foreach (Vector3 v in vertices) {
            if (!firstVertex) {
                Face face1 = new Face(prev, prev + direction, v + direction, v);
                face1.SetUVFront(Vector3.Distance(prev, v) * uvScale, el * uvScale);
                faces.Add(face1);
            }
            prev = v;
            firstVertex = false;
        }
        faces.Add(face.MoveFaceBy(direction));
        return faces;
    }

    protected List<Face> Cutout(List<Face> fromFaces, Rect dim, float uvScale) {
        List<Face> result = new List<Face>();
        // check which faces are affected
        foreach (Face face in fromFaces) {
            // is the normal of this face already pointing to us?
            Debug.Log(face.normal);
            // project the 2D rect on this face (the normal needs to point to Vector3.back)
            Face cutoutFace = ProjectRectOnFrontFace(dim, face.a.z);
            // is the cutout part of this face?
            if (cutoutFace.a.x >= face.a.x && cutoutFace.a.y >= face.a.y && cutoutFace.c.x <= face.c.x && cutoutFace.c.y <= face.c.y) {
                // slice the face into 9 parts leaving the center piece at the size of the cutout
                Face[] nf = SliceFace(face, cutoutFace.a.x - face.a.x, 0);

                Face leftColumn = nf[0];
                Face[] nf2 = SliceFace(nf[1], cutoutFace.d.x - nf[1].a.x, 0);
                Face middleColumn = nf2[0];
                Face rightColumn = nf2[1];
                
                nf = SliceFace(leftColumn, 0, cutoutFace.a.y - leftColumn.a.y);
                result.Add(nf[0]); // bottom left corner
                nf2 = SliceFace(nf[1], 0, cutoutFace.b.y-nf[1].a.y);
                result.Add(nf2[1]); // top left corner
                result.Add(nf2[0]); // left middle

                nf = SliceFace(middleColumn, 0, cutoutFace.a.y - middleColumn.a.y);
                result.Add(nf[0]); // bottom center
                nf2 = SliceFace(nf[1], 0, cutoutFace.b.y - nf[1].a.y);
                //result.Add(nf2[0]); // center
                nf2[0].Tag(CUTOUT);
                result.AddRange(IndentFace(nf2[0], new Vector3(0, 0, 0.3f), uvScale));
                result.Add(nf2[1]); // top center
                nf = SliceFace(rightColumn, 0, cutoutFace.a.y-rightColumn.a.y);
                result.Add(nf[0]); // bottom right corner
                nf2 = SliceFace(nf[1], 0, cutoutFace.b.y-nf[1].a.y);
                result.Add(nf2[0]); // right middle
                result.Add(nf2[1]); // top right corner
            } else {
                result.Add(face);
            }
        }
        return result;
    }

    protected Face[] SliceFace(Face face, float dx, float dy) {
        if (dx > 0) {
            Vector3 e = new Vector3(face.b.x + dx, face.b.y, face.b.z);
            Vector3 f = new Vector3(face.a.x + dx, face.a.y, face.a.z);
            float relX = dx/(face.d.x-face.a.x);
            Face left = new Face(face.a, face.b, e, f);
            left.uvA = face.uvA;
            left.uvB = face.uvB;
            left.uvC = Vector2.Lerp(face.uvB, face.uvC, relX);
            left.uvD = Vector2.Lerp(face.uvA, face.uvD, relX);
            Face right = new Face(f, e, face.c, face.d);
            right.uvA = left.uvD;
            right.uvB = left.uvC;
            right.uvC = face.uvC;
            right.uvD = face.uvD;
            return new Face[] { left, right };
        } else {
            Vector3 e = new Vector3(face.a.x, face.a.y + dy, face.a.z);
            Vector3 f = new Vector3(face.d.x, face.d.y + dy, face.d.z);
            float relY = dy/(face.b.y-face.a.y);
            Face bottom = new Face(face.a, e, f, face.d);
            bottom.uvA = face.uvA;
            bottom.uvB = Vector2.Lerp(face.uvA, face.uvB, relY);
            bottom.uvC = Vector2.Lerp(face.uvD, face.uvC, relY);
            bottom.uvD = face.uvD;
            Face top = new Face(e, face.b, face.c, f);
            top.uvA = bottom.uvB;
            top.uvB = face.uvB;
            top.uvC = face.uvC;
            top.uvD = bottom.uvC;
            return new Face[] { bottom, top };
        }
    }

/* Other than in Unity 2D this rect defines positive y up */
    protected Face ProjectRectOnFrontFace(Rect rect, float z) {
        Vector3 a = new Vector3(rect.x, rect.y, z);
        Vector3 b = new Vector3(rect.x, rect.y+rect.height, z);
        Vector3 c = new Vector3(rect.x+rect.width, rect.y+rect.height, z);
        Vector3 d = new Vector3(rect.x+rect.width, rect.y, z);
        return new Face(a, b, c, d);
    }

    protected Face FindFirstFaceByTag(List<Face> faces, int tag) {
        foreach (Face f in faces) {
            if (f.IsTagged(tag)) {
                return f;
            }
        }
        return null;
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
        public Vector3 normal { get { return Vector3.Cross(b-a, d-a);} }
        public bool isTriangle = false;
        public int tags = 0;
        public Vector3[] GetVertices() {
            return new Vector3[] { a, b, c, d };
        }
        public Vector3[] GetVerticesCCW() {
            return new Vector3[] { a, d, c, b };
        }
        public override string ToString() {
            return "F(" + a +"," + b + "," + c + "," + d + ")";
        }
        public bool IsTagged(int tag) {
            return (tags & tag) != 0;
        }
        public void Tag(int tag) {
            tags |= tag;
        }
        public void UnTag(int tag) {
            tags &= ~tag;
        }
        public Face MoveFaceBy(Vector3 direction) {
            a = a + direction;
            b = b + direction;
            c = c + direction;
            d = d + direction;
            return this;
        }
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

    protected class Building {
        Dictionary<Material, List<Face>> facesByMaterial = new Dictionary<Material, List<Face>>();

        public List<Face> GetFacesByMaterial(Material material) {
            if (material == null) {
                material = new Material(Shader.Find("Standard"));
            }
            if (facesByMaterial.ContainsKey(material)) {
                return facesByMaterial[material];
            } else {
                List<Face> faces = new List<Face>();
                facesByMaterial[material] = faces;
                return faces;
            }
        }

        public void AddFace(Face face, Material material) {
            GetFacesByMaterial(material).Add(face);
        }

        public void AddFaces(List<Face> faces, Material material) {
            GetFacesByMaterial(material).AddRange(faces);
        }

        public void Build(GameObject target) {
            ClearMeshes(target);
            foreach (KeyValuePair<Material, List<Face>> keyValue in facesByMaterial) {
                Mesh mesh = BuildMesh(keyValue.Value);
                mesh.name = "Generated Mesh (" + keyValue.Key.name + ")";
                AddMesh(target, mesh, keyValue.Key);
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
                childByMaterial.isStatic = target.isStatic;
            }
            MeshFilter meshFilter = childByMaterial.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = childByMaterial.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            mesh.RecalculateNormals();
            mesh.Optimize();
            MeshCollider meshCollider = childByMaterial.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        public void ClearMeshes(GameObject target) {
            for (int i = target.transform.childCount-1; i>=0; i--) {
                GameObject go = target.transform.GetChild(i).gameObject;
                DestroyImmediate(go);
            }
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
