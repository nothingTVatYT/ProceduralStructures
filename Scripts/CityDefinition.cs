using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CityDefinition {

    public GameObject parent;
    public List<GameObject> houses;
    public List<Street> streets;
    public float doorToStreet = 3.5f;
    public float houseToHouse = 0.8f;
    public int seed;

    [Serializable]
    public class Street {
        public string name;
        public float length;
        public List<Vector3> points;
        public List<Transform> transforms;
    }
}
