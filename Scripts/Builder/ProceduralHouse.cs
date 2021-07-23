using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class ProceduralHouse {
        public void RebuildHouseWithInterior(HouseDefinition house, GameObject target) {
            RebuildHouseWithInterior(house, target, 0);
            RebuildHouseWithInterior(house, target, 1);
            LODGroup lodGroup = target.GetComponent<LODGroup>();
            if (lodGroup == null) {
                lodGroup = target.AddComponent<LODGroup>();
            }
            GameObject go0 = Building.GetChildByName(target, "LOD0");
            LOD lod0 = new LOD(0.5f, go0.GetComponentsInChildren<MeshRenderer>());
            GameObject go1 = Building.GetChildByName(target, "LOD1");
            LOD lod1 = new LOD(0.1f, go1.GetComponentsInChildren<MeshRenderer>());
            lodGroup.SetLODs(new LOD[] { lod0, lod1 });
            lodGroup.RecalculateBounds();
            MeshCollider[] colliders = go1.GetComponentsInChildren<MeshCollider>();
            foreach (MeshCollider collider in colliders) {
                collider.enabled = false;
            }
        }

        // the lod integer should be 0 to build the complete house and with higher levels we add less details
        // currently one 0 and 1 is used where with lod=1 we skip the interior objects including the inner walls sides
        public void RebuildHouseWithInterior(HouseDefinition house, GameObject target, int lod) {
            // this is the position of the resulting gameobject
            Vector3 center = new Vector3(0, house.heightOffset, 0);
            // width is the distance from the leftmost to the rightmost wall corners seen from the front i.e. on the x axis
            float width = house.width;
            // length is the distance from front to back wall i.e. on the z axis
            float length = house.length;
            Building building = new Building();
            bool lastLayerIsHollow = false;
            // this is used to close the top of the walls on the last layer when we build the roof
            float lastWallThickness = 0;

            // each layer describes a floor although multiple layers could be combined to a floor if you disable floor/ceiling creation
            foreach (HouseDefinition.BuildingStructure bs in house.layers) {
                float height = bs.height;
                float wallThickness = bs.wallThickness;
                lastWallThickness = wallThickness;
                BuildingObject layer = new BuildingObject();
                Vector3 a = center + new Vector3(-width/2, 0, -length/2);
                Vector3 b = center + new Vector3(-width/2, 0, length/2);
                Vector3 c = center + new Vector3(width/2, 0, length/2);
                Vector3 d = center + new Vector3(width/2, 0, -length/2);
                Vector3 a1 = a + new Vector3(bs.slopeX*height, height, bs.slopeZ*height);
                Vector3 b1 = b + new Vector3(bs.slopeX*height, height, -bs.slopeZ*height);
                Vector3 c1 = c + new Vector3(-bs.slopeX*height, height, -bs.slopeZ*height);
                Vector3 d1 = d + new Vector3(-bs.slopeX*height, height, bs.slopeZ*height);

                if (bs.hollow && lod==0) {
                    lastLayerIsHollow = true;
                    Vector3 ai = a + new Vector3(wallThickness, 0, wallThickness);
                    Vector3 bi = b + new Vector3(wallThickness, 0, -wallThickness);
                    Vector3 ci = c + new Vector3(-wallThickness, 0, -wallThickness);
                    Vector3 di = d + new Vector3(-wallThickness, 0, wallThickness);
                    // outer faces of walls
                    layer.ExtrudeEdges(new List<Vector3> {a, d, c, b, a}, Vector3.up * height, bs.uvScale);
                    // inner faces
                    layer.ExtrudeEdges(new List<Vector3> {ai, bi, ci, di, ai}, Vector3.up * height, bs.uvScale);
                    if (bs.addFloor) {
                        Face floor = new Face(ai, bi, ci, di);
                        floor.SetUVFront(width * bs.uvScale, height * bs.uvScale);
                        layer.AddFace(floor);
                    }
                    if (bs.addCeiling) {
                        Face ceiling = new Face(ai, bi, ci, di);
                        ceiling.MoveFaceBy(Vector3.up * (height - wallThickness)).InvertNormals();
                        ceiling.SetUVFront(width * bs.uvScale, height * bs.uvScale);
                        layer.AddFace(ceiling);
                    }
                    foreach (HouseDefinition.WallCutout co in bs.cutouts) {
                        Vector3 origin = center + new Vector3(co.dimension.x, co.dimension.y + co.dimension.height/2, 0);
                        Vector3 direction = Vector3.back;
                        switch(co.side) {
                            case HouseDefinition.Side.Right: direction = Vector3.right; break;
                            case HouseDefinition.Side.Left: direction = Vector3.left; break;
                            case HouseDefinition.Side.Back: direction = Vector3.forward; break;
                        }
                        layer.MakeHole(origin, direction, Vector3.up, co.dimension.width, co.dimension.height);
                    }
                } else {
                    lastLayerIsHollow = false;
                    layer.ExtrudeEdges(new List<Vector3> {a, d, c, b, a}, Vector3.up * height, bs.uvScale);
                    foreach (HouseDefinition.WallCutout co in bs.cutouts) {
                        //layer.CutFront(co.dimension, bs.uvScale);
                        Vector3 origin = center + new Vector3(co.dimension.x, co.dimension.y + co.dimension.height/2, 0);
                        Vector3 direction = Vector3.back;
                        switch(co.side) {
                            case HouseDefinition.Side.Right: direction = Vector3.right; break;
                            case HouseDefinition.Side.Left: direction = Vector3.left; break;
                            case HouseDefinition.Side.Back: direction = Vector3.forward; break;
                        }
                        layer.MakeHole(origin, direction, Vector3.up, co.dimension.width, co.dimension.height);
                        Face opening = layer.FindFirstFaceByTag(Builder.CUTOUT);
                        if (opening != null) {
                            layer.IndentFace(opening, Vector3.forward * 0.1f, co.uvScale);
                            opening.material = co.material;
                            opening.SetUVForSize(co.uvScale);
                            opening.UnTag(Builder.CUTOUT);
                        } else {
                            Debug.Log("no opening found for " + co.name);
                        }
                    }
                }
                // add stairs
                if (bs.stairs != null) {
                    foreach (HouseDefinition.Stairs stairs in bs.stairs) {
                        // skip inside stairs for LOD >0
                        if (lod > 0 && stairs.inside) {
                            continue;
                        }
                        Vector3 stairsPosition = center;
                        Quaternion stairsRotation = Quaternion.identity;
                        switch (stairs.side)
                        {
                            case HouseDefinition.Side.Front:
                                stairsPosition = center - new Vector3(0, 0, -length/2);
                                break;
                            case HouseDefinition.Side.Back:
                                stairsPosition = center - new Vector3(0, height, -length/2);
                                stairsRotation = Quaternion.AngleAxis(180, Vector3.up);
                                break;
                            case HouseDefinition.Side.Right:
                                stairsPosition = center - new Vector3(0, height, -width/2);
                                stairsRotation = Quaternion.AngleAxis(-90, Vector3.up);
                                break;
                            case HouseDefinition.Side.Left:
                                stairsPosition = center - new Vector3(0, height, -width/2);
                                stairsRotation = Quaternion.AngleAxis(90, Vector3.up);
                                break;
                        }
                        Face floor = Face.CreateXZPlane(stairs.baseWidth,stairs.baseLength);
                        floor.SetUVFront(stairs.baseWidth * stairs.uvScale, stairs.baseLength * stairs.uvScale);
                        BuildingObject stairsBlock = new BuildingObject();
                        stairsBlock.material = stairs.material;
                        stairsBlock.rotation = stairsRotation;
                        stairsBlock.AddFace(floor);
                        Vector3 dn = Vector3.down * stairs.stepHeight;
                        Vector3 ou = Vector3.back * Mathf.Cos(stairs.descentAngle*Mathf.Deg2Rad) * stairs.stepDepth;
                        Vector3 si = Vector3.right * Mathf.Sin(stairs.descentAngle*Mathf.Deg2Rad) * stairs.stepDepth;
                        float currentHeight = 0;
                        Vector3 stepC = floor.c;
                        Vector3 stepD = floor.d;
                        Vector3 stepA = floor.a;
                        Vector3 stepB = floor.b;
                        //Debug.Log("stairs ou=" + ou + ", si=" + si);
                        while (currentHeight < stairs.totalHeight) {
                            // extrude down
                            stairsBlock.ExtrudeEdges(new List<Vector3> {stepC, stepD, stepA, stepB}, dn, stairs.uvScale);
                            stepC += dn;
                            stepD += dn;
                            stepA += dn;
                            stepB += dn;
                            currentHeight += stairs.stepHeight;
                            if (currentHeight >= stairs.totalHeight) break;
                            // extrude step
                            if (ou.sqrMagnitude > 0) {
                                stairsBlock.ExtrudeEdges(new List<Vector3> {stepD, stepA}, ou, stairs.uvScale);
                                stepD += ou;
                                stepA += ou;
                            }
                            if (si.sqrMagnitude > 0) {
                                if (stairs.descentAngle > 0) {
                                    stairsBlock.ExtrudeEdges(new List<Vector3> {stepC, stepD}, si, stairs.uvScale);
                                    stepC += si;
                                    stepD += si;
                                } else {
                                    stairsBlock.ExtrudeEdges(new List<Vector3> {stepA, stepB}, si, stairs.uvScale);
                                    stepA += si;
                                    stepB += si;
                                }
                            }
                        }

                        stairsBlock.SetUVBoxProjection(stairs.uvScale);

                        float zOffset = length/2;
                        if (stairs.side == HouseDefinition.Side.Left || stairs.side == HouseDefinition.Side.Right) {
                            zOffset = width/2;
                        }
                        if (stairs.inside) {
                            zOffset -= stairs.baseLength + wallThickness;
                            stairsBlock.RotateFaces(Quaternion.AngleAxis(180, Vector3.up));
                        }
                        stairsBlock.position = center + new Vector3(stairs.offset, stairs.baseHeight, -stairs.baseLength/2 - zOffset);
                        if (stairs.inside) {
                            //Vector3 holePosition = stairsBlock.rotation * (stairsBlock.position - Vector3.up);
                            Bounds stairsBounds = stairsBlock.CalculateGlobalBounds();
                            Vector3 holePosition = stairsBounds.center;
                            layer.MakeHole(holePosition, Vector3.up, Vector3.back, stairsBounds.extents.x*2, stairsBounds.extents.z*2);
                            Face hole = layer.FindFirstFaceByTag(Builder.CUTOUT);
                            if (hole != null) {
                                layer.RemoveFace(hole);
                            }
                        }
                        //Debug.Log("stairs position: " + stairsBlock.position);
                        building.AddObject(stairsBlock);
                    }
                }
                layer.material = bs.material;
                building.AddObject(layer);
                center.y += height;
            }

            if (house.roofHeight > 0) {
                BuildingObject roofLayer = new BuildingObject();
                roofLayer.material = house.materialGable;
                Vector3 a = center + new Vector3(-width/2, 0, -length/2);
                Vector3 b = center + new Vector3(-width/2, 0, length/2);
                Vector3 c = center + new Vector3(width/2, 0, length/2);
                Vector3 d = center + new Vector3(width/2, 0, -length/2);

                float uvScale = house.uvScaleGable;
                Vector3 e1 = center + new Vector3(0, house.roofHeight, -length/2);
                Vector3 e2 = center + new Vector3(0, house.roofHeight, length/2);
                Face frontFace = new Face(a, e1, d);
                frontFace.SetUVFront(width * uvScale, house.roofHeight * uvScale);
                roofLayer.AddFace(frontFace);
                Face backface = new Face(c, e2, b);
                backface.SetUVFront(width * uvScale, house.roofHeight * uvScale);
                roofLayer.AddFace(backface);
                
                if (lastLayerIsHollow) {
                    Face innerFrontFace = frontFace.DeepCopy();
                    innerFrontFace.MoveFaceBy(Vector3.forward * lastWallThickness).InvertNormals();
                    Face innerBackFace = backface.DeepCopy().MoveFaceBy(Vector3.back * lastWallThickness).InvertNormals();
                    roofLayer.AddFace(innerFrontFace);
                    roofLayer.AddFace(innerBackFace);
                    Face innerLeftRoof = new Face(b, a, e1, e2);
                    float h = Mathf.Sqrt(width/2 * width/2 + house.roofHeight * house.roofHeight);
                    innerLeftRoof.SetUVFront(length * uvScale, h * uvScale);
                    Face innerRightRoof = new Face(d, c, e2, e1);
                    innerRightRoof.SetUVFront(length * uvScale, h * uvScale);
                    roofLayer.AddFace(innerLeftRoof);
                    roofLayer.AddFace(innerRightRoof);
                    Face innerLeftTopWall = new Face(a,b, b+Vector3.right * lastWallThickness, a+Vector3.right * lastWallThickness);
                    innerLeftTopWall.SetUVFront(lastWallThickness, length);
                    Face innerRightTopWall = new Face(d+Vector3.left * lastWallThickness, c+Vector3.left * lastWallThickness, c, d);
                    innerRightTopWall.SetUVFront(lastWallThickness, length);
                    roofLayer.AddFace(innerLeftTopWall);
                    roofLayer.AddFace(innerRightTopWall);
                }

                Vector3 extZ = new Vector3(0, 0, house.roofExtendZ);
                if (house.roofExtendZ > 0) {
                    Face rightBackExtend = new Face(e2, c, c + extZ, e2 + extZ);
                    rightBackExtend.SetUVFront(house.roofExtendZ * uvScale, width/2 * uvScale);
                    roofLayer.AddFace(rightBackExtend);
                    Face rightFrontExtend = new Face(e1 - extZ, d - extZ, d, e1);
                    rightFrontExtend.SetUVFront(house.roofExtendZ * uvScale, width/2 * uvScale);
                    roofLayer.AddFace(rightFrontExtend);
                    Face leftBackExtend = new Face(b, e2, e2 + extZ, b + extZ);
                    leftBackExtend.SetUVFront(house.roofExtendZ * uvScale, width/2 * uvScale);
                    roofLayer.AddFace(leftBackExtend);
                    Face leftFrontExtend = new Face(a - extZ, e1 - extZ, e1, a);
                    leftFrontExtend.SetUVFront(house.roofExtendZ * uvScale, width/2 * uvScale);
                    roofLayer.AddFace(leftFrontExtend);
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
                    roofLayer.AddFace(rightExtend);
                    dr = dr + extX;
                    cr = cr + extX;
                    extX = new Vector3(-house.roofExtendX, house.roofExtendX * m, 0);
                    Face leftExtend = new Face(br, br+extX, ar+extX, ar);
                    leftExtend.SetUVFront(length * uvScale, house.roofExtendX * uvScale);
                    roofLayer.AddFace(leftExtend);
                    br = br + extX;
                    ar = ar + extX;
                }

                Vector3 roofThickness = new Vector3(0, house.roofThickness, 0);
                List<Vector3> roofEdges = new List<Vector3> {ar, e1, dr, cr, e2, br, ar};
                roofLayer.ExtrudeEdges(roofEdges, roofThickness, uvScale);

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
                leftRoof.material = house.materialRoof;
                building.AddFace(leftRoof);
                Face rightRoof = new Face(dr, e1, e2, cr);
                rightRoof.SetUVFront((length + 2 * house.roofExtendZ) * uvScale, halfSlope * uvScale);
                rightRoof.material = house.materialRoof;
                building.AddFace(rightRoof);
                building.AddObject(roofLayer);
            }

            string name = "LOD" + lod;
            GameObject lodMeshes = Building.GetChildByName(target, name);
            if (lodMeshes == null) {
                lodMeshes = new GameObject(name);
                lodMeshes.transform.parent = target.transform;
                lodMeshes.transform.position = target.transform.position;
                lodMeshes.transform.rotation = target.transform.rotation;
            }
            building.Build(lodMeshes);
        }
    }
}