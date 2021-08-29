using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralStructures {
    public class CityBuilder {

        public void PlaceHouses(CityDefinition city) {
            Random.InitState(city.seed);

            List<GameObject> generated = new List<GameObject>();
            foreach (CityDefinition.Street street in city.streets) {

                float streetLength = street.length;

                for (int side = -1; side <= 1; side+=2) {
                    if (side == -1 && street.abandonLeft) continue;
                    if (side == 1 && street.abandonRight) continue;
                    int number = 1;
                    if (side > 0) number++;
                    float rightOffset = street.houseToHouse;
                    while (rightOffset < streetLength) {
                        GameObject prefab = RandomHouse(city);
                        HouseDefinition houseDefinition = prefab.GetComponent<HouseBuilder>().houseDefinition;
                        // this points to the right of the streets beginning looking towards the end
                        float segmentStart = 0;
                        Vector3[] segment = GetStreetSegmentAt(street, rightOffset, out segmentStart);
                        Vector3 normal = GetStreetNormalAt(segment);
                        Vector3 pos = segment[0] + normal * (houseDefinition.length/2 + street.doorToStreet) * side
                            + (segment[1]-segment[0]).normalized * (houseDefinition.width/2 + rightOffset - segmentStart);
                        rightOffset += street.houseToHouse + houseDefinition.width;
                        if (rightOffset > streetLength) {
                            break;
                        }
                        Vector3 frontCenter = pos - normal * houseDefinition.length/2 * side;
                        pos.y = Terrain.activeTerrain.SampleHeight(frontCenter) + city.yOffset;
                        GameObject marker1 = GameObject.Instantiate(prefab);
                        generated.Add(marker1);
                        marker1.transform.parent = city.parent.transform;
                        marker1.transform.position = pos;
                        marker1.transform.rotation = Quaternion.LookRotation(normal * side);
                        marker1.isStatic = true;
                        HouseBuilder hb = marker1.GetComponent<HouseBuilder>();
                        hb.streetName = street.name;
                        hb.number = number;
                        number+=2;
                    }
                }
            }

            for (int i = generated.Count-1; i >= 0; i--) {
                GameObject go = generated[i];
                HouseBuilder hb = go.GetComponent<HouseBuilder>();
                Vector3 boundingCenter = hb.calculateCenter();
                Vector3 boundingCenterW = go.transform.TransformPoint(boundingCenter);
                Vector3 boundingBoxSize = hb.calculateSize();
                float maxRadius = boundingBoxSize.magnitude/2;
                Bounds bounds = new Bounds(boundingCenter, boundingBoxSize);
                bool removeIt = BoundsIntersectsStreets(go.transform, bounds, city.streets);
                
                if (!removeIt) { for (int j = 0; j < i; j++) {
                    HouseBuilder hb2 = generated[j].GetComponent<HouseBuilder>();
                    Vector3 boundingCenter2 = hb2.calculateCenter();
                    Vector3 boundingCenter2W = generated[j].transform.TransformPoint(boundingCenter2);
                    Vector3 boundingBoxSize2 = hb2.calculateSize();
                    float maxRadius2 = boundingBoxSize2.magnitude/2;
                    if ((boundingCenterW-boundingCenter2W).magnitude >= (maxRadius+maxRadius2)) {
                        continue;
                    }
                    // test current building to previous
                    foreach (Vector3 testVector in new Vector3[] {
                        boundingCenter2,
                        boundingCenter2 + new Vector3(boundingBoxSize2.x/2, 0, boundingBoxSize2.z/2),
                        boundingCenter2 + new Vector3(-boundingBoxSize2.x/2, 0, boundingBoxSize2.z/2),
                        boundingCenter2 + new Vector3(boundingBoxSize2.x/2, 0, -boundingBoxSize2.z/2),
                        boundingCenter2 + new Vector3(-boundingBoxSize2.x/2, 0, -boundingBoxSize2.z/2),
                    }) {
                        Vector3 world = generated[j].transform.TransformPoint(testVector);
                        Vector3 local = go.transform.InverseTransformPoint(world);
                        removeIt = bounds.Contains(local);
                        if (removeIt) break;
                    }
                    if (!removeIt) {
                        // test previous to current
                        Bounds bounds2 = new Bounds(boundingCenter2, boundingBoxSize2);
                        foreach (Vector3 testVector in new Vector3[] {
                            boundingCenter,
                            boundingCenter + new Vector3(boundingBoxSize.x/2, 0, boundingBoxSize.z/2),
                            boundingCenter + new Vector3(-boundingBoxSize.x/2, 0, boundingBoxSize.z/2),
                            boundingCenter + new Vector3(boundingBoxSize.x/2, 0, -boundingBoxSize.z/2),
                            boundingCenter + new Vector3(-boundingBoxSize.x/2, 0, -boundingBoxSize.z/2),
                        }) {
                            Vector3 world = go.transform.TransformPoint(testVector);
                            Vector3 local = generated[j].transform.InverseTransformPoint(world);
                            removeIt = bounds2.Contains(local);
                            if (removeIt) break;
                        }
                    }
                    if (removeIt) {
                        break;
                    }
                }
                }
                if (removeIt) {
                    go.SetActive(false);
                }
            }
        }

        GameObject RandomHouse(CityDefinition city) {
            return city.houses[Random.Range(0, city.houses.Count)];
        }

        Vector3[] GetStreetSegmentAt(CityDefinition.Street street, float offset, out float segmentStart) {
            segmentStart = 0;
            Vector3 a = street.points[0];
            Vector3 b = street.points[1];
            for (int index = 0; index < street.points.Count - 1; index++) {
                a = street.points[index];
                b = street.points[index+1];
                float segmentLength = Vector3.Distance(a, b);
                if (offset >= segmentStart && offset < (segmentStart + segmentLength)) {
                    break;
                }
                segmentStart += segmentLength;
            }
            return new Vector3[] { a, b };
        }

        IEnumerable<Vector3[]> StreetSegments(CityDefinition.Street street) {
            for (int index = 0; index < street.points.Count -1; index++) {
                yield return new Vector3[] { street.points[index], street.points[index+1] };
            }
        }

        Vector3 GetStreetNormalAt(Vector3[] segment) {
            Vector3 a = segment[0];
            Vector3 b = segment[1];
            return Vector3.Cross(a-b, Vector3.up).normalized;
        }

        bool BoundsIntersectsStreets(Transform transform, Bounds bounds, List<CityDefinition.Street> streets) {
            bool result = false;
            foreach (CityDefinition.Street street in streets) {
                result = BoundsIntersectsStreet(transform, bounds, street);
                if (result) break;
            }
            return result;
        }

        bool BoundsIntersectsStreet(Transform transform, Bounds bounds, CityDefinition.Street street) {
            bool result = false;
            float hitDistance;
            Ray segmentRay = new Ray();
            for (int i = 0; i < street.points.Count - 1; i++) {
                Vector3 l = street.points[i+1] - street.points[i];
                float segmentLength = l.magnitude;
                segmentRay.origin = transform.InverseTransformPoint(street.points[i]);
                segmentRay.direction = transform.InverseTransformDirection(l.normalized);
                if (bounds.IntersectRay(segmentRay, out hitDistance)) {
                    if (hitDistance < segmentLength) {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public void RemoveHouses(CityDefinition city) {
            for (int i = city.parent.transform.childCount-1; i>=0; i--) {
                GameObject go = city.parent.transform.GetChild(i).gameObject;
                if (Application.isPlaying) {
                    Object.Destroy(go);
                } else {
                    Object.DestroyImmediate(go);
                }
            }

        }

        public void RepairTerrainAlphamap(CityDefinition city) {
            int mx = city.roadPainting.terrain.terrainData.alphamapWidth;
            int my = city.roadPainting.terrain.terrainData.alphamapHeight;
            float[,,] alphaMap = city.roadPainting.terrain.terrainData.GetAlphamaps(0, 0, mx, my);
            int errors = 0;
            int corrected = 0;
            for (int y = 0; y < my; y++) {
                for (int x = 0; x < mx; x++) {
                    float sumAlpha = 0;
                    for (int i = 0; i < city.roadPainting.terrain.terrainData.alphamapLayers; i++) {
                        sumAlpha += alphaMap[x, y, i];
                    }
                    if (Mathf.Abs(1f - sumAlpha) > 1e-2f) {
                        errors++;
                        if (sumAlpha > 0) {
                            for (int i = 0; i < city.roadPainting.terrain.terrainData.alphamapLayers; i++) {
                                alphaMap[x, y, i] /= sumAlpha;
                            }
                            corrected++;
                        }
                    }
                }
            }
            if (errors > 0) {
                city.roadPainting.terrain.terrainData.SetAlphamaps(0, 0, alphaMap);
                Debug.Log("alpha mismatch on " + errors + " pixels of " + (mx * my) + ", corrected: " + corrected);
            } else {
                Debug.Log("Alpha map is OK.");
            }
        }

        public void PaintTerrain(CityDefinition city) {
            CityDefinition.RoadPainting painting = city.roadPainting;
            Terrain terrain = painting.terrain;
            if (terrain == null) terrain = Terrain.activeTerrain;
            int nLayers = terrain.terrainData.alphamapLayers;
            float roadAlpha = painting.maxAlpha;
            float stepSize = 1;
            int splatSize = painting.paintRadius * 2 - 1;
            int lIdx = painting.layerIndex;

            foreach (Vector3 sample in LocationOnStreets(city, stepSize)) {
                Vector2 terrainUV = WorldToTextureCoordinate(sample, terrain);
                int posX = Mathf.RoundToInt(terrainUV.x) - splatSize/2;
                int posZ = Mathf.RoundToInt(terrainUV.y) - splatSize/2;
                float[,,] alphaMap = terrain.terrainData.GetAlphamaps(posX, posZ, splatSize, splatSize);
                for (int y = 0; y < splatSize; y++) {
                    float dy = Mathf.Abs(splatSize/2 - y);
                    for (int x = 0; x < splatSize; x++) {
                        float dx = Mathf.Abs(splatSize/2 - x);
                        float splatAlpha = Mathf.Clamp01(roadAlpha - Mathf.Clamp01((dx*dx + dy*dy)/splatSize));
                        float otherAlpha = 0;
                        for (int i = 0; i < nLayers; i++) {
                            if (i != lIdx) {
                                otherAlpha += alphaMap[x,y,i];
                            }
                        }
                        float deltaAlpha = -alphaMap[x, y, lIdx];
                        alphaMap[x, y, lIdx] = Mathf.Max(splatAlpha, alphaMap[x, y, lIdx]);
                        deltaAlpha += alphaMap[x, y, lIdx];
                        if (otherAlpha > 0) {
                            for (int i = 0; i < nLayers; i++) {
                                if (i != lIdx) {
                                    alphaMap[x,y,i] *= 1f - deltaAlpha/otherAlpha;
                                }
                            }
                        }
                    }
                }
                terrain.terrainData.SetAlphamaps(posX, posZ, alphaMap);
            }
        }

        IEnumerable<Vector3> LocationOnStreet(CityDefinition.Street street, float stepSize) {
            float currentMark = 0;
            float streetLength = street.length;
            float pastSegmentsLength = 0;
            foreach (Vector3[] segment in StreetSegments(street)) {
                float segmentMark = currentMark - pastSegmentsLength;
                float segmentLength = (segment[1]-segment[0]).magnitude;
                while (segmentMark <= segmentLength) {
                    Vector3 pos = segment[0] + (segment[1]-segment[0]).normalized * segmentMark;
                    currentMark += stepSize;
                    segmentMark += stepSize;
                    yield return pos;
                }
                pastSegmentsLength += segmentLength;
            }
        }

        IEnumerable<Vector3> LocationOnStreets(CityDefinition city, float stepSize = 1) {
            foreach (CityDefinition.Street street in city.streets) {
                foreach (Vector3 v in LocationOnStreet(street, stepSize)) {
                    yield return v;
                }
            }
        }
        Vector2 WorldToTextureCoordinate(Vector3 position, Terrain terrain) {
            Vector3 localPosition = position - terrain.transform.position;
            float relativeX = localPosition.x / terrain.terrainData.size.x;
            float relativeZ = localPosition.z / terrain.terrainData.size.z;
            return new Vector2(relativeX * terrain.terrainData.alphamapWidth, relativeZ * terrain.terrainData.alphamapHeight);
        }
    }
}
