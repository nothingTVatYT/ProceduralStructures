using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "HouseDefinition", menuName = "Procedural Structures/House Definition", order = 1)]
public class HouseDefinition : ScriptableObject
{
    public enum Side {Front, Back, Right, Left}

    [Serializable]
    public class WallCutout {
        public string name;
        public Side side = Side.Front;
        public Rect dimension;
        public Material material;
        public float uvScale = 1;
        public GameObject prefab;
    }

    [Serializable]
    public class Stairs {
        public Side side = Side.Front;
        public bool inside = false;
        public float offset = 0;
        public float baseWidth = 1;
        public float baseLength = 0.6f;
        public float baseHeight = 0;
        public float descentAngle = 0f;
        public float totalHeight;
        public float stepHeight = 0.25f;
        public float stepDepth = 0.4f;
        public Material material;
        public float uvScale;
    }

    [Serializable]
    public class BuildingStructure {
        public string name;
        public float height;
        public bool hollow;
        public bool addCeiling;
        public bool addFloor = true;
        public float wallThickness = 0.5f;

        [Tooltip("This is an indent per unit height.")]
        public float slopeX = 0;
        [Tooltip("This is an indent per unit height.")]
        public float slopeZ = 0;
        public Material material;
        public float uvScale = 1;
        public WallCutout[] cutouts;
        public Stairs[] stairs;
    }

    [Header("Basement Settings")]
    public float heightOffset = -2;
    public float width = 9;
    public float length = 6;
    public bool constructFrameHouse;
 
    public List<BuildingStructure> layers = new List<BuildingStructure>();

    [Header("Roof")]
    public float roofHeight;
    public Material materialRoof;
    public float uvScaleRoof = 1;
    public Material materialGable;
    public float uvScaleGable = 1;
    public float roofExtendX;
    public float roofExtendZ;
    public float roofThickness;

    public float totalHeight {
        get {
            float h = roofHeight;
            foreach (BuildingStructure l in layers) { h+=l.height; }
            return h;
            }
        }
}
