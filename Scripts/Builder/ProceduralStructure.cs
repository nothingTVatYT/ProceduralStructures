using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class ProceduralStructure {
        public void RebuildLadder(LadderDefinition ladder, GameObject target) {
            Building building = new Building();

            // these are the center points of the poles
            Vector3 originLeft = Vector3.left * (ladder.stepWidth + ladder.poleThickness)/2;
            Vector3 originRight = Vector3.right * (ladder.stepWidth + ladder.poleThickness)/2;

            // first two faces are the bottom faces of the poles
            Face leftBottom = Face.CreateXZPlane(ladder.poleThickness, ladder.poleThickness).MoveFaceBy(originLeft).InvertNormals();
            Face rightBottom = Face.CreateXZPlane(ladder.poleThickness, ladder.poleThickness).MoveFaceBy(originRight).InvertNormals();
            BuildingObject leftPole = new BuildingObject();
            leftPole.material = ladder.ladderMaterial;
            BuildingObject rightPole = new BuildingObject();
            rightPole.material = ladder.ladderMaterial;
            leftPole.AddFace(leftBottom);
            float poleHeight = ladder.stepHeight * (ladder.steps+1);
            leftPole.AddFaces(Builder.ExtrudeEdges(leftBottom, Vector3.up * poleHeight, ladder.uvScale));
            rightPole.AddFace(rightBottom);
            rightPole.AddFaces(Builder.ExtrudeEdges(rightBottom, Vector3.up * poleHeight, ladder.uvScale));
            leftPole.AddFace(leftBottom.DeepCopy().MoveFaceBy(Vector3.up * poleHeight).InvertNormals());
            rightPole.AddFace(rightBottom.DeepCopy().MoveFaceBy(Vector3.up * poleHeight).InvertNormals());
            leftPole.RotateUVs();
            rightPole.RotateUVs();

            BuildingObject steps = new BuildingObject();
            for (int i = 0; i < ladder.steps; i++) {
                originLeft.y += ladder.stepHeight;
                originRight.y += ladder.stepHeight;
                leftPole.MakeHole(originLeft, Vector3.right, Vector3.up, ladder.stepThickness, ladder.stepThickness);
                rightPole.MakeHole(originRight, Vector3.left, Vector3.up, ladder.stepThickness, ladder.stepThickness);
                Face leftHoleFace = leftPole.FindFirstFaceByTag(Builder.CUTOUT);
                Face rightHoleFace = rightPole.FindFirstFaceByTag(Builder.CUTOUT);
                if (leftHoleFace != null && rightHoleFace != null) {
                    leftPole.RemoveFace(leftHoleFace);
                    rightPole.RemoveFace(rightHoleFace);
                    steps.AddFaces(Builder.BridgeEdgeLoops(rightHoleFace.GetVerticesList(), leftHoleFace.GetVerticesList(), ladder.uvScale));
                }
            }
            steps.material = ladder.ladderMaterial;
            building.AddObject(steps);

            building.AddObject(leftPole);
            building.AddObject(rightPole);

            building.Build(target, 0);
        }

        public void RebuildWall(WallDefinition wall, GameObject target) {
            int minNumberMarkers = wall.closeLoop ? 3 : 2;
            if (wall.points.Count < minNumberMarkers) {
                Debug.LogWarning("cannot build a wall with " + wall.points.Count + " corner(s).");
                return;
            }
            List<Vector3> points = new List<Vector3>(wall.points.Count);
            List<Vector3> originalPoints = new List<Vector3>(wall.points.Count);
            float maxY = float.MinValue;
            foreach (Transform t in wall.points) {
                Vector3 p = t.position;
                maxY = Mathf.Max(maxY, p.y);
            }
            foreach (Transform t in wall.points) {
                Vector3 p = t.position;
                originalPoints.Add(t.position);
                p.y = maxY;
                points.Add(p);
            }
            Vector3 centroid = Builder.FindCentroid(points);
            if (wall.sortMarkers) {
                wall.points.Sort(delegate(Transform a, Transform b) {
                    float angleA = Mathf.Atan2(a.position.z - centroid.z, a.position.x - centroid.x);
                    float angleB = Mathf.Atan2(b.position.z - centroid.z, b.position.x - centroid.x);
                    return angleA < angleB ? 1 : angleA > angleB ? -1 : 0;
                });
                points.Clear();
                originalPoints.Clear();
                foreach (Transform t in wall.points) {
                    Vector3 p = t.position;
                    originalPoints.Add(t.position);
                    p.y = maxY;
                    points.Add(p);
                }
            }
            for (int i = 0; i < points.Count; i++) {
                points[i] = points[i] - centroid;
                originalPoints[i] = originalPoints[i] - centroid;
            }
            Building building = new Building();
            BuildingObject wallObject = new BuildingObject();
            wallObject.material = wall.material;

            Vector3 wallHeight = new Vector3(0, wall.wallHeight, 0);
            Vector3 heightOffset = new Vector3(0, wall.heightOffset, 0);

            if (wall.closeLoop) {
                points.Add(points[0]);
            }
            List<Face> outerWall = Builder.ExtrudeEdges(points, wallHeight, wall.uvScale);
            List<Face> innerWall = Builder.CloneAndMoveFacesOnNormal(outerWall, wall.wallThickness, wall.uvScale);
            // project it back on the ground, this should be done by above function
            for (int i = 0; i < points.Count; i++) {
                Builder.MoveVertices(outerWall, points[i], Builder.MatchingVertex.XZ, originalPoints[i] + heightOffset);
            }
            for (int i = 0; i < outerWall.Count; i++) {
                Face outer = outerWall[i];
                Face inner = innerWall[i];
                // copy y values
                inner.d.y = outer.a.y;
                inner.a.y = outer.d.y;
                inner.b.y = outer.c.y;
                inner.c.y = outer.b.y;
            }
            wallObject.AddFaces(outerWall);
            wallObject.AddFaces(innerWall);
            // now close the top by extracting all BC edges and create faces
            List<Edge> innerEdges = new List<Edge>();
            List<Edge> outerEdges = new List<Edge>();
            foreach (Face face in innerWall) {
                innerEdges.Add(new Edge(face.b, face.c));
            }
            foreach (Face face in outerWall) {
                outerEdges.Add(new Edge(face.b, face.c));
            }
            List<Face> top = Builder.BridgeEdges(innerEdges, outerEdges, true, wall.uvScale);
            wallObject.AddFaces(top);

            if (!wall.closeLoop) {
                Face start = new Face(new Edge(innerWall[0].d, innerWall[0].c), new Edge(outerWall[0].b, outerWall[0].a));
                start.SetUVForSize(wall.uvScale);
                wallObject.AddFace(start);
                int i = outerWall.Count-1;
                Face end = new Face(new Edge(outerWall[i].d, outerWall[i].c), new Edge(innerWall[i].b, innerWall[i].a));
                end.SetUVForSize(wall.uvScale);
                wallObject.AddFace(end);
            }

            if (wall.generateCornerPieces) {
                for (int i = 0; i < originalPoints.Count; i++) {
                    Vector3 corner = originalPoints[i];
                    int j = i % outerWall.Count;
                    Vector3 direction = (innerWall[j].a - outerWall[j].a).normalized;
                    wallObject.AddObject(GenerateCornerPiece(corner + heightOffset, wallHeight.y+1, wall.wallThickness*2, direction, wall.uvScale));
                }
            }

            building.AddObject(wallObject);
            building.Build(target, 0);
            GameObject go = Building.GetChildByName(target, "LOD0");
            if (go != null) {
                go.transform.position = centroid;
            }

            if (wall.cornerPiece != null) {
                
                foreach (Transform t in wall.points) {
                    GameObject corner = GameObject.Instantiate(wall.cornerPiece);
                    corner.transform.parent = go.transform;
                    corner.transform.position = t.position + (centroid-t.position).normalized * wall.wallThickness/2;
                    corner.transform.LookAt(new Vector3(centroid.x, corner.transform.position.y, centroid.z));
                }
            }
        }

        private BuildingObject GenerateCornerPiece(Vector3 center, float height, float width,
                Vector3 inwards, float uvScale) {
            BuildingObject result = new BuildingObject();                    
            Face top = Face.CreateXZPlane(width, width);
            top.MoveFaceBy(Vector3.up * height).Rotate(Quaternion.LookRotation(inwards, Vector3.up));
            result.AddFaces(Builder.ExtrudeEdges(top, Vector3.down * height, uvScale));
            result.AddFace(top);
            result.position = center;
            return result;
        }
    }
}