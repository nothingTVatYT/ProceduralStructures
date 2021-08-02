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
    }
}