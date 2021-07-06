using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBuilder : MonoBehaviour
{
    public bool useChildren = true;
    public List<Transform> points;
    public float wallHeight = 2f;
    public float heightOffset = 0;
    public float wallThickness = 1f;
    public bool sortMarkers = true;
    public bool closeLoop = true;
    public Material material;
    public float tilingX = 1;
    public float tilingY = 1;
    public bool generateCornerPieces = true;
    public GameObject cornerPiece;

    // Start is called before the first frame update
    void Start()
    {
        HideMarkers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HideMarkers() {
        List<Transform> children;
        if (useChildren) {
            children = new List<Transform>();
            foreach (Transform t in gameObject.transform) {
                children.Add(t);
            }
        } else {
            children = points;
        }
        foreach (Transform t in children) {
            t.gameObject.SetActive(false);
        }
    }
}
