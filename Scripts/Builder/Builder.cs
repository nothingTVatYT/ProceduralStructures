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
                    face1.material = face.material;
                    faces.Add(face1);
                }
                prev = v;
                firstVertex = false;
            }
            faces.Add(face.MoveFaceBy(direction));
            return faces;
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
            f1.material = face.material;
            f2.material = face.material;
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
            f1.material = face.material;
            f2.material = face.material;
            return new Face[] { f1, f2 };
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

        public static List<Face> BridgeEdgeLoops(List<Vector3> fromVertices, List<Vector3> toVertices, float uvScale=1f) {
            List<Face> faces = new List<Face>();
            //TemporaryTesting();
            int numberOfVertices = fromVertices.Count;
            if (numberOfVertices < 3) {
                Debug.LogWarning("There are not enough vertices to bridge: " + numberOfVertices);
                return faces;
            }
            if (numberOfVertices != toVertices.Count) {
                Debug.LogWarning("The vertices counts do not match: from=" + numberOfVertices + ", to=" + toVertices.Count);
                return faces;
            }
            CircularReadonlyList<Vector3> fromRing = new CircularReadonlyList<Vector3>(fromVertices);
            CircularReadonlyList<Vector3> toRing = new CircularReadonlyList<Vector3>(toVertices);
            Vector3 fromNormal = Vector3.Cross(fromRing[1]-fromRing[0], fromRing[-1]-fromRing[0]).normalized;
            Vector3 toNormal = Vector3.Cross(toRing[1]-toRing[0], toRing[-1]-toRing[0]).normalized;
            // the normals should point to opposit directions
            float dot = Vector3.Dot(fromNormal, toNormal);
            if (dot > 0) {
                toRing.Reverse();
            }
            toRing.Reverse();
            // now check which vertices we should use for bridging
            float minDeviation = float.MaxValue;
            int bestPairing = 0;
            for (int o = 0; o < fromRing.Count; o++) {
                toRing.indexOffset = o;
                List<Vector3> directions = new List<Vector3>();
                for (int i = 0; i < fromRing.Count; i++) {
                    directions.Add((toRing[i]-fromRing[i]).normalized);
                }
                // sum the deviations from the expected value of one to each other
                float sumDeviations = 0;
                for (int i = 1; i < directions.Count; i++) {
                    float dotDir = Vector3.Dot(directions[i-1], directions[i]);
                    sumDeviations += 1f - dotDir;
                }
                if (sumDeviations < minDeviation) {
                    minDeviation = sumDeviations;
                    bestPairing = toRing.indexOffset;
                }
            }
            toRing.indexOffset = bestPairing;

            for (int i = 0; i < fromRing.Count; i++) {
                Face face = new Face(fromRing[i], fromRing[i+1], toRing[i+1], toRing[i]);
                face.SetUVForSize(uvScale);
                faces.Add(face);
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