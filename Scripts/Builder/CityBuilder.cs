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
                int number = 1;

                for (int side = -1; side <= 1; side+=2) {
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
                        pos.y = Terrain.activeTerrain.SampleHeight(frontCenter);
                        GameObject marker1 = GameObject.Instantiate(prefab);
                        generated.Add(marker1);
                        marker1.transform.parent = city.parent.transform;
                        marker1.transform.position = pos;
                        marker1.transform.rotation = Quaternion.LookRotation(normal * side);
                        marker1.isStatic = true;
                        HouseBuilder hb = 
                        marker1.GetComponent<HouseBuilder>();
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
                for (int j = 0; j < i; j++) {
                    HouseBuilder hb2 = generated[j].GetComponent<HouseBuilder>();
                    Vector3 boundingCenter2 = hb2.calculateCenter();
                    Vector3 boundingCenter2W = generated[j].transform.TransformPoint(boundingCenter2);
                    Vector3 boundingBoxSize2 = hb2.calculateSize();
                    float maxRadius2 = boundingBoxSize2.magnitude/2;
                    if ((boundingCenterW-boundingCenter2W).magnitude >= (maxRadius+maxRadius2)) {
                        continue;
                    }
                    bool removeIt = false;
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
                        go.SetActive(false);
                    }
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

        Vector3 GetStreetNormalAt(Vector3[] segment) {
            Vector3 a = segment[0];
            Vector3 b = segment[1];
            return Vector3.Cross(a-b, Vector3.up).normalized;
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
    }
}
