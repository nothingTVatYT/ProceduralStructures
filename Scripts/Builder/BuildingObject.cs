using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class BuildingObject {
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public List<Face> faces = new List<Face>();

        public BuildingObject ResetTransform() {
            foreach (Face face in faces) {
                face.Rotate(Quaternion.Inverse(rotation)).MoveFaceBy(-position);
            }
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return this;
        }

        public BuildingObject ApplyTransform() {
            foreach (Face face in faces) {
                face.MoveFaceBy(position).Rotate(rotation);
            }
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return this;
        }

        public BuildingObject AddFace(Face face) {
            faces.Add(face);
            return this;
        }

        public BuildingObject AddFaces(List<Face> newFaces) {
            faces.AddRange(newFaces);
            return this;
        }

        public BuildingObject RemoveFace(Face face) {
            if (!faces.Remove(face))
                Debug.LogWarning("could not remove " + face);
            return this;
        }

        public Face LocalToWorld(Face face) {
            face.Rotate(Quaternion.Inverse(rotation)).MoveFaceBy(-position);
            return face;
        }

        public BuildingObject TranslateFaces(Vector3 translation) {
            foreach (Face face in faces) {
                face.MoveFaceBy(translation);
            }
            this.position += translation;
            return this;
        }

        public BuildingObject TranslatePosition(Vector3 translation) {
            this.position -= Quaternion.Inverse(rotation) * translation;
            return this;
        }

        public BuildingObject TransformFaces(Vector3 translation, Quaternion rotation) {
            foreach (Face face in faces) {
                face.MoveFaceBy(translation).Rotate(rotation);
            }
            this.position += translation;
            this.rotation *= rotation;
            return this;
        }

        public BuildingObject RotateFaces(Quaternion rotation) {
            foreach (Face face in faces) {
                face.Rotate(rotation);
            }
            return this;
        }

        public Bounds CalculateGlobalBounds() {
            Bounds bounds = new Bounds(rotation * (faces[0].a + position), Vector3.zero);
            foreach (Face face in faces) {
                foreach (Vector3 p in face.GetVertices()) {
                    bounds.Encapsulate(rotation * (p + position));
                }
            }
            return bounds;
        }

        public BuildingObject MakeHole(Vector3 origin, Vector3 direction, Vector3 up, float width, float height) {
            List<Face> result = new List<Face>();
            Vector3 intersection;
            bool fromBack;
            List<Face> affectedFaces = new List<Face>();
            List<Face> unaffectedFaces = new List<Face>();
            foreach (Face face in faces) {
                if (face.RayHit(origin, direction, false, out fromBack, out intersection)) {
                    face.sortOrder = Vector3.Distance(origin, intersection);
                    affectedFaces.Add(face);
                } else {
                    unaffectedFaces.Add(face);
                }
            }
            // sort by distance
            affectedFaces.Sort((f1, f2) => f1.sortOrder.CompareTo(f2.sortOrder));
            // keep track of the previous cut face
            Face previousCutFace = null;

            for (int cut = 0; cut < 4; cut++) {
                Face thisCutFace = null;
                foreach (Face face in affectedFaces) {
                    if (face.RayHit(origin, direction, false, out fromBack, out intersection)) {
                        // compute corners of the hole
                        Vector3 localRight = Vector3.Cross(face.normal, up);
                        Vector3 ha = intersection - localRight * width/2 - up * height/2;
                        Vector3 hb = ha + up * height;
                        Vector3 hc = hb + localRight * width;
                        Vector3 hd = hc - up * height;
                        // a plane through ha,hb and any point on the face normal that is not on the face defines our left cutting plane
                        // the normal of this cutting plane is face.normal x up = localRight
                        /*
                        DrawLine(ha, hb, transform);
                        DrawLine(hb, hc, transform);
                        DrawLine(hc, hd, transform);
                        DrawLine(hd, ha, transform);
                        Debug.Log("hit a face: " + face);
                        */
                        Vector3 vCut = ha;
                        Vector3 nCut = localRight;
                        if (cut == 1) {
                            vCut = hc;
                        } else if (cut == 2) {
                            vCut = hb;
                            nCut = up;
                        } else if (cut == 3) {
                            vCut = ha;
                            nCut = up;
                        }
                        float dA = Vector3.Dot(face.a-vCut, nCut);
                        float dB = Vector3.Dot(face.b-vCut, nCut);
                        float dC = Vector3.Dot(face.c-vCut, nCut);
                        float dD = Vector3.Dot(face.d-vCut, nCut);
                        //Debug.LogFormat("da={0},db={1},dc={2},dd={3}", dA, dB, dC, dD);
                        // if all determinants have the same sign there is no edge to split
                        if (Mathf.Sign(dA) != Mathf.Sign(dB) || Mathf.Sign(dB) != Mathf.Sign(dC) || Mathf.Sign(dC) != Mathf.Sign(dD)) {
                            // check which edges we have to split
                            float rAB = (dA*dB)>=0 ? 0 : Mathf.Abs(dA/Mathf.Abs(dA-dB));
                            float rBC = (dB*dC)>=0 ? 0 : Mathf.Abs(dB/Mathf.Abs(dB-dC));
                            float rCD = (dC*dD)>=0 ? 0 : Mathf.Abs(dC/Mathf.Abs(dC-dD));
                            float rDA = (dD*dA)>=0 ? 0 : Mathf.Abs(dD/Mathf.Abs(dD-dA));
                            //Debug.LogFormat("rAB={0},rBC={1},rCD={2},rDA={3}", rAB, rBC, rCD, rDA);
                            if (rAB > 0) {
                                if (rCD > 0) {
                                    // we cut through AB and CD
                                    Face[] f = Builder.SplitFaceABCD(face, rAB, rCD);
                                    if (cut == 3) {
                                        if (dA < 0) {
                                            result.Add(f[1]);
                                            thisCutFace = f[0];
                                        } else {
                                            result.Add(f[0]);
                                            thisCutFace = f[1];
                                        }
                                    } else {
                                        result.Add(f[0]);
                                        result.Add(f[1]);
                                    }
                                }
                            } else if (rBC > 0) {
                                if (rDA > 0) {
                                    // we cut through BC and DA
                                    Face[] f = Builder.SplitFaceBCDA(face, rBC, rDA);
                                    if (cut == 3) {
                                        if (dA < 0) {
                                            result.Add(f[0]);
                                            thisCutFace = f[1];
                                        } else {
                                            result.Add(f[1]);
                                            thisCutFace = f[0];
                                        }
                                    } else {
                                        result.Add(f[0]);
                                        result.Add(f[1]);
                                    }
                                }
                            }
                            if (thisCutFace != null) {
                                if (previousCutFace != null) {
                                    unaffectedFaces.AddRange(Builder.CloseEdgeLoops(
                                        new List<Vector3> { previousCutFace.a, previousCutFace.b, previousCutFace.c, previousCutFace.d, previousCutFace.a},
                                        new List<Vector3> { thisCutFace.d, thisCutFace.c, thisCutFace.b, thisCutFace.a, thisCutFace.d},
                                        1f));
                                    previousCutFace = null;
                                } else {
                                    previousCutFace = thisCutFace;
                                }
                                thisCutFace.Tag(Builder.CUTOUT);
                            }
                        } else {
                            // that should never happen and is here just for debugging
                            //Debug.Log("all points are on the same side, no splitting of " + face);
                            result.Add(face);
                        }
                    } else {
                        //Debug.Log("face not affected by split: " + face);
                        //result.Add(face);
                        unaffectedFaces.Add(face);
                    }
                }
                affectedFaces.Clear();
                affectedFaces.AddRange(result);
                result.Clear();
            }
            // if previous cut face is still set we haven't closed the hole and the face is added back to the mesh
            if (previousCutFace != null) {
                previousCutFace.Tag(Builder.CUTOUT);
                affectedFaces.Add(previousCutFace);
            }
            faces.Clear();
            faces.AddRange(unaffectedFaces);
            faces.AddRange(affectedFaces);
            return this;
        }

        void DrawLine(Vector3 a, Vector3 b, Transform transform) {
            Debug.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b), Color.white, 10);
        }

        public BuildingObject MakeHole(Vector3 origin, Vector3 direction, float width, float height) {
            List<Face> result = new List<Face>();
            Quaternion tiltRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
            int facesHitFront = 0;
            int facesHitBack = 0;
            Face lastFrontFace = new Face();
            Face lastBackFace = new Face();
            bool tilt = false;
            // this logic won't work if the direction is up or down, so we tilt the faces
            if (direction == Vector3.up) {
                tilt = true;
                direction = Vector3.forward;
                origin = tiltRotation * origin;
            }
            foreach (Face face in faces) {
                if (tilt) {
                    face.Rotate(tiltRotation);
                }
                Vector3 intersection;
                bool fromBack;
                if (face.RayHit(origin, direction, false, out fromBack, out intersection)) {
                    // slice vertically
                    if (!fromBack) {
                        Vector3 v0 = intersection + Vector3.Cross(direction, Vector3.up) * width/2;
                        v0.y = face.b.y;
                        Vector3 v1 = intersection + Vector3.Cross(direction, Vector3.up) * width/2;
                        v1.y = face.a.y;
                        //Debug.Log("1.Slice " + face + " at " + v0 + "," + v1);
                        Face[] leftFaces = Builder.SliceFace(face, v0, v1);
                        result.Add(leftFaces[0]);
                        if (leftFaces.Length == 2) {
                            v0 -= Vector3.Cross(direction, Vector3.up) * width;
                            v1 -= Vector3.Cross(direction, Vector3.up) * width;
                            //Debug.Log("2.Slice " + leftFaces[1] + " at " + v0 + "," + v1);
                            Face[] rightfaces = Builder.SliceFace(leftFaces[1], v0, v1);
                            if (rightfaces.Length == 2) {
                                result.Add(rightfaces[1]);
                                // slice horizontally
                                float relY = (intersection.y - height/2 - rightfaces[0].a.y)/(rightfaces[0].b.y - rightfaces[0].a.y);
                                //Debug.Log("1.h slice, relY=" + relY);
                                Face[] middlefaces = Builder.SliceFaceHorizontally(rightfaces[0], relY);
                                if (middlefaces.Length == 2) {
                                    result.Add(middlefaces[0]);
                                    relY = (intersection.y + height/2 - middlefaces[1].a.y)/(middlefaces[1].b.y - middlefaces[1].a.y);
                                    //Debug.Log("2.h slice, relY=" + relY);
                                    Face[] bottomFaces = Builder.SliceFaceHorizontally(middlefaces[1], relY);
                                    result.Add(bottomFaces[1]);
                                    lastFrontFace = bottomFaces[0];
                                } else {
                                    result.Add(rightfaces[0]);
                                }
                                facesHitFront++;
                                if (facesHitFront == facesHitBack) {
                                    // close the sides of the hole
                                    result.AddRange(Builder.CloseEdgeLoops(
                                        new List<Vector3> { lastFrontFace.a, lastFrontFace.b, lastFrontFace.c, lastFrontFace.d, lastFrontFace.a},
                                        new List<Vector3> { lastBackFace.d, lastBackFace.c, lastBackFace.b, lastBackFace.a, lastBackFace.d},
                                        1f));
                                }
                            } else {
                                // in case the slicing failed we add the original face
                                result.Add(leftFaces[1]);
                            }
                        }
                    } else {
                        Vector3 v0 = intersection + Vector3.Cross(direction, Vector3.up) * width/2;
                        v0.y = face.b.y;
                        Vector3 v1 = intersection + Vector3.Cross(direction, Vector3.up) * width/2;
                        v1.y = face.a.y;
                        //Debug.Log("1.Slice " + face + " at " + v0 + "," + v1);
                        Face[] leftFaces = Builder.SliceFace(face, v0, v1);
                        if (leftFaces.Length == 2) {
                            result.Add(leftFaces[1]);
                            v0 -= Vector3.Cross(direction, Vector3.up) * width;
                            v1 -= Vector3.Cross(direction, Vector3.up) * width;
                            //Debug.Log("2.Slice " + leftFaces[0] + " at " + v0 + "," + v1);
                            Face[] rightfaces = Builder.SliceFace(leftFaces[0], v0, v1);
                            if (rightfaces.Length == 2) {
                                result.Add(rightfaces[0]);
                                // slice horizontally
                                float relY = (intersection.y - height/2 - rightfaces[1].a.y)/(rightfaces[1].b.y - rightfaces[1].a.y);
                                //Debug.Log("1.h slice, relY=" + relY);
                                Face[] middlefaces = Builder.SliceFaceHorizontally(rightfaces[1], relY);
                                if (middlefaces.Length == 2) {
                                    result.Add(middlefaces[0]);
                                    relY = (intersection.y + height/2 - middlefaces[1].a.y)/(middlefaces[1].b.y - middlefaces[1].a.y);
                                    //Debug.Log("2.h slice, relY=" + relY);
                                    Face[] bottomFaces = Builder.SliceFaceHorizontally(middlefaces[1], relY);
                                    result.Add(bottomFaces[1]);
                                    lastBackFace = bottomFaces[0];
                                }
                                facesHitBack++;
                                if (facesHitFront == facesHitBack) {
                                    //Debug.Log("close edge loop");
                                    // close the sides of the hole
                                    result.AddRange(Builder.CloseEdgeLoops(new List<Vector3> { lastFrontFace.a, lastFrontFace.b, lastFrontFace.c, lastFrontFace.d, lastFrontFace.a}, new List<Vector3> { lastBackFace.a, lastBackFace.b, lastBackFace.c, lastBackFace.d, lastBackFace.a}, 1f));
                                }
                            } else {
                                // in case the slicing failed we add the original face
                                result.Add(leftFaces[0]);
                            }
                        } else {
                            result.Add(face);
                        }
                    }
                } else {
                    result.Add(face);
                }
            }
            if (facesHitBack > facesHitFront) {
                lastBackFace.Tag(Builder.CUTOUT);
                result.Add(lastBackFace);
                //Debug.Log("mark " + lastBackFace);
            } else if (facesHitFront > facesHitBack) {
                lastFrontFace.Tag(Builder.CUTOUT);
                result.Add(lastFrontFace);
            }
            //Debug.Log("faces hit front: " + facesHitFront + ", faces hit back: " + facesHitBack);
            faces = result;
            if (tilt) {
                RotateFaces(Quaternion.Inverse(tiltRotation));
            }
            return this;
        }

        public Face FindFirstFaceByTag(int tag) {
            return Builder.FindFirstFaceByTag(faces, tag);
        }

        public BuildingObject CutFront(Rect dim, float uvScale) {
            return CutFront(dim, true, uvScale);
        }

        public BuildingObject CutFront(Rect dim, bool indent, float uvScale) {
            List<Face> result = new List<Face>();
            // project the 2D rect on this face (the normal needs to point to Vector3.back)
            // the z is not used so we don't care
            // check which faces are affected
            foreach (Face face in faces) {
                List<Face> n = new List<Face>();
                Face cutoutFace = Builder.ProjectRectOnFrontFace(dim, 0);
                // is the cutout part of this face?
                if (cutoutFace.a.x > face.a.x && cutoutFace.a.y >= face.a.y && cutoutFace.c.x < face.c.x && cutoutFace.c.y <= face.c.y) {
                    // slice the face into 9 parts leaving the center piece at the size of the cutout
                    Face[] nf = Builder.SliceFace(face, cutoutFace.a.x - face.a.x, 0);

                    Face leftColumn = nf[0];
                    Face[] nf2 = Builder.SliceFace(nf[1], cutoutFace.d.x - nf[1].a.x, 0);
                    Face middleColumn = nf2[0];
                    Face rightColumn = nf2[1];

                    /*                    
                    nf = Builder.SliceFace(leftColumn, 0, cutoutFace.a.y - leftColumn.a.y);
                    n.Add(nf[0]); // bottom left corner
                    nf2 = Builder.SliceFace(nf[1], 0, cutoutFace.b.y-nf[1].a.y);
                    n.Add(nf2[1]); // top left corner
                    n.Add(nf2[0]); // left middle
                    */
                    n.Add(leftColumn);

                    nf = Builder.SliceFace(middleColumn, 0, cutoutFace.a.y - middleColumn.a.y);
                    if (Mathf.Abs(nf[0].b.y - nf[0].a.y) > 1e-3f)
                        n.Add(nf[0]); // bottom center
                    nf2 = Builder.SliceFace(nf[1], 0, cutoutFace.b.y - nf[1].a.y);
                    //result.Add(nf2[0]); // center
                    nf2[0].Tag(Builder.CUTOUT);
                    if (indent)
                        n.AddRange(Builder.IndentFace(nf2[0], new Vector3(0, 0, 0.3f), uvScale));
                    if (Mathf.Abs(nf2[1].b.y - nf2[1].a.y) > 1e-3f)
                        n.Add(nf2[1]); // top center

                    /*
                    nf = Builder.SliceFace(rightColumn, 0, cutoutFace.a.y-rightColumn.a.y);
                    n.Add(nf[0]); // bottom right corner
                    nf2 = Builder.SliceFace(nf[1], 0, cutoutFace.b.y-nf[1].a.y);
                    n.Add(nf2[0]); // right middle
                    n.Add(nf2[1]); // top right corner
                    */
                    n.Add(rightColumn);
                    result.AddRange(n);
                } else {
                    //Debug.Log("cutout is not part of this face. cutout=" + cutoutFace + ", face=" + face);
                    result.Add(face);
                }
            }
            faces = result;
            return this;
        }

    }
}