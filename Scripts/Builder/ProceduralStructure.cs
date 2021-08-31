using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

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

        public void RebuildCave(CaveDefinition cave, GameObject target) {
            Building building = new Building();
            BuildingObject cavemesh = new BuildingObject();
            cavemesh.material = cave.material;
            // construct the shape
            List<Vector3> shapeEdgeList = new List<Vector3>();
            switch (cave.crosscutShape) {
                case CaveDefinition.Shape.O_Shaped:
                shapeEdgeList = ConstructOShape(cave.baseWidth, cave.baseHeight);
                break;
                case CaveDefinition.Shape.Tunnel:
                default:
                shapeEdgeList = ConstructTunnelShape(cave.baseWidth, cave.baseHeight);
                break;
            }
            for(int i = 0; i < cave.shapeSmoothing; i++) {
                shapeEdgeList = SmoothVertices(shapeEdgeList);
            }
            List<Vector3> previousEdgeLoop = null;
            List<Face> previousAddedFaces = null;
            Vector3 previousPosition = Vector3.zero;
            Vector3 previousDirection = Vector3.zero;
            int idx = 0;
            float uOffset = 0;
            foreach (Tangent tangent in cave.GetTangents()) {
                Vector3 localPos = tangent.position - target.transform.position;
                Quaternion localRotation = Quaternion.LookRotation(tangent.direction, Vector3.up);
                List<Vector3> currentEdgeLoop = Builder.MoveVertices(Builder.RotateVertices(shapeEdgeList, localRotation), localPos);
                if (previousEdgeLoop != null) {
                    // only add it if there is no overlap
                    Vector3 right = Vector3.Cross(tangent.direction, Vector3.up);
                    Vector3 right0 = Vector3.Cross(previousDirection, Vector3.up);
                    float w = cave.baseWidth/2;
                    Vector3 m = previousPosition + right0 * w - right * w;
                    // there is a collision if the current location is nearer than m
                    Vector3 plane = localPos + (previousPosition - localPos) / 2;
                    Vector3 planeNormal = (previousDirection + tangent.direction) / 2;
                    if ((localPos - previousPosition).sqrMagnitude < (m - previousPosition).sqrMagnitude) {
                        //Debug.LogFormat("#{0}, m = {1}, localPos = {2}, previousPosition = {3}", idx, m, localPos, previousPosition);
                        // clamp generatedFaces to plane in normal direction and previous to opposite
                        //Debug.LogFormat("plane({0},{1}), current {2}, previous {3}", plane, planeNormal, currentEdgeLoop.Elements(), previousEdgeLoop.Elements());
                        cavemesh.ClampToPlane(currentEdgeLoop, previousEdgeLoop, plane, planeNormal);
                    }
                    List<Face> generatedFaces = Builder.BridgeEdgeLoopsPrepared(previousEdgeLoop, currentEdgeLoop, cave.uvScale);
                    //Builder.SetUVCylinderProjection(generatedFaces, plane + Vector3.up * cave.baseHeight/2, planeNormal, uOffset, cave.uvScale);
                    Vector3 cylinderCenter = (Builder.FindCentroid(currentEdgeLoop) + Builder.FindCentroid(previousEdgeLoop))/2;
                    Builder.SetUVCylinderProjection(generatedFaces, cylinderCenter, planeNormal, uOffset, cave.uvScale);
                    cavemesh.AddFaces(generatedFaces);
                    previousPosition = localPos;
                    previousDirection = tangent.direction;
                    previousAddedFaces = generatedFaces;
                    previousEdgeLoop = currentEdgeLoop;
                } else {
                    // this is the first crosscut
                    previousPosition = localPos;
                    previousDirection = tangent.direction;
                    if (cave.closeEnds) {
                        BuildingObject frontFace = new BuildingObject();
                        frontFace.AddFaces(Face.PolygonToTriangleFan(currentEdgeLoop));
                        frontFace.InvertNormals();
                        frontFace.SetUVBoxProjection(cave.uvScale);
                        cavemesh.AddObject(frontFace);
                    }
                    previousEdgeLoop = currentEdgeLoop;
                }
                uOffset += (previousPosition - localPos).magnitude;
                idx++;
            }
            // this is the last crosscut
            if (cave.closeEnds && previousEdgeLoop != null) {
                BuildingObject frontFace = new BuildingObject();
                frontFace.AddFaces(Face.PolygonToTriangleFan(previousEdgeLoop));
                frontFace.SetUVBoxProjection(cave.uvScale);
                cavemesh.AddObject(frontFace);
            }
            //cavemesh.SetUVBoxProjection(cave.uvScale);
            building.AddObject(cavemesh);
            building.Build(target, 0);
        }

        public List<Vector3> SmoothVertices(List<Vector3> l) {
            Vector3 center = Builder.FindCentroid(l);
            List<Vector3> newList = new List<Vector3>(l.Count*2);
            for (int i = 0; i < l.Count; i++) {
                newList.Add(l[i]);
                int j = i < l.Count-1 ? i+1 : 0;
                Vector3 interpolated = (l[i] + l[j])/2f;
                float smoothedDist = ((center - l[i]).magnitude + (center - l[j]).magnitude)/2;
                float dist = (interpolated - center).magnitude;
                newList.Add(interpolated / dist * smoothedDist);
            }
            return newList;
        }

        private List<Vector3> ConstructTunnelShape(float width, float height) {
            Vector3 a = new Vector3(0, 0, 0);
            Vector3 b = new Vector3(-0.4f * width, 0.06f * height, 0);
            Vector3 c = new Vector3(-0.5f * width, 0.12f * height, 0);
            Vector3 d = new Vector3(-0.5f * width, 0.58f * height, 0);
            Vector3 e = new Vector3(-0.36f * width, 0.84f * height, 0);
            Vector3 f = new Vector3(-0.18f * width, 0.97f * height, 0);
            Vector3 g = new Vector3(0, height, 0);
            Vector3 h = new Vector3(-f.x, f.y, 0);
            Vector3 i = new Vector3(-e.x, e.y, 0);
            Vector3 j = new Vector3(-d.x, d.y, 0);
            Vector3 k = new Vector3(-c.x, c.y, 0);
            Vector3 l = new Vector3(-b.x, b.y, 0);
            return new List<Vector3> { a, b, c, d, e, f, g, h, i, j, k, l };
        }

        private List<Vector3> ConstructOShape(float width, float height) {
            List<Vector3> result = new List<Vector3>(12);
            for (int i = 0; i < 12; i++) {
                result.Add(new Vector3(width/2 * Mathf.Sin(Mathf.Deg2Rad * i*30), height/2 + height/2 * Mathf.Cos(Mathf.Deg2Rad * i*30), 0));
            }
            return result;
        }
    }
}