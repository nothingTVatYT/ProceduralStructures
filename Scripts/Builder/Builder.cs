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

        public static List<Face> Cutout(List<Face> fromFaces, Rect dim, Vector3 cutoutOrigin, float uvScale) {
            List<Face> result = new List<Face>();
            // project the 2D rect on this face (the normal needs to point to Vector3.back)
            // the z is not used so we don't care
            // check which faces are affected
            Quaternion localToWorld = Quaternion.identity;
            foreach (Face face in fromFaces) {
                List<Face> n = new List<Face>();
                // is the normal of this face already pointing to us?
                Quaternion rotation = Quaternion.identity;
                if (face.normal != Vector3.back) {
                    Vector3 originalDirection = face.normal;
                    rotation = Quaternion.FromToRotation(originalDirection, Vector3.back);
                    face.Rotate(rotation);
                    localToWorld = Quaternion.FromToRotation(Vector3.back, originalDirection);
                }
                Vector3 localOrigin = rotation * cutoutOrigin;
                Face cutoutFace = ProjectRectOnFrontFace(dim, 0).MoveFaceBy(localOrigin);
                // is the cutout part of this face?
                if (cutoutFace.a.x >= face.a.x && cutoutFace.a.y >= face.a.y && cutoutFace.c.x <= face.c.x && cutoutFace.c.y <= face.c.y) {
                    // slice the face into 9 parts leaving the center piece at the size of the cutout
                    Face[] nf = SliceFace(face, cutoutFace.a.x - face.a.x, 0);

                    Face leftColumn = nf[0];
                    Face[] nf2 = SliceFace(nf[1], cutoutFace.d.x - nf[1].a.x, 0);
                    Face middleColumn = nf2[0];
                    Face rightColumn = nf2[1];
                    
                    nf = SliceFace(leftColumn, 0, cutoutFace.a.y - leftColumn.a.y);
                    n.Add(nf[0]); // bottom left corner
                    nf2 = SliceFace(nf[1], 0, cutoutFace.b.y-nf[1].a.y);
                    n.Add(nf2[1]); // top left corner
                    n.Add(nf2[0]); // left middle

                    nf = SliceFace(middleColumn, 0, cutoutFace.a.y - middleColumn.a.y);
                    n.Add(nf[0]); // bottom center
                    nf2 = SliceFace(nf[1], 0, cutoutFace.b.y - nf[1].a.y);
                    //result.Add(nf2[0]); // center
                    nf2[0].Tag(CUTOUT);
                    n.AddRange(IndentFace(nf2[0], new Vector3(0, 0, 0.3f), uvScale));
                    n.Add(nf2[1]); // top center
                    nf = SliceFace(rightColumn, 0, cutoutFace.a.y-rightColumn.a.y);
                    n.Add(nf[0]); // bottom right corner
                    nf2 = SliceFace(nf[1], 0, cutoutFace.b.y-nf[1].a.y);
                    n.Add(nf2[0]); // right middle
                    n.Add(nf2[1]); // top right corner
                    foreach (Face turned in n) {
                        turned.Rotate(localToWorld);
                    }
                    result.AddRange(n);
                } else {
                    Debug.Log("cutout is not part of this face. cutout=" + cutoutFace + ", face=" + face);
                    // rotate it back
                    face.Rotate(localToWorld);
                    result.Add(face);
                }
            }
            return result;
        }

        public static Face[] SplitFaceABCD(Face face, float rAB, float rCD) {
            Vector3 mab = Vector3.Lerp(face.a, face.b, rAB);
            Vector3 mcd = Vector3.Lerp(face.c, face.d, rCD);
            Face f1 = new Face(mab, face.b, face.c, mcd);
            Face f2 = new Face(face.a, mab, mcd, face.d);
            f1.uvA = Vector2.Lerp(face.uvA, face.uvB, rAB);
            f1.uvB = face.uvB;
            f1.uvC = face.uvC;
            f1.uvD = Vector2.Lerp(face.uvC, face.uvD, rCD);
            f2.uvA = face.uvA;
            f2.uvB = f1.uvA;
            f2.uvC = f1.uvD;
            f2.uvD = face.uvD;
            return new Face[] { f1, f2 };
        }

        public static Face[] SplitFaceBCDA(Face face, float rBC, float rDA) {
            Vector3 mbc = Vector3.Lerp(face.b, face.c, rBC);
            Vector3 mda = Vector3.Lerp(face.d, face.a, rDA);
            Face f1 = new Face(face.a, face.b, mbc, mda);
            Face f2 = new Face(mda, mbc, face.c, face.d);
            f1.uvA = face.uvA;
            f1.uvB = face.uvB;
            f1.uvC = Vector2.Lerp(face.uvB, face.uvC, rBC);
            f1.uvD = Vector2.Lerp(face.uvD, face.uvA, rDA);
            f2.uvA = f1.uvD;
            f2.uvB = f1.uvC;
            f2.uvC = face.uvC;
            f2.uvD = face.uvD;
            return new Face[] { f1, f2 };
        }

        public static Face[] SliceFace(Face face, Vector3 v0, Vector3 v1) {
            if (Vector3.Dot(face.d-face.a, v1-face.a) < 0.999f) {
                Debug.Log("Slicing v1 " + v1 + " is not on DA [" + face.d + "," + face.a + "]");
            }
            if (Vector3.Dot(face.c-face.b, v0-face.b) < 0.999f) {
                Debug.Log("Slicing v0 " + v0 + " is not on CB [" + face.c + "," + face.b + "]");
            }
            Face left = new Face(face.a, face.b, v0, v1);
            float relU = (v1 - face.a).magnitude/(face.d - face.a).magnitude;
            left.uvA = face.uvA;
            left.uvB = face.uvB;
            left.uvC = Vector2.Lerp(face.uvB, face.uvC, relU);
            left.uvD = Vector2.Lerp(face.uvA, face.uvD, relU);
            Face right = new Face(v1, v0, face.c, face.d);
            right.uvA = left.uvD;
            right.uvB = left.uvC;
            right.uvC = face.uvC;
            right.uvD = face.uvD;
            return new Face[] {left, right};
        }

        public static Face[] SliceFaceHorizontally(Face face, float relY) {
            if (relY < 0 || relY > 1) {
                Debug.Log("Slicing relY is invalid: " + relY);
            }
            Vector3 v0 = Vector3.Lerp(face.a, face.b, relY);
            Vector3 v1 = Vector3.Lerp(face.d, face.c, relY);
            Face bottom = new Face(face.a, v0, v1, face.d);
            float relU = (v0 - face.a).magnitude/(face.b - face.a).magnitude;
            bottom.uvA = face.uvA;
            bottom.uvB = Vector2.Lerp(face.uvA, face.uvB, relU);
            bottom.uvC = Vector2.Lerp(face.uvD, face.uvC, relU);
            bottom.uvD = face.uvD;
            Face top = new Face(v0, face.b, face.c, v1);
            top.uvA = bottom.uvB;
            top.uvB = face.uvB;
            top.uvC = face.uvC;
            top.uvD = bottom.uvC;
            return new Face[] {bottom, top};
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

        public static List<Face> CloseEdgeLoops(List<Vector3> fromLoop, List<Vector3> toLoop, float uvScale=1f) {
            List<Face> faces = new List<Face>();
            Vector3 prev0 = Vector3.zero;
            Vector3 prev1 = Vector3.zero;
            bool firstVertex = true;
            for (int i = 0; i < fromLoop.Count; i++) {
                Vector3 v0 = fromLoop[i];
                Vector3 v1 = toLoop[i];
                if (!firstVertex) {
                    Face face = new Face(prev0, v0, v1, prev1);
                    face.SetUVFront(Vector3.Distance(prev0, prev1) * uvScale, Vector3.Distance(prev0, v0) * uvScale);
                    faces.Add(face);
                }
                prev0 = v0;
                prev1 = v1;
                firstVertex = false;
            }
            return faces;
        }

        public static List<Face> ExtrudeEdges(Face face, Vector3 direction, float uvScale = 1f) {
            return ExtrudeEdges(new List<Vector3> {face.a, face.b, face.c, face.d, face.a}, direction, uvScale);
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