using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "HouseDefinition", menuName = "Procedural Structures/House Definition", order = 1)]
public class HouseDefinition : ScriptableObject
{
    [Serializable]
    public class BuildingStructure {
        public string name;
        public float height;
        public bool isRoof = false;
        [Tooltip("This is an indent per unit height.")]
        public float slopeX = 0;
        [Tooltip("This is an indent per unit height.")]
        public float slopeZ = 0;
        public Material material;
        public float uvScale = 1;
    }

    [Header("Basement Settings")]
    public float heightOffset = -2;
    public float width = 9;
    public float length = 6;
 
    public List<BuildingStructure> layers = new List<BuildingStructure>();

    [Header("Roof")]
    public float roofHeight;
    public float gableWidth;
    public Material materialRoof;
    public Material materialGable;
    public float roofExtendX;
    public float roofExtendZ;
    public float roofThickness;


}
