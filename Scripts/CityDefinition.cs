using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CityDefinition {

    public GameObject parent;
    public List<GameObject> houses;
    public List<Street> streets;
    public int seed;
    public float yOffset = 0;
    public RoadPainting roadPainting;

    [Serializable]
    public class Street {
        public string name;
        public float length;
        public float doorToStreet = 3.5f;
        public float houseToHouse = 0.8f;
        public List<Vector3> points;
        public List<Transform> transforms;
    }
    [Serializable]
    public class RoadPainting {
        public Terrain terrain;
        public int layerIndex;
        [Range(0,1)]
        public float maxAlpha = 1f;
        public int paintRadius = 2;
    }
}
