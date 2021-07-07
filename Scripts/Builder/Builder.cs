using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public static class Builder {
        public static int CUTOUT = 1;
        public static List<Face> IndentFace(Face face, Vector3 direction, float uvScale=1f) {
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

        public static List<Face> Cutout(List<Face> fromFaces, Rect dim, float uvScale) {
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
        public static Face[] SliceFace(Face face, float dx, float dy) {
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

        public static List<Face> ExtrudeEdges(List<Vector3> vertices, Vector3 direction, float uvScale=1f) {
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

        /* Other than in Unity 2D this rect defines positive y up */
        public static Face ProjectRectOnFrontFace(Rect rect, float z) {
            Vector3 a = new Vector3(rect.x, rect.y, z);
            Vector3 b = new Vector3(rect.x, rect.y+rect.height, z);
            Vector3 c = new Vector3(rect.x+rect.width, rect.y+rect.height, z);
            Vector3 d = new Vector3(rect.x+rect.width, rect.y, z);
            return new Face(a, b, c, d);
        }
        public static Face FindFirstFaceByTag(List<Face> faces, int tag) {
            foreach (Face f in faces) {
                if (f.IsTagged(tag)) {
                    return f;
                }
            }
            return null;
        }

    }
}