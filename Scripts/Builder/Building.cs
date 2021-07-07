using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class Building
    {
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
                Object.DestroyImmediate(go);
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
}