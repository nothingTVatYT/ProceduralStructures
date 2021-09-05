using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

namespace ProceduralStructures {
    public class ConvexBody : MeshObject {

        private Vector3 center;

        public override int Add(Vector3 vertex) {
            int vIdx = base.Add(vertex);
            if (vertices.Count == 3 && triangles.Count == 0) {
                AddTriangle(vertices[0], vertices[1], vertices[2]);
            } else if (vertices.Count == 4 && triangles.Count == 1) {
                if (!IsBehind(triangles[0], vertex)) {
                    triangles[0].FlipNormal();
                }
                MakePyramid(triangles[0], vertices[vIdx]);
            } else if (vertices.Count > 4) {
                Vertex addedVertex = vertices[vIdx];
                List<Triangle> facingPoint = triangles.FindAll((t) => t.FacesPoint(vertex));
                if (facingPoint.Count > 0) {
                    triangles.RemoveAll(t => facingPoint.Contains(t));
                    facingPoint.ForEach(t => t.RemoveTriangleLinks());
                    List<Vertex> boundaryVertices = new List<Vertex>();
                    foreach (Triangle triangle in facingPoint) {
                        foreach (Vertex v in triangle.GetVertices()) {
                            if (v.triangles.FindAll(t => !facingPoint.Contains(t)).Count > 0) {
                                boundaryVertices.Add(v);
                            }
                        }
                    }
                    Vertex v0 = boundaryVertices[0];
                    Vertex vBegin = v0;
                    do {
                        int c = boundaryVertices.Count;
                        for (int i = 0; i < boundaryVertices.Count; i++) {
                            Vertex v1 = boundaryVertices[i];
                            if (v0 != v1) {
                                bool invalid = false;
                                for (int j = 0; j < boundaryVertices.Count; j++) {
                                    Vertex v2 = boundaryVertices[j];
                                    if (v2 != v0 && v2 != v1) {
                                        if (Triangle.FacesPoint(vertex, v0.pos, v1.pos, v2.pos)) {
                                            invalid = true;
                                            break;
                                        }
                                    }
                                }
                                if (!invalid) {
                                    Triangle t = new Triangle(addedVertex, v0, v1);
                                    t.SetUVProjected(uvScale);
                                    triangles.Add(t);
                                    v0 = v1;
                                    break;
                                }
                            }
                        }
                    } while (v0 != vBegin);
                }
            }
            center = GetCenter();
            return vIdx;
        }
    }
}