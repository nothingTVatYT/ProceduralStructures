using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class ProceduralHouse {
        public void RebuildHouse(HouseDefinition house, GameObject target) {
            Building building = new Building();

            Vector3 center = new Vector3(0, house.heightOffset, 0);
            float width = house.width;
            float length = house.length;
            foreach (HouseDefinition.BuildingStructure bs in house.layers) {
                float height = bs.height;
                List<Face> frontFaces = new List<Face>();
                List<Face> backFaces = new List<Face>();
                List<Face> leftFaces = new List<Face>();
                List<Face> rightFaces = new List<Face>();
                Vector3 a = center + new Vector3(-width/2, 0, -length/2);
                Vector3 b = center + new Vector3(-width/2, 0, length/2);
                Vector3 c = center + new Vector3(width/2, 0, length/2);
                Vector3 d = center + new Vector3(width/2, 0, -length/2);
                Vector3 a1 = a + new Vector3(bs.slopeX*height, height, bs.slopeZ*height);
                Vector3 b1 = b + new Vector3(bs.slopeX*height, height, -bs.slopeZ*height);
                Vector3 c1 = c + new Vector3(-bs.slopeX*height, height, -bs.slopeZ*height);
                Vector3 d1 = d + new Vector3(-bs.slopeX*height, height, bs.slopeZ*height);
                Face frontface = new Face(a, a1, d1, d);
                frontface.SetUVFront(width * bs.uvScale, height * bs.uvScale);
                frontFaces.Add(frontface);
                Face rightFace = new Face(d, d1, c1, c);
                rightFace.SetUVFront(length * bs.uvScale, height * bs.uvScale);
                rightFaces.Add(rightFace);
                BuildingObject rightWall = new BuildingObject();
                rightWall.faces.Add(rightFace);
                rightWall.position = d;
                Face backFace = new Face(c, c1, b1, b);
                backFace.SetUVFront(width * bs.uvScale, height * bs.uvScale);
                backFaces.Add(backFace);
                Face leftFace = new Face(b, b1, a1, a);
                leftFace.SetUVFront(length * bs.uvScale, height * bs.uvScale);
                leftFaces.Add(leftFace);
                width -= 2*bs.slopeX*height;
                length -= 2*bs.slopeZ*height;

                // doors and windows are defined with x,y,width and height relative to the lower left
                // corner of the face
                Vector3 cutoutOrigin = a;
                // handle doors/windows and alike
                foreach (HouseDefinition.WallCutout co in bs.cutouts) {
                    List<Face> side = frontFaces; // assign something to make the compiler happy
                    switch (co.side) {
                        case HouseDefinition.Side.Front: side = frontFaces; break;
                        case HouseDefinition.Side.Back: side = backFaces; cutoutOrigin = c; break;
                        case HouseDefinition.Side.Right: side = rightFaces; cutoutOrigin = d; break;
                        case HouseDefinition.Side.Left: side = leftFaces; cutoutOrigin = b; break;
                    }
                    List<Face> sliced = Builder.Cutout(side, co.dimension, cutoutOrigin, bs.uvScale);
                    if (co.material != bs.material) {
                        Face doorFace = Builder.FindFirstFaceByTag(sliced, Builder.CUTOUT);
                        if (doorFace != null) {
                            doorFace.SetUVFront(co.dimension.width * co.uvScale, co.dimension.height * co.uvScale);
                            building.AddFace(doorFace, co.material);
                            sliced.Remove(doorFace);
                        }
                    }
                    side.Clear();
                    side.AddRange(sliced);
                }

                building.AddFaces(frontFaces, bs.material);
                building.AddFaces(backFaces, bs.material);
                building.AddFaces(rightFaces, bs.material);
                building.AddFaces(leftFaces, bs.material);

                center += new Vector3(0, height, 0);
            }
            if (house.roofHeight > 0) {
                Vector3 a = center + new Vector3(-width/2, 0, -length/2);
                Vector3 b = center + new Vector3(-width/2, 0, length/2);
                Vector3 c = center + new Vector3(width/2, 0, length/2);
                Vector3 d = center + new Vector3(width/2, 0, -length/2);

                float uvScale = house.uvScaleGable;
                Vector3 e1 = center + new Vector3(0, house.roofHeight, -length/2);
                Vector3 e2 = center + new Vector3(0, house.roofHeight, length/2);
                Face frontFace = new Face(a, e1, d);
                frontFace.SetUVFront(width * uvScale, house.roofHeight * uvScale);
                building.AddFace(frontFace, house.materialGable);
                Face backface = new Face(c, e2, b);
                backface.SetUVFront(width * uvScale, house.roofHeight * uvScale);
                building.AddFace(backface, house.materialGable);
                Vector3 extZ = new Vector3(0, 0, house.roofExtendZ);
                if (house.roofExtendZ > 0) {
                    Face rightBackExtend = new Face(e2, c, c + extZ, e2 + extZ);
                    rightBackExtend.SetUVFront(house.roofExtendZ * uvScale, width/2 * uvScale);
                    building.AddFace(rightBackExtend, house.materialGable);
                    Face rightFrontExtend = new Face(e1 - extZ, d - extZ, d, e1);
                    rightFrontExtend.SetUVFront(house.roofExtendZ * uvScale, width/2 * uvScale);
                    building.AddFace(rightFrontExtend, house.materialGable);
                    Face leftBackExtend = new Face(b, e2, e2 + extZ, b + extZ);
                    leftBackExtend.SetUVFront(house.roofExtendZ * uvScale, width/2 * uvScale);
                    building.AddFace(leftBackExtend, house.materialGable);
                    Face leftFrontExtend = new Face(a - extZ, e1 - extZ, e1, a);
                    leftFrontExtend.SetUVFront(house.roofExtendZ * uvScale, width/2 * uvScale);
                    building.AddFace(leftFrontExtend, house.materialGable);
                    e2 = e2 + extZ;
                    e1 = e1 - extZ;
                }
                Vector3 ar = a - extZ;
                Vector3 br = b + extZ;
                Vector3 cr = c + extZ;
                Vector3 dr = d - extZ;
                if (house.roofExtendX > 0) {
                    float m = -house.roofHeight / (width/2);
                    Vector3 extX = new Vector3(house.roofExtendX, house.roofExtendX * m, 0);
                    Face rightExtend = new Face(dr, dr+extX, cr+extX, cr);
                    rightExtend.SetUVFront(length * uvScale, house.roofExtendX * uvScale);
                    building.AddFace(rightExtend, house.materialGable);
                    dr = dr + extX;
                    cr = cr + extX;
                    extX = new Vector3(-house.roofExtendX, house.roofExtendX * m, 0);
                    Face leftExtend = new Face(br, br+extX, ar+extX, ar);
                    leftExtend.SetUVFront(length * uvScale, house.roofExtendX * uvScale);
                    building.AddFace(leftExtend, house.materialGable);
                    br = br + extX;
                    ar = ar + extX;
                }

                Vector3 roofThickness = new Vector3(0, house.roofThickness, 0);
                List<Vector3> roofEdges = new List<Vector3> {ar, e1, dr, cr, e2, br, ar};
                building.AddFaces(Builder.ExtrudeEdges(roofEdges, roofThickness, uvScale), house.materialGable);

                ar = ar + roofThickness;
                br = br + roofThickness;
                cr = cr + roofThickness;
                dr = dr + roofThickness;
                e1 = e1 + roofThickness;
                e2 = e2 + roofThickness;

                uvScale = house.uvScaleRoof;
                Face leftRoof = new Face(br, e2, e1, ar);
                float halfSlope = Mathf.Sqrt(width/2 * width/2 + house.roofHeight * house.roofHeight);
                leftRoof.SetUVFront((length + 2 * house.roofExtendZ) * uvScale, halfSlope * uvScale);
                building.AddFace(leftRoof, house.materialRoof);
                Face rightRoof = new Face(dr, e1, e2, cr);
                rightRoof.SetUVFront((length + 2 * house.roofExtendZ) * uvScale, halfSlope * uvScale);
                building.AddFace(rightRoof, house.materialRoof);
            }

            building.Build(target);
        }

    }
}