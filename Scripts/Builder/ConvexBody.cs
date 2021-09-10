using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

namespace ProceduralStructures {
    public class ConvexBody : MeshObject {

        private Vector3 center;
        public VertexListRecorder recorder;
        public bool fixSlivers = true;

        public int AddPoint(Vector3 vertex) {
            int vIdx = AddUnique(vertex);
            if (recorder != null) recorder.vertices.Add(vertex);
            Stack<Triangle> slivers = new Stack<Triangle>();
            if (vertices[vIdx].triangles.Count > 0) {
                Debug.Log("An already existing vertex is added again. " + vertex);
                return vIdx;
            }
            if (VerticesCount == 3 && TrianglesCount == 0) {
                AddTriangle(vertices[0], vertices[1], vertices[2]);
            } else if (VerticesCount == 4 && TrianglesCount == 1) {
                if (!IsBehind(triangles[0], vertex)) {
                    FlipNormals();
                }
                MakePyramid(triangles[0], vertices[vIdx]);
            } else if (vertices.Count > 4) {
                Vertex addedVertex = vertices[vIdx];
                List<Triangle> facingPoint = triangles.FindAll((t) => t.FacesPoint(vertex));
                if (facingPoint.Count > 0) {
                    center = GetCenter();
                    Debug.Log("Remove facing triangles: " + facingPoint.Elements());
                    triangles.RemoveAll(t => facingPoint.Contains(t));
                    facingPoint.ForEach(t => t.RemoveTriangleLinks());
                    HashSet<Vertex> h = new HashSet<Vertex>();
                    List<Vertex> boundaryVertices = new List<Vertex>();
                    foreach (Triangle triangle in facingPoint) {
                        foreach (Vertex v in triangle.GetVertices()) {
                            h.Add(v);
                            /*
                            if (v.triangles.FindAll(t => !facingPoint.Contains(t)).Count > 0) {
                                if (!boundaryVertices.Contains(v))
                                    boundaryVertices.Add(v);
                            }
                            */
                        }
                    }
                    h.RemoveWhere(v => !vertices.Contains(v));
                    boundaryVertices.AddRange(h);
                    //boundaryVertices.RemoveAll(v => !vertices.Contains(v));
                    if (boundaryVertices.Count == 0) {
                        Debug.Log("No boundary vertices. Triangles facing the new vertex are: " + facingPoint.Elements());
                    }
                    Debug.Log("Boundary vertices before: " + boundaryVertices.Count);
                    List<Vertex> sorted = FindBoundaryAround(boundaryVertices);
                    boundaryVertices = sorted;
                    Debug.Log("Boundary vertices after: " + boundaryVertices.Count);
                    // double check the order
                    Triangle test = new Triangle(boundaryVertices[0], addedVertex, boundaryVertices[1]);
                    bool valid = !test.FacesPoint(center);
                    test.RemoveTriangleLinks();
                    if (!valid) {
                        test = new Triangle(boundaryVertices[0], boundaryVertices[1], addedVertex);
                        valid = !test.FacesPoint(center);
                        test.RemoveTriangleLinks();
                        if (!valid) {
                            // we're lost ?!?
                            Debug.Log("triangles with the boundary vertices don't work in either direction");
                        } else {
                            boundaryVertices.Reverse();
                        }
                    }
                    for (int i = 0; i < boundaryVertices.Count; i++) {
                        int j = i + 1;
                        if (j >= boundaryVertices.Count) j=0;
                        Triangle t = triangles[AddTriangle(boundaryVertices[i], addedVertex, boundaryVertices[j])];
                        float phi = t.MaxAngle();
                        if (phi >= 170f) {
                            Debug.Log("A sliver (phi=" + phi + ") was just created: " + t + " and normal=" + t.normal);
                            slivers.Push(t);
                        }
                    }
                    /*
                    Vertex v0 = boundaryVertices[0];
                    Vertex vBegin = v0;
                    do {
                        int c = boundaryVertices.Count;
                        bool triangleCreated = false;
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
                                    float phi = t.MaxAngle();
                                    if (phi >= 170f) {
                                        Debug.Log("A sliver (phi="+phi+") was just created: " + t + " and normal=" + t.normal);
                                        slivers.Push(t);
                                    }
                                    v0 = v1;
                                    triangleCreated = true;
                                    break;
                                }
                            }
                        }
                        if (!triangleCreated) {
                            Debug.LogWarning("Triangulation hasn't progressed, possibly an endless loop.");
                            break;
                        }
                    } while (v0 != vBegin);
                    */
                } else {
                    // this vertex was inside of the hull
                    Remove(addedVertex);
                }
            }
            if (fixSlivers) {
                int corrected = 0;
                while (slivers.Count > 0) {
                    Triangle t = slivers.Pop();
                    Triangle neighbor = t.GetNearestAdjacentTriangleByNormal(5f);
                    // swap edges
                    if (neighbor != null) {
                        if (SwapEdges(t, neighbor)) {
                            Debug.Log("Fixed by swapping edges.");
                            corrected++;
                        } else {
                            Debug.Log("Could not fix " + t);
                        }
                    }
                    if (corrected >= 1) break;
                }
            }
            center = GetCenter();

            return vIdx;
        }
    }
}