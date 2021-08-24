using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WallDefinition {
    public bool useChildren = true;
    public List<Transform> points;
    public float wallHeight = 2f;
    public float heightOffset = 0;
    public float wallThickness = 1f;
    public bool sortMarkers = true;
    public bool closeLoop = true;
    public Material material;
    public float uvScale = 1;
    public bool generateCornerPieces = true;
    public GameObject cornerPiece;
}
