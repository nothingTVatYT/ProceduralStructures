using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

namespace ProceduralStructures {
    public class MeshObject {

        protected class DeferredAction {
            public enum Command { SplitTriangleByLine }
            public Command command;
            public Triangle triangle;
            public Vector3 a;
            public Vector3 b;
            public static DeferredAction SplitTriangleAction(Triangle triangle, Vector3 a, Vector3 b) {
                DeferredAction deferredAction = new DeferredAction();
                deferredAction.command = Command.SplitTriangleByLine;
                deferredAction.triangle = triangle;
                deferredAction.a = a;
                deferredAction.b = b;
                return deferredAction;
            }
        }

        public enum Shading { Flat, Smooth, Auto }
        private static float Epsilon = 1e-3f;
        private static float EpsilonSquared = Epsilon*Epsilon;
        protected List<Vertex> vertices = new List<Vertex>();
        protected List<Triangle> triangles = new List<Triangle>();
        public float uvScale;
        public Shading shading = Shading.Flat;
        public Transform transform;
        public GameObject targetGameObject;
        public Material material;

        public float area {
            get {
                float a = 0;
                foreach (Triangle triangle in triangles) {
                    a += triangle.area;
                }
                return a;
            }
        }

        public virtual int Add(Vector3 pos) {
            return AddUnique(pos);
        }

        protected int AddUnique(Vector3 pos) {
            int idx = vertices.FindIndex((v) => (SameInTolerance(v.pos,pos)));
            if (idx >= 0) return idx;
            Vertex v = new Vertex(pos);
            v.id = vertices.Count;
            vertices.Add(v);
            return vertices.Count - 1;
        }

        protected int AddUnchecked(Vector3 pos) {
            Vertex v = new Vertex(pos);
            v.id = vertices.Count;
            vertices.Add(v);
            return vertices.Count - 1;
        }

        public virtual List<Vertex> AddRange(IEnumerable<Vector3> points) {
            List<int> result = new List<int>();
            foreach (Vector3 v in points) {
                int i = Add(v);
                if (!result.Contains(i)) {
                    result.Add(i);
                }
            }
            return VertexList(result);
        }

        public List<Vertex> VertexList(List<int> l) {
            List<Vertex> result = new List<Vertex>(l.Count);
            foreach (int i in l) {
                result.Add(vertices[i]);
            }
            return result;
        }

        public List<Triangle> TriangleList(IEnumerable<int> l) {
            List<Triangle> result = new List<Triangle>();
            foreach (int i in l) {
                result.Add(triangles[i]);
            }
            return result;
        }

        public int AddTriangle(Vertex v0, Vertex v1, Vertex v2) {
            Triangle triangle = new Triangle(v0, v1, v2);
            triangles.Add(triangle);
            // for visualizing only
            triangle.SetUVProjected(uvScale);
            return triangles.Count-1;
        }

        public int AddTriangle(Triangle triangle) {
            triangles.Add(triangle);
            return triangles.Count-1;
        }

        public void Remove(Triangle triangle) {
            triangle.RemoveTriangleLinks();
            triangles.Remove(triangle);
        }

        public void RemoveTriangles(IEnumerable<Triangle> triangleList) {
            foreach (Triangle t in triangleList) {
                //t.RemoveTriangleLinks();
                triangles.Remove(t);
            }
        }
        
        public void Remove(Vertex v) {
            RemoveTriangles(v.triangles);
            vertices.Remove(v);
        }

