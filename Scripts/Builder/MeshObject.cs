using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class MeshObject {

        private static float Epsilon = 1e-3f;
        private static float EpsilonSquared = Epsilon*Epsilon;
        protected List<Vertex> vertices = new List<Vertex>();
        protected List<Triangle> triangles = new List<Triangle>();
        public float uvScale;

        public virtual int Add(Vector3 pos) {
            int idx = vertices.FindIndex((v) => (SameInTolerance(v.pos,pos)));
            if (idx >= 0) return idx;
            vertices.Add(new Vertex(pos));
            return vertices.Count - 1;
        }

        public int AddTriangle(Vertex v0, Vertex v1, Vertex v2) {
            Triangle triangle = new Triangle(v0, v1, v2);
            triangles.Add(triangle);
            // for visualizing only
            triangle.SetUVProjected(uvScale);
            return triangles.Count-1;
        }

        public int GetTriangleHit(Vector3 origin, Vector3 direction) {
            foreach (Triangle triangle in triangles) {
                Vector3 intersection;
                if (Face.RayHitTriangle(origin, direction, triangle.v0.pos, triangle.v1.pos, triangle.v2.pos, out intersection)) {
                    return triangles.IndexOf(triangle);
                }
            }
            return -1;
        }

        public bool IsBehind(Triangle triangle, Vector3 point) {
            Vector3 normal = triangle.normal;
            return Vector3.Dot(normal, point-triangle.v0.pos) < 0;
        }

        public bool IsInFront(Triangle triangle, Vector3 point) {
            Vector3 normal = triangle.normal;
            return Vector3.Dot(normal, point-triangle.v0.pos) > 0;
        }

        public int[] MakePyramid(Triangle triangle, Vertex newVertex) {
            int[] indices = new int[3];
            indices[0] = AddTriangle(triangle.v0, newVertex, triangle.v1);
            indices[1] = AddTriangle(triangle.v1, newVertex, triangle.v2);
            indices[2] = AddTriangle(triangle.v2, newVertex, triangle.v0);
            return indices;
        }

        public int[] SplitTriangle(Triangle triangle, Vertex v3) {
            triangles.Remove(triangle);
            int[] indices = new int[3];
            indices[0] = AddTriangle(triangle.v0, v3, triangle.v2);
            indices[1] = AddTriangle(triangle.v2, v3, triangle.v1);
            indices[2] = AddTriangle(triangle.v1, v3, triangle.v0);
            return indices;
        }

        public List<Triangle> GetNeighbors(Triangle triangle) {
            List<Triangle> result = new List<Triangle>();
            foreach (Vertex vertex in triangle.GetVertices()) {
                foreach (Triangle t in vertex.triangles) {
                    if (t != triangle && triangle.SharesEdgeWith(t) && !result.Contains(t)) {
                        result.Add(t);
                    }
                }
            }
            return result;
        }

        public void Clear() {
            vertices.Clear();
            triangles.Clear();
        }

        public Vector3 GetCenter() {
            Vector3 sum = Vector3.zero;
            foreach (Vertex vertex in vertices) {
                sum += vertex.pos;
            }
            return sum / vertices.Count;
        }

        protected Mesh BuildMesh() {
            Mesh mesh = new Mesh();
            int uniqueVertices = 0;
            foreach (Vertex vertex in vertices) {
                uniqueVertices += vertex.triangles.Count;
            }
            Vector3[] verts = new Vector3[uniqueVertices];
            Vector2[] uv = new Vector2[uniqueVertices];
            int[] tris = new int[3 * triangles.Count];
            Vector3[] normals = new Vector3[uniqueVertices];

            int vertIndex = 0;
            int trisIndex = 0;
            foreach (Triangle triangle in triangles) {
                Vector3 n = triangle.normal;
                verts[vertIndex] = triangle.v0.pos;
                uv[vertIndex] = triangle.uv0;
                tris[trisIndex++] = vertIndex;
                normals[vertIndex] = n;
                vertIndex++;
                verts[vertIndex] = triangle.v1.pos;
                uv[vertIndex] = triangle.uv1;
                tris[trisIndex++] = vertIndex;
                normals[vertIndex] = n;
                vertIndex++;
                verts[vertIndex] = triangle.v2.pos;
                uv[vertIndex] = triangle.uv2;
                tris[trisIndex++] = vertIndex;
                normals[vertIndex] = n;
                vertIndex++;
            }
            
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uv;
            mesh.normals = normals;
            mesh.name = "generated mesh";
            return mesh;
        }

        public void Build(GameObject target, Material material) {
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
            MeshFilter meshFilter = childByMaterial.GetComponent<MeshFilter>();
            if (meshFilter == null) {
                meshFilter = childByMaterial.AddComponent<MeshFilter>();
            }
            Mesh mesh = BuildMesh();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = childByMaterial.GetComponent<MeshRenderer>();
            if (meshRenderer == null) {
                meshRenderer = childByMaterial.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterial = material;
            //mesh.Optimize();
            //mesh.RecalculateNormals();
            MeshCollider meshCollider = childByMaterial.GetComponent<MeshCollider>();
            if (meshCollider == null) {
                meshCollider = childByMaterial.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = mesh;
        }

        public static bool SameInTolerance(Vector3 a, Vector3 b) {
            return (a-b).sqrMagnitude <= EpsilonSquared;
        }
    }
}
