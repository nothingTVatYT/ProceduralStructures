using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GK;

namespace ProceduralStructures {
    public class ConvexHull : MeshObject {
        List<Vector3> points = new List<Vector3>();

        public void AddPoint(Vector3 p) {
            points.Add(p);
        }

        public void CalculateHull() {
            bool splitverts = false;
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            ConvexHullCalculator chc = new ConvexHullCalculator();
            chc.GenerateHull(points, splitverts, ref verts, ref tris, ref normals);

            vertices.Clear();
            triangles.Clear();
            foreach (Vector3 v in verts) {
                AddUnchecked(v);
            }
            for (int i = 0; i < tris.Count; i+=3) {
                triangles.Add(new Triangle(vertices[tris[i]], vertices[tris[i+1]], vertices[tris[i+2]]));
            }
        }
    }
}