        public void AddObject(MeshObject other) {
            foreach (Triangle t in other.triangles) {
                Vertex v0 = vertices[AddUnique(t.v0.pos)];
                Vertex v1 = vertices[AddUnique(t.v1.pos)];
                Vertex v2 = vertices[AddUnique(t.v2.pos)];
                Triangle nt = triangles[AddTriangle(v0, v1, v2)];
                nt.uv0 = t.uv0;
                nt.uv1 = t.uv1;
                nt.uv2 = t.uv2;
            }
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

        public void ScaleVertices(List<Vertex> list, Vector3 scale) {
            foreach (Vertex v in list) {
                v.pos = Vector3.Scale(v.pos, scale);
            }
        }

        public void RandomizeVertices(Vector3 displacement) {
            foreach (Vertex vertex in vertices) {
                vertex.pos = vertex.pos + displacement * (Random.value - 0.5f);
            }
        }

        public void SetUVCylinderProjection(IEnumerable<int> triangleIndices, Vector3 center, Vector3 direction, float uOffset, float uvScale) {
            foreach (int ti in triangleIndices) {
                Triangle triangle = triangles[ti];
                triangle.SetUVCylinderProjection(center, direction, uOffset, uvScale);
            }
        }

        public void ClampToPlane(List<Vertex> front, List<Vertex> back, Vector3 plane, Vector3 normal) {
            for (int i = 0; i < front.Count; i++) {
                // if vertex is behind the plane (not on normal side) project it on the plane
                float dot = Vector3.Dot(front[i].pos - plane, normal);
                if (dot < 0) {
                    Vector3 v = front[i].pos - plane;
                    float dist = v.x*normal.x + v.y*normal.y + v.z*normal.z;
                    Vector3 projected = front[i].pos - dist*normal;
                    // collapse front and back vertices
                    front[i].pos = projected;
                    back[i].pos = projected;
                }
            }
        }

        public int[] CreateTriangleFan(List<Vertex> l) {
            Vector3 centroid = GetCenter(l);
            Vertex fanCenter = vertices[AddUnique(centroid)];
            List<int> result = new List<int>();
            if (l.Count >= 3) {
                for (int i = 0; i < l.Count; i++) {
                    int j = i < l.Count-1 ? i+1 : 0;
                    result.Add(AddTriangle(fanCenter, l[i], l[j]));
                }
            }
            return result.ToArray();
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
            if (!triangles.Remove(triangle)) {
                Debug.LogWarning("Could not remove " + triangle + ".");
            }
            triangle.RemoveTriangleLinks();
            int[] indices = new int[3];
            indices[0] = AddTriangle(triangle.v0, triangle.v1, v3);
            indices[1] = AddTriangle(triangle.v1, triangle.v2, v3);
            indices[2] = AddTriangle(triangle.v2, triangle.v0, v3);
            //Debug.Log("Created " + triangles[indices[0]] + " and " + triangles[indices[1]] + " and " + triangles[indices[2]] + " from " + triangle);
            return indices;
        }

        public List<Triangle> SplitTriangleByLine(Triangle triangle, Vector3 a, Vector3 b) {
            List<Triangle> result = new List<Triangle>();
            // calculate the two new vertices, i.e. the intersections of a line through ab and two edges
            Vector3 iv;
            Vertex v3, v4;
            DebugLocalLine(a, b, Color.yellow);
            DebugLocalLine(triangle.v0.pos, triangle.v1.pos, Color.red);
            if (GeometryTools.EdgeLineIntersect(triangle.v0.pos, triangle.v1.pos, a, b, out iv)) {
                v3 = vertices[Add(iv)];
                if (GeometryTools.EdgeLineIntersect(triangle.v1.pos, triangle.v2.pos, a, b, out iv)) {
                    // intersect v0v1 and v1v2
                    triangle.RemoveTriangleLinks();
                    result.Add(triangle);
                    v4 = vertices[Add(iv)];
                    result.Add(triangles[AddTriangle(v3, triangle.v1, v4)]);
                    result.Add(triangles[AddTriangle(triangle.v0, v3, v4)]);
                    triangle.v1 = v4;
                    triangle.ResetTriangleLinks();
                } else if (GeometryTools.EdgeLineIntersect(triangle.v2.pos, triangle.v0.pos, a, b, out iv)) {
                    // intersect v0v1 and v2v0
                    triangle.RemoveTriangleLinks();
                    result.Add(triangle);
                    v4 = vertices[Add(iv)];
                    result.Add(triangles[AddTriangle(v3, triangle.v1, triangle.v2)]);
                    result.Add(triangles[AddTriangle(v3, triangle.v2, v4)]);
                    triangle.v1 = v3;
                    triangle.v2 = v4;
                    triangle.ResetTriangleLinks();
                }
            } else if (GeometryTools.EdgeLineIntersect(triangle.v1.pos, triangle.v2.pos, a, b, out iv)) {
                v3 = vertices[Add(iv)];
                if (GeometryTools.EdgeLineIntersect(triangle.v2.pos, triangle.v0.pos, a, b, out iv)) {
                    // intersect v1v2 and v2v0
                    triangle.RemoveTriangleLinks();
                    result.Add(triangle);
                    v4 = vertices[Add(iv)];
                    result.Add(triangles[AddTriangle(triangle.v0, v3, v4)]);
                    result.Add(triangles[AddTriangle(v4, v3, triangle.v2)]);
                    triangle.v2 = v3;
                    triangle.ResetTriangleLinks();
                }
            }
            return result;
        }

        public void SplitBigTriangles(float maxRelativeSize, float offset) {
            float totalArea = area;
            Vector3 center = GetCenter();
            List<Triangle> trianglesToSplit = new List<Triangle>();
            do {
                foreach (Triangle triangle in triangles) {
                    if (triangle.area / totalArea > maxRelativeSize) {
                        trianglesToSplit.Add(triangle);
                    }
                }
                if (trianglesToSplit.Count > 0) {
                    foreach (Triangle triangle in trianglesToSplit) {
                        int vIdx = Add(triangle.center + (triangle.center - center).normalized * offset);
                        if (vIdx != vertices.Count-1) {
                            Debug.LogWarning("the vertex is reused.");
                        }
                        Vertex n = vertices[vIdx];
                        SplitTriangle(triangle, n);
                    }
                    trianglesToSplit.Clear();
                }
            } while (trianglesToSplit.Count > 0);
        }

        public int[] BridgeEdgeLoops(List<Vertex> fromVertices, List<Vertex> toVertices, float uvScale = 1f) {
            List<int> indices = new List<int>();
            CircularReadonlyList<Vertex> fromRing = new CircularReadonlyList<Vertex>(fromVertices);
            CircularReadonlyList<Vertex> toRing = new CircularReadonlyList<Vertex>(toVertices);
            List<Face> faces = new List<Face>();
            for (int i = 0; i < fromRing.Count; i++) {
                indices.Add(AddTriangle(fromRing[i], fromRing[i+1], toRing[i+1]));
                indices.Add(AddTriangle(fromRing[i], toRing[i+1], toRing[i]));
            }
            return indices.ToArray();
        }

        public int[] FillPolygon(List<Vertex> edgeLoop, List<Vertex> hole) {
            List<int> createdTriangles = new List<int>();

            // Debug.Log("polygon has " + edgeLoop.Count + " vertices.");
            // Debug.Log("hole has " + hole.Count + " vertices.");
            hole.Reverse();

            int vIdx = 0;
            int hIdx = 0;
            for (int h = 0; h < hole.Count; h++) {
                Vertex v = hole[h];
                for (int i = 0; i < edgeLoop.Count-1; i++) {
                    int j = i < edgeLoop.Count-2 ? i+1 : 0;
                    Triangle t = new Triangle(v, edgeLoop[j], edgeLoop[i]);
                    if (t.ContainsAnyVertex(hole) || t.ContainsAnyVertex(edgeLoop)) continue;
                    vIdx = i;
                    hIdx = h;
                    createdTriangles.Add(AddTriangle(t));
                    break;
                }
                if (createdTriangles.Count > 0) break;
            }
            if (createdTriangles.Count == 0) {
                Debug.LogWarning("Could not create a starting triangle to close the edge loop.");
            } else {
                List<Vertex> polygon = new List<Vertex>();
                polygon.AddRange(edgeLoop.GetRange(0, vIdx+1));
                polygon.AddRange(hole.GetRange(hIdx, hole.Count-hIdx));
                polygon.AddRange(hole.GetRange(0, hIdx+1));
                polygon.AddRange(edgeLoop.GetRange(vIdx+1, edgeLoop.Count-vIdx-1));
                
                // for (int i = 0; i < polygon.Count; i++) {
                //     DebugLocalPoint(polygon[i].pos, "pv-"+i+"-"+polygon[i].id);
                // }
                createdTriangles.AddRange(FillPolygon(polygon));
            }
            return createdTriangles.ToArray();
        }

        public int[] FillPolygon(List<Vertex> edgeLoop) {
            Vector3 center = GetCenter(edgeLoop);
            Vector3 normal = -Vector3.Cross(edgeLoop[1].pos-edgeLoop[0].pos, edgeLoop[2].pos-edgeLoop[0].pos).normalized;
            LinkedList<Vertex> list = new LinkedList<Vertex>(edgeLoop);
            LinkedListNode<Vertex> current = list.First;
            List<int> createdTriangles = new List<int>();
            int tAtOverflow = 0;
            while (list.Count > 2) {
                // search a vertex that has an inner angle of less than 180 forming with its neighbors
                LinkedListNode<Vertex> nextNeighbor = current.Next ?? list.First;
                Vector3 tangent = nextNeighbor.Value.pos - current.Value.pos;
                //DebugLocalVector(current.Value.pos, tangent, Color.green);
                LinkedListNode<Vertex> secondNextNeighbor = nextNeighbor.Next ?? list.First;
                Vector3 nextTangent = secondNextNeighbor.Value.pos - nextNeighbor.Value.pos;
                //DebugLocalVector(nextNeighbor.Value.pos, nextTangent, Color.green);
                Vector3 vin = Vector3.Cross(tangent, normal);
                //DebugLocalVector(current.Value.pos, vin, Color.red);
                Vector3 nextIn = Vector3.Cross(nextTangent, normal);
                //DebugLocalVector(nextNeighbor.Value.pos, nextIn, Color.red);
                if (Vector3.Dot(vin, nextTangent) > 0) {
                    Triangle tNew = new Triangle(current.Value, secondNextNeighbor.Value, nextNeighbor.Value);
                    if (tNew.ContainsAnyVertex(list)) {
                        // this would be an invalid triangle
                        current = current.Next ?? list.First;
                        tNew.RemoveTriangleLinks();
                    } else {
                        createdTriangles.Add(AddTriangle(tNew));
                        LinkedListNode<Vertex> newStartingPoint = secondNextNeighbor;
                        list.Remove(nextNeighbor);
                    }
                } else {
                    if (current.Next == null) {
                        // inhibit endless loops
                        if (createdTriangles.Count == tAtOverflow) {
                            Debug.LogWarning("Could not create any more triangles with " + list.Count + " vertices left.");
                            break;
                        }
                        tAtOverflow = createdTriangles.Count;
                    }
                    current = current.Next ?? list.First;
                }
                //if (createdTriangles.Count > 0) break;
            }
            return createdTriangles.ToArray();
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

        public Vector3 GetCenter(List<Vertex> l) {
            Vector3 sum = Vector3.zero;
            foreach (Vertex vertex in l) {
                sum += vertex.pos;
            }
            return sum / l.Count;
        }

        public void FlipNormals() {
            foreach (Triangle triangle in triangles) {
                triangle.FlipNormal();
            }
        }

        public void FlipNormals(IEnumerable<int> l) {
            foreach (int i in l) {
                triangles[i].FlipNormal();
            }
        }

        public void SetUVBoxProjection(float uvScale) {
            foreach (Triangle triangle in triangles) {
                triangle.SetUVProjected(uvScale);
            }
        }

        public void SetUVBoxProjection(IEnumerable<int> l, float uvScale) {
            foreach (int i in l) {
                triangles[i].SetUVProjected(uvScale);
            }
        }

        public Vector3 WorldPosition(Vertex v) {
            if (transform != null) {
                return transform.TransformPoint(v.pos);
            }
            return v.pos;
        }

        public Vector3 WorldPosition(Vector3 v) {
            if (transform != null) {
                return transform.TransformPoint(v);
            }
            return v;
        }

        public Vector3 LocalPosition(Vector3 v) {
            if (transform != null) {
                return transform.InverseTransformPoint(v);
            }
            return v;
        }

        public bool TriangleTriangleIntersection(Triangle triangle1, Triangle triangle2, out Vector3[] intersections) {
            int verticesInFront = 0;
            int verticesBehind = 0;
            foreach  (Vertex v in triangle2.GetVertices()) {
                if (triangle1.FacesPoint(v.pos)) {
                    verticesInFront++;
                } else {
                    verticesBehind++;
                }
            }
            if (verticesInFront == 0 || verticesBehind == 0) {
                intersections = new Vector3[0];
                return false;
            }
            List<Vector3> intersectionPoints = new List<Vector3>();
            Vector3 intersection;
            if (triangle1.EdgeIntersection(triangle2.v0.pos, triangle2.v1.pos, out intersection)) {
                intersectionPoints.Add(intersection);
            }
            if (triangle1.EdgeIntersection(triangle2.v1.pos, triangle2.v2.pos, out intersection)) {
                intersectionPoints.Add(intersection);
            }
            if (triangle1.EdgeIntersection(triangle2.v2.pos, triangle2.v0.pos, out intersection)) {
                intersectionPoints.Add(intersection);
            }
            if (triangle2.EdgeIntersection(triangle1.v0.pos, triangle1.v1.pos, out intersection)) {
                intersectionPoints.Add(intersection);
            }
            if (triangle2.EdgeIntersection(triangle1.v1.pos, triangle1.v2.pos, out intersection)) {
                intersectionPoints.Add(intersection);
            }
            if (triangle2.EdgeIntersection(triangle1.v2.pos, triangle1.v0.pos, out intersection)) {
                intersectionPoints.Add(intersection);
            }
            if (intersectionPoints.Count == 0) {
                intersections = new Vector3[0];
                return false;
            }
            if (intersectionPoints.Count != 2) {
                Debug.LogWarning("TriangleTriangleIntersection found " + intersectionPoints.Count + " points.");
                intersections = intersectionPoints.ToArray();
                return false;
            }
            intersections = intersectionPoints.ToArray();
            return true;
        }

        public void AddConnector(MeshObject other) {
            List<Triangle> changedTriangles = new List<Triangle>();
            Vector3 center = GetCenter();
            Vector3 otherCenter = LocalPosition(other.WorldPosition(other.GetCenter()));
            Vector3 projectionDirection = (center - otherCenter).normalized;
            Vertex centerVertex = new Vertex(center + (center - otherCenter) * 10f);

            //DebugLocalPoint(center, "DEBUG-Center");
            List<Vertex> newVertices = new List<Vertex>();
            List<Vertex> addedVertices = new List<Vertex>();
            foreach (Vertex v in other.vertices) {
                newVertices.Add(vertices[Add(LocalPosition(other.WorldPosition(v)))]);
            }
            CircularReadonlyList<Vertex> ring = new CircularReadonlyList<Vertex>(newVertices);
            for (int i = 0; i < newVertices.Count; i++) {
                Triangle cutTriangle = new Triangle(centerVertex, ring[i], ring[i+1]);
                List<DeferredAction> deferredActions = new List<DeferredAction>();
                foreach (Triangle triangle in triangles) {
                    if (triangle.FacesPoint(ring[i].pos)) {
                        bool fromback;
                        Vector3 intersection;
                        if (triangle.RayHit(ring[i].pos, projectionDirection, false, out fromback, out intersection)) {
                            addedVertices.Add(vertices[Add(intersection)]);
                        }
                    }
                }
            }

            MeshObject cutObject = new MeshObject();
            cutObject.transform = transform;
            List<Vertex> fromVertices = new List<Vertex>();
            List<Vertex> toVertices = new List<Vertex>();
            foreach (Vertex v in addedVertices) {
                Vector3 localPos = v.pos;
                toVertices.Add(cutObject.vertices[cutObject.Add(localPos + projectionDirection * 0.5f)]);
                fromVertices.Add(cutObject.vertices[cutObject.Add(localPos - projectionDirection * 0.5f)]);
            }
            cutObject.BridgeEdgeLoops(fromVertices, toVertices, 1);
            cutObject.CreateTriangleFan(toVertices);
            cutObject.FlipNormals(cutObject.CreateTriangleFan(fromVertices));

            
            //cutObject.Build(other.targetGameObject, other.material);
            List<Vertex> outerVertices = RemoveEverythingInside(cutObject);
            int[] createdTriangles = FillPolygon(outerVertices, addedVertices);
            newVertices.Reverse();
            BridgeEdgeLoops(addedVertices, newVertices, uvScale);
        }

        public List<Vertex> RemoveEverythingInside(MeshObject other) {
            HashSet<Vertex> affectedVertices = new HashSet<Vertex>();
            HashSet<Triangle> affectedTriangles = new HashSet<Triangle>();
            HashSet<Vertex> outerVertices = new HashSet<Vertex>();
            HashSet<Vertex> innerVertices = new HashSet<Vertex>();
            // first check vertices
            foreach (Vertex v in vertices) {
                if (other.Contains(v.pos)) {
                    affectedVertices.Add(v);
                    foreach (Triangle t in v.triangles) {
                        affectedTriangles.Add(t);
                        outerVertices.Add(t.v0);
                        outerVertices.Add(t.v1);
                        outerVertices.Add(t.v2);
                    }
                }
            }
            foreach (Triangle t in triangles) {
                if (affectedTriangles.Contains(t)) {
                    continue;
                }
                foreach (Triangle ot in other.triangles) {
                    Vector3[] intersections;
                    if (TriangleTriangleIntersection(t, ot, out intersections)) {
                        affectedTriangles.Add(t);
                        outerVertices.Add(t.v0);
                        outerVertices.Add(t.v1);
                        outerVertices.Add(t.v2);
                        foreach (Vector3 i in intersections) {
                            innerVertices.Add(new Vertex(i));
                        }
                        break;
                    }
                }
            }
            outerVertices.RemoveWhere( v => affectedVertices.Contains(v));

            // Debug.Log("Found " + affectedVertices.Count + " vertices and " + affectedTriangles.Count + " triangles affected.");
            // Debug.Log("Found " + innerVertices.Count + " inner and " + outerVertices.Count + " outer vertices.");
            // foreach (Vertex v in outerVertices) {
            //     DebugLocalPoint(v.pos, Color.blue);
            // }
            // foreach (Vertex v in innerVertices) {
            //     DebugLocalPoint(v.pos, Color.red);
            // }
            foreach(Vertex v in affectedVertices) {
                Remove(v);
            }
            foreach(Triangle t in affectedTriangles) {
                Remove(t);
            }

            outerVertices.RemoveWhere(v => v.triangles.Count == 0);
            List<Vertex> ov = new List<Vertex>(outerVertices);
            return SortConnectedVertices(ov);
        }

        public List<Vertex> SortConnectedVertices(ICollection<Vertex> l) {
            List<Vertex> sortedList = new List<Vertex>();
            IEnumerator<Vertex> iter = l.GetEnumerator();
            iter.MoveNext();
            Vertex start = iter.Current;
            if (!vertices.Contains(start)) {
                Debug.LogWarning("Found a starting vertex " + start + " that is not part of this object.");
            }
            Vertex next = null;
            sortedList.Add(start);
            l.Remove(start);
            while (l.Count > 0) {
                foreach (Triangle triangle in start.triangles) {
                    // sanity check
                    if (!triangles.Contains(triangle)) {
                        Debug.LogWarning("A triangle is linked in " + start + " which is not part of this object: " + triangle);
                        continue;
                    }
                    foreach (Vertex v in triangle.GetVertices()) {
                        if (v != start && l.Contains(v) && !IsSharedEdge(start, v)) {
                            next = v;
                            break;
                        }
                    }
                    if (next != null) break;
                }
                if (next != null) {
                    sortedList.Add(next);
                    l.Remove(next);
                    start = next;
                    if (!vertices.Contains(next)) {
                        Debug.LogWarning("Found a vertex " + next + " that is not part of this object.");
                        break;
                    }
                    next = null;
                } else {
                    Debug.LogWarning("The list is not connected. " + l.Count + " vertices are left.");
                    break;
                }
            }
            return sortedList;
        }

        public bool IsSharedEdge(Vertex a, Vertex b) {
            List<Triangle> common = a.triangles.FindAll(t => b.triangles.Contains(t));
            return common.Count > 1;
        }

        public bool Contains(Vector3 v) {
            foreach (Triangle t in triangles) {
                if (!IsBehind(t, v)) {
                    return false;
                }
            }
            return true;
        }

        public void Decimate(int maxVertices) {
            while (vertices.Count > maxVertices) {
                float minDist = float.MaxValue;
                int i1 = 0;
                int i2 = 0;
                for (int i = 0; i < vertices.Count-1; i++) {
                    for (int j = i+1; j < vertices.Count; j++) {
                        float dist = (vertices[i].pos - vertices[j].pos).magnitude;
                        if (dist < minDist) {
                            minDist = dist;
                            i1 = i; i2 = j;
                        }
                    }
                }
                vertices[i1].pos = (vertices[i1].pos + vertices[i2].pos)/2;
                vertices.RemoveAt(i2);
            }
        }

        public void DebugLocalPoint(Vector3 vector3, string name) {
            GameObject go = new GameObject(name);
            if (transform != null) {
                go.transform.position = transform.TransformPoint(vector3);
            } else {
                go.transform.position = vector3;
            }
        }

        public void DebugLocalLine(Vector3 a, Vector3 b, Color color) {
            Vector3 start = a - (b-a).normalized * 10f;
            Vector3 end = b + (b-a).normalized * 10f;
            Debug.DrawLine(transform.TransformPoint(start), transform.TransformPoint(end), color, 5);
        }

        public void DebugLocalVector(Vector3 a, Vector3 direction, Color color) {
            Vector3 start = a;
            Vector3 end = a + direction * 2f;
            Debug.DrawLine(transform.TransformPoint(start), transform.TransformPoint(end), color, 5);
        }

        public void DebugLocalPoint(Vector3 a, Color color) {
            Vector3 w = transform.TransformPoint(a);
            float d = 0.5f;
            Debug.DrawLine(w-Vector3.right*d, w+Vector3.right*d, color, 5);
            Debug.DrawLine(w-Vector3.up*d, w+Vector3.down*d, color, 5);
            Debug.DrawLine(w-Vector3.forward*d, w+Vector3.back*d, color, 5);
        }

        string ToString(Vector3[] l) {
            string result = "[";
            foreach (Vector3 v in l) {
                result += string.Format("({0:F4},{1:F4},{2:F4})", v.x, v.y, v.z);
            }
            result += "]";
            return result;
        }
        protected Mesh BuildMesh() {
            Mesh mesh = new Mesh();
            int uniqueVertices = 0;
            foreach (Vertex vertex in vertices) {
                uniqueVertices += vertex.triangles.Count;
            }
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            int[] tris = new int[3 * triangles.Count];
            List<Vector3> normals = new List<Vector3>();

            int vertIndex = 0;
            int trisIndex = 0;
            foreach (Triangle triangle in triangles) {
                Vector3 n = triangle.normal;
                verts.Add(triangle.v0.pos);
                uv.Add(triangle.uv0);
                tris[trisIndex++] = vertIndex;
                normals.Add(CalculateNormal(triangle.v0, triangle));
                vertIndex++;
                verts.Add(triangle.v1.pos);
                uv.Add(triangle.uv1);
                tris[trisIndex++] = vertIndex;
                normals.Add(CalculateNormal(triangle.v1, triangle));
                vertIndex++;
                verts.Add(triangle.v2.pos);
                uv.Add(triangle.uv2);
                tris[trisIndex++] = vertIndex;
                normals.Add(CalculateNormal(triangle.v2, triangle));
                vertIndex++;
            }
            
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris;
            mesh.uv = uv.ToArray();
            mesh.normals = normals.ToArray();
            mesh.name = "generated mesh";
            return mesh;
        }

        Vector3 CalculateNormal(Vertex v, Triangle triangle) {
            Vector3 n = Vector3.zero;
            int ct = 0;
            switch (shading) {
                case Shading.Flat:
                    n = triangle.normal;
                    break;
                case Shading.Smooth:
                    foreach (Triangle t in v.triangles) {
                        n += t.normal;
                    }
                    n /= v.triangles.Count;
                    break;
                case Shading.Auto:
                    n = triangle.normal;
                    ct = 1;
                    foreach (Triangle t in v.triangles) {
                        if (t != triangle && Vector3.Angle(triangle.normal, t.normal) < 60f) {
                            n += t.normal;
                            ct++;
                        }
                    }
                    n /= ct;
                    break;
            }
            return n;
        }
        public void Build(GameObject target, Material material) {
                        GameObject childByMaterial = null;
            if (material == null) {
                material = new Material(Shader.Find("Standard"));
            }
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
