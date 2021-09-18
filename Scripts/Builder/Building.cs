using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class Building
    {
        public delegate void ExcludeFromNavmesh(GameObject gameObject);
        public ExcludeFromNavmesh excludeFromNavmesh;

        List<Face> faces = new List<Face>();
        Dictionary<Material, List<Face>> facesByMaterial = new Dictionary<Material, List<Face>>();
        public static string ADDED_INTERIOR = "generatedInterior";
        public static List<string> namesOfGeneratedObjects = new List<string> {"LOD0", "LOD1", "LOD2", ADDED_INTERIOR};
        private List<Material> nonNavmeshStaticMaterials = new List<Material>();

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

        void GroupFacesByMaterial() {
            facesByMaterial.Clear();
            foreach (Face face in faces) {
                GetFacesByMaterial(face.material).Add(face);
            }
        }

        public void ClearNavmeshStaticOnMaterial(Material material) {
            nonNavmeshStaticMaterials.Add(material);
        }

        public void AddFace(Face face) {
            faces.Add(face);
        }

        public void AddObject(BuildingObject child) {
            child.ApplyTransform().ApplyDefaultMaterial();
            faces.AddRange(child.faces);
        }

        public void Build(GameObject target, int lod) {
            GameObject lodTarget = GetChildByName(target, "LOD" + lod);
            if (lodTarget == null) {
                lodTarget = new GameObject("LOD" + lod);
                lodTarget.transform.parent = target.transform;
                lodTarget.transform.localPosition = Vector3.zero;
                lodTarget.transform.localRotation = Quaternion.identity;
                lodTarget.transform.localScale = Vector3.one;
                lodTarget.isStatic = target.isStatic;
            }
            Build(lodTarget);
        }
        
        public void Build(GameObject target) {
            ClearMeshes(target);
            GroupFacesByMaterial();
            foreach (KeyValuePair<Material, List<Face>> keyValue in facesByMaterial) {
                Mesh mesh = BuildMesh(keyValue.Value);
                mesh.name = "Generated Mesh (" + keyValue.Key.name + ")";
                AddMesh(target, mesh, keyValue.Key);
            }
        }

        public void AddMesh(GameObject target, Mesh mesh, Material material) {
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
                if (nonNavmeshStaticMaterials.Contains(material) && excludeFromNavmesh != null) {
                    excludeFromNavmesh(childByMaterial);
                }
            }
            MeshFilter meshFilter = childByMaterial.GetComponent<MeshFilter>();
            if (meshFilter == null) {
                meshFilter = childByMaterial.AddComponent<MeshFilter>();
            }
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = childByMaterial.GetComponent<MeshRenderer>();
            if (meshRenderer == null) {
                meshRenderer = childByMaterial.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterial = material;
            mesh.Optimize();
            mesh.RecalculateNormals();
            MeshCollider meshCollider = childByMaterial.GetComponent<MeshCollider>();
            if (meshCollider == null) {
                meshCollider = childByMaterial.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = mesh;
        }

        public static void ClearMeshes(GameObject target) {
            for (int i = target.transform.childCount-1; i>=0; i--) {
                GameObject go = target.transform.GetChild(i).gameObject;
                if (namesOfGeneratedObjects.Contains(go.name)) {
                    if (Application.isPlaying) {
                        Object.Destroy(go);
                    } else {
                        Object.DestroyImmediate(go);
                    }
                }
            }
        }

        public static GameObject GetChildByName(GameObject parent, string name) {
            for (int i = 0; i < parent.transform.childCount; i++) {
                GameObject child = parent.transform.GetChild(i).gameObject;
                if (child.name == name) {
                    return child;
                }
            }
            return null;
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