using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WallBuilder))]
public class WallBuilderEditor : Editor
{
    private WallBuilder wallBuilder;
    private static string GENERATED = "Generated GameObjects";

    class Face {
        public Vector3 a,b,c,d;
        public Vector2 uvA,uvB,uvC,uvD;
        public void setUVFront(float width, float height) {
            uvA = new Vector2(width, 0);
            uvB = new Vector2(width, height);
            uvC = new Vector2(0, 0);
            uvD = new Vector2(0, height);
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        wallBuilder = (WallBuilder)target;
        if (GUILayout.Button("Rebuild")) {
            RebuildWall();
        }
    }

    void RebuildWall() {
        if (wallBuilder.useChildren) {
            wallBuilder.points.Clear();
            foreach (Transform t in wallBuilder.gameObject.transform) {
                if (t.gameObject.name != GENERATED)
                    wallBuilder.points.Add(t);
            }
        }
        int minNumberMarkers = wallBuilder.closeLoop ? 3 : 2;
        if (wallBuilder.points.Count >= minNumberMarkers) {
            Vector3 centroid = findCentroid(wallBuilder.points);
            if (wallBuilder.sortMarkers) {
                wallBuilder.points.Sort(delegate(Transform a, Transform b) {
                    float angleA = Mathf.Atan2(a.position.z - centroid.z, a.position.x - centroid.x);
                    float angleB = Mathf.Atan2(b.position.z - centroid.z, b.position.x - centroid.x);
                    return angleA < angleB ? 1 : angleA > angleB ? -1 : 0;
                });
            }
            float tilingX = wallBuilder.tilingX;
            float tilingY = wallBuilder.tilingY;
            MeshRenderer renderer = wallBuilder.gameObject.GetComponent<MeshRenderer>();
            if (renderer == null) {
                renderer = wallBuilder.gameObject.AddComponent<MeshRenderer>();
            }
            if (wallBuilder.material != null) {
                renderer.sharedMaterial = wallBuilder.material;
            } else {
                renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            }
            MeshFilter meshFilter = wallBuilder.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null) {
                meshFilter = wallBuilder.gameObject.AddComponent<MeshFilter>();
            }
            Mesh mesh = new Mesh();
            int segments = wallBuilder.points.Count;
            if (!wallBuilder.closeLoop) {
                segments--;
            }
            int nFaces = segments * 3;
            if (!wallBuilder.closeLoop) {
                nFaces += 2;
            }
            if (wallBuilder.generateCornerPieces) {
                nFaces += 10;
            }
            Debug.Log("Generate " + segments + " segments with " + nFaces + " faces.");
            Face[] faces = new Face[nFaces];

            float wallLength = 0;
            float wallThickness = wallBuilder.wallThickness;
            int faceIndex = 0;
            Vector3 wallHeight = new Vector3(0, wallBuilder.wallHeight, 0);
            Vector3 heightOffset = new Vector3(0, wallBuilder.heightOffset, 0);
            for (int i = 0; i < segments; i++) {
                // outer face
                Face face = new Face();
                face.a = wallBuilder.points[i].position + heightOffset;
                face.b = face.a + wallHeight;
                if (i == wallBuilder.points.Count-1) {
                    face.c = wallBuilder.points[0].position + heightOffset;
                } else {
                    face.c = wallBuilder.points[i+1].position + heightOffset;
                }
                face.d = face.c + wallHeight;
                float segmentLength = distanceXZ(face.a, face.c);
                face.setUVFront(segmentLength * tilingX, wallHeight.y * tilingY);

                faces[faceIndex++] = face;
                wallLength += segmentLength;

                // inner face
                Vector3 insetDirectionA = (centroid-face.a).normalized;
                Vector3 insetDirectionC = (centroid-face.c).normalized;
                if (!wallBuilder.closeLoop) {
                    if (i==0) {
                        insetDirectionA = Vector3.Cross(face.a-face.c, face.b-face.a).normalized;
                    } else if (i == segments-1) {
                        insetDirectionC = Vector3.Cross(face.c-face.a, face.a-face.b).normalized;
                    }
                }
                Face face2 = new Face();
                face2.c = face.a + (insetDirectionA * wallThickness);
                face2.d = face2.c + wallHeight;
                face2.a = face.c + (insetDirectionC * wallThickness);
                face2.b = face2.a + wallHeight;
                face2.setUVFront(segmentLength * tilingX, wallHeight.y * tilingY);
                faces[faceIndex++] = face2;
                if (wallBuilder.generateCornerPieces) {
                    Face[] boxFaces = null;
                    if (i == 0) {
                        boxFaces = generateCornerPiece(face.a + insetDirectionA * wallThickness/2, wallHeight.y+1, wallThickness*2, insetDirectionA, tilingX, tilingY);
                    } else if (i == segments-1) {
                        boxFaces = generateCornerPiece(face.c + insetDirectionC * wallThickness/2, wallHeight.y+1, wallThickness*2, insetDirectionC, tilingX, tilingY);                        
                    }
                    if (boxFaces != null) {
                        foreach (Face f in boxFaces) {
                            faces[faceIndex++] = f;
                        }
                    }
                }

                // top face
                Face face3 = new Face();
                face3.a = face.b;
                face3.b = face2.d;
                face3.c = face.d;
                face3.d = face2.b;
                face3.setUVFront(segmentLength * tilingX, wallThickness * tilingY);
                faces[faceIndex++] = face3;

                if (!wallBuilder.closeLoop) {
                    // end faces
                    if (i == 0) {
                        Face faceStart = new Face();
                        faceStart.a = face2.c;
                        faceStart.b = face2.d;
                        faceStart.c = face.a;
                        faceStart.d = face.b;
                        faceStart.setUVFront(wallThickness * tilingX, wallHeight.y * tilingY);
                        faces[faceIndex++] = faceStart;
                    } else if (i == segments-1) {
                        Face faceEnd = new Face();
                        faceEnd.a = face.c;
                        faceEnd.b = face.d;
                        faceEnd.c = face2.a;
                        faceEnd.d = face2.b;
                        faceEnd.setUVFront(wallThickness * tilingX, wallHeight.y * tilingY);
                        faces[faceIndex++] = faceEnd;
                    }
                }
            }
            Vector3[] vertices = new Vector3[faces.Length * 4];
            Vector2[] uv = new Vector2[faces.Length * 4];
            int[] tris = new int[6 * faces.Length];
            int index = 0;
            int trisIndex = 0;
            foreach (Face face in faces) {
                vertices[index] = face.a;
                uv[index] = face.uvA;
                index++;
                vertices[index] = face.b;
                uv[index] = face.uvB;
                index++;
                vertices[index] = face.c;
                uv[index] = face.uvC;
                index++;
                vertices[index] = face.d;
                uv[index] = face.uvD;
                index++;
                tris[trisIndex++] = index - 4; // A
                tris[trisIndex++] = index - 2; // C
                tris[trisIndex++] = index - 3; // B
                tris[trisIndex++] = index - 3; // B
                tris[trisIndex++] = index - 2; // C
                tris[trisIndex++] = index - 1; // D
            }
            
            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uv;
            mesh.name = "generated wall";
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
            mesh.Optimize();
            MeshCollider collider = wallBuilder.gameObject.GetComponent<MeshCollider>();
            if (collider == null) {
                wallBuilder.gameObject.AddComponent<MeshCollider>();
            } else {
                collider.sharedMesh = mesh;
            }

            if (wallBuilder.cornerPiece != null) {
                GameObject go = null;
                foreach (Transform t in wallBuilder.gameObject.transform) {
                    if (t.gameObject.name == GENERATED) {
                        go = t.gameObject;
                    }
                }
                if (go != null) {
                    DestroyImmediate(go);
                }
                go = new GameObject(GENERATED);
                go.transform.parent = wallBuilder.gameObject.transform;
                
                foreach (Transform t in wallBuilder.points) {
                    GameObject corner = GameObject.Instantiate(wallBuilder.cornerPiece);
                    corner.transform.parent = go.transform;
                    corner.transform.position = t.position + (centroid-t.position).normalized * wallThickness/2;
                    corner.transform.LookAt(new Vector3(centroid.x, corner.transform.position.y, centroid.z));
                }
            }
        }
    }

    private Face[] generateCornerPiece(Vector3 center, float height, float width,
     Vector3 inwards, float tilingX, float tilingY) {
        Vector3 pieceHeight = new Vector3(0, height, 0);
        Vector3 right = Quaternion.Euler(0, -90, 0) * inwards;
        Vector3 a0 = center - right * width/2 - inwards * width/2;
        Vector3 a1 = center - right * width/2 + inwards * width/2;
        Vector3 a2 = center + right * width/2 - inwards * width/2;
        Vector3 a3 = center + right * width/2 + inwards * width/2;
        Face face0 = new Face();
        face0.a = a0;
        face0.b = a0 + pieceHeight;
        face0.c = a2;
        face0.d = a2 + pieceHeight;
        face0.setUVFront(width * tilingX, height * tilingY);
        Face face1 = new Face();
        face1.a = a2;
        face1.b = a2 + pieceHeight;
        face1.c = a3;
        face1.d = a3 + pieceHeight;
        face1.setUVFront(width * tilingX, height * tilingY);
        Face face2 = new Face();
        face2.a = a3;
        face2.b = a3 + pieceHeight;
        face2.c = a1;
        face2.d = a1 + pieceHeight;
        face2.setUVFront(width * tilingX, height * tilingY);
        Face face3 = new Face();
        face3.a = a1;
        face3.b = a1 + pieceHeight;
        face3.c = a0;
        face3.d = a0 + pieceHeight;
        face3.setUVFront(width * tilingX, height * tilingY);
        Face face4 = new Face();
        face4.a = a0 + pieceHeight;
        face4.b = a1 + pieceHeight;
        face4.c = a2 + pieceHeight;
        face4.d = a3 + pieceHeight;
        face4.setUVFront(width * tilingX, width * tilingY);
        return new Face[] {
            face0, face1, face2, face3, face4
        };
    }

    private float distanceXZ(Vector3 a, Vector3 b) {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private Vector3 findCentroid(List<Transform> list) {
        float x = 0;
        float y = 0;
        float z = 0;
        foreach (Transform t in list) {
            x += t.position.x;
            y += t.position.y;
            z += t.position.z;
        }
        x /= list.Count;
        y /= list.Count;
        z /= list.Count;
        return new Vector3(x, y, z);
    }

    private bool isLeftOf(Vector3 m, Vector3 a, Vector3 b) {
        float sign = Mathf.Sign((b.x - a.x) * (m.z - a.z) - (b.z - a.z) * (m.x - a.x));
        Debug.Log(m + " is left of " + a + " to " + b + ": " + sign);
        return sign > 0;
    }
}
