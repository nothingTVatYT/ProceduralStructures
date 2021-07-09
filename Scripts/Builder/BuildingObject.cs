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

        public BuildingObject AddFace(Face face) {
            faces.Add(face);
            return this;
        }

        public BuildingObject AddFaces(List<Face> newFaces) {
            faces.AddRange(newFaces);
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

        public BuildingObject CutFront(Rect dim, float uvScale) {
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