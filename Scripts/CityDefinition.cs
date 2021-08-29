using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CityDefinition {

    [Tooltip("This is going to be the parent of all generated houses.")]
    public GameObject parent;
    [Tooltip("Prefab of house, is picked from randomly")]
    public List<GameObject> houses;
    [Tooltip("Transforms used as a path to draw the streets along")]
    public List<Street> streets;
    [Tooltip("Random number generator will be initialized with this value")]
    public int seed;
    [Tooltip("General ground offset of all houses, set this if you have z-fighting issues")]
    public float yOffset = 0;
    public RoadPainting roadPainting;

    [Serializable]
    public class Street {
        public string name;
        public float length;
        [Tooltip("Distance from the middle of the street to the front of the house")]
        public float doorToStreet = 3.5f;
        [Tooltip("Distance between houses on he same side of the street")]
        public float houseToHouse = 0.8f;
        [Tooltip("Inhibit houses on the left side of the street")]
        public bool abandonLeft = false;
        [Tooltip("Inhibit houses on the right side of the street")]
        public bool abandonRight = false;
        public List<Vector3> points;
        public List<Transform> transforms;
    }
    [Serializable]
    public class RoadPainting {
        [Tooltip("Terrain to paint road on")]
        public Terrain terrain;
        [Tooltip("Index to the terrain layer with the road texture, starts at 0")]
        public int layerIndex;
        [Tooltip("max. strength to be used for road texture")]
        [Range(0,1)]
        public float maxAlpha = 1f;
        [Tooltip("Brush radius in pixels")]
        public int paintRadius = 2;
    }
}
