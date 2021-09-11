using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using ExtensionMethods;

namespace ProceduralStructures {
    public class CityBuilder {

        public void UpdateStreets(CityDefinition city) {
            foreach (CityDefinition.Street street in city.streets) {
                UpdateStreetsPoints(street);
            }
        }

        public void UpdateHousePrefabs(CityDefinition city) {
            city.housePlaceholders.Clear();
            foreach (GameObject go in city.houses) {
                HouseDefinition hd = go.GetComponent<HouseBuilder>().houseDefinition;
                city.housePlaceholders.Add(new CityDefinition.HousePlaceholder(hd, go));
            }
            foreach (HouseDefinition houseDef in city.houseDefinitions) {
                city.housePlaceholders.Add(new CityDefinition.HousePlaceholder(houseDef, null));
            }
        }

        public void PlaceHouses(CityDefinition city) {
            Random.InitState(city.seed);
            Terrain currentTerrain = city.terrain;
            if (currentTerrain == null) {
                currentTerrain = Terrain.activeTerrain;
            }
            UpdateStreets(city);
            UpdateHousePrefabs(city);
            List<GameObject> generated = new List<GameObject>();
            foreach (CityDefinition.Street street in city.streets) {
                if (street.name == "" && street.transformsParent != null) {
                    street.name = street.transformsParent.name;
                }

                float streetLength = street.length;

                for (int side = -1; side <= 1; side+=2) {
                    if (side == -1 && street.abandonLeft) continue;
                    if (side == 1 && street.abandonRight) continue;
                    int number = 1;
                    if (side > 0) number++;
                    float rightOffset = street.houseToHouse;
                    while (rightOffset < streetLength) {
                        CityDefinition.HousePlaceholder placeholder = RandomHouse(city);
                        HouseDefinition houseDefinition = placeholder.houseDefinition;
                        float offset = rightOffset + houseDefinition.width/2;
                        Tangent tangent = GetStreetTangentAt(street, offset);
                        // this points to the right of the streets beginning looking towards the end
                        Vector3 normal = Vector3.Cross(Vector3.up, tangent.direction);
                        Vector3 pos = tangent.position + normal * (houseDefinition.length/2 + street.doorToStreet) * side;
                        rightOffset += street.houseToHouse + houseDefinition.width;
                        Vector3 frontCenter = pos - normal * houseDefinition.length/2 * side;
                        if (currentTerrain != null) {
                            pos.y = Terrain.activeTerrain.SampleHeight(frontCenter) + city.yOffset;
                        }
                        GameObject marker1 = InstanceFromPlaceholder(placeholder);
                        generated.Add(marker1);
                        marker1.name = street.name + " " + number + " (" + marker1.name + ")";
                        marker1.transform.parent = city.parent.transform;
                        marker1.transform.position = pos;
                        marker1.transform.rotation = Quaternion.LookRotation(normal * side);
                        marker1.isStatic = true;
                        HouseBuilder hb = marker1.GetComponent<HouseBuilder>();
                        hb.streetName = street.name;
                        hb.number = number;
                        number++;
                        if (!street.abandonLeft && !street.abandonRight) number++;
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

        void UpdateStreetsPoints(CityDefinition.Street street) {
            if (street.useChildNodes && street.transformsParent != null) {
                List<Transform> transforms = new List<Transform>();
                for (int i = 0; i < street.transformsParent.transform.childCount; i++) {
                    transforms.Add(street.transformsParent.transform.GetChild(i));
                }
                UpdateStreetsPointsFromTransforms(street, transforms);
            } else if (street.transforms != null) {
                UpdateStreetsPointsFromTransforms(street, street.transforms);
            }
        }

        void UpdateStreetsPointsFromTransforms(CityDefinition.Street street, List<Transform> transforms) {
            street.points.Clear();
            street.tangents.Clear();
            street.length = 0;
            if (transforms.Count > 0) {
                if (street.smoothCurve) {
                    List<WayPoint> wayPoints = new List<WayPoint>();
                    foreach (Transform tr in transforms) {
                        wayPoints.Add(new WayPoint(tr.position));
                    }
                    BezierSpline spline = new BezierSpline(wayPoints);
                    street.length = spline.EstimatedLength;
                    Vector3 prev = spline.GetVertex(0);
                    float t = 0;
                    float uResolution = 5;
                    float stepSize = uResolution / spline.EstimatedLength;
                    while (t < (1f + stepSize)) {
                        Tangent v = spline.GetTangent(t);
                        t += stepSize;
                        street.tangents.Add(v);
                        street.points.Add(v.position);
                    }
                } else {
                    Vector3 prev = transforms[0].position;
                    for (int i = 0; i < transforms.Count; i++) {
                        Vector3 pos = transforms[i].position;
                        float segmentLength = (pos-prev).magnitude;
                        street.length += segmentLength;
                        street.points.Add(pos);
                        street.tangents.Add(new Tangent(pos, Vector3.zero, street.length, transforms[i].localScale.x, transforms[i].localScale.y));
                        prev = pos;
                    }
                    if (street.tangents.Count > 1) {
                        for (int i = 0; i < street.tangents.Count; i++) {
                            if (i == 0) {
                                street.tangents[0].direction = (street.tangents[1].position - street.tangents[0].position).normalized;
                            } else if (i < street.tangents.Count-1) {
                                street.tangents[i].direction = (street.tangents[i+1].position - street.tangents[i].position).normalized;
                            } else {
                                street.tangents[i].direction = (street.tangents[i].position - street.tangents[i-1].position).normalized;
                            }
                            street.tangents[i].relativePosition /= street.length;
                        }
                    }
                }
            }
        }

        CityDefinition.HousePlaceholder RandomHouse(CityDefinition city) {
            return city.housePlaceholders[Random.Range(0, city.housePlaceholders.Count)];
        }

        GameObject InstanceFromPlaceholder(CityDefinition.HousePlaceholder h) {
            if (h.prefab != null) {
                return GameObject.Instantiate(h.prefab);
            }
            GameObject go = new GameObject(h.houseDefinition.name);
            go.AddComponent<HouseBuilder>().houseDefinition = h.houseDefinition;
            return go;
        }

        Tangent GetStreetTangentAt(CityDefinition.Street street, float offset) {
            if (street.tangents == null || street.tangents.Count == 0 || street.length == 0) {
                return null;
            }
            float relativePosition = Mathf.Clamp01(offset / street.length);
            Tangent tangent = street.tangents[0];
            for (int i = 1; i < street.tangents.Count; i++) {
                Tangent t = street.tangents[i];
                if (t.relativePosition <= relativePosition) {
                    tangent = t;
                } else {
                    // interpolate between (previous) tangent and this
                    float f = (relativePosition - tangent.relativePosition) / (t.relativePosition - tangent.relativePosition);
                    Tangent nt = Tangent.Lerp(tangent, t, f);
                    if (!street.smoothCurve) {
                        nt.direction = tangent.direction;
                    }
                    return nt;
                }
            }
            return tangent;
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
            for (int i = 0; i < street.tangents.Count - 1; i++) {
                Vector3 l = street.tangents[i+1].position - street.tangents[i].position;
                float segmentLength = l.magnitude;
                segmentRay.origin = transform.InverseTransformPoint(street.tangents[i].position);
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
            while (currentMark <= streetLength) {
                Vector3 pos = GetStreetTangentAt(street, currentMark).position;
                currentMark += stepSize;
                yield return pos;
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
