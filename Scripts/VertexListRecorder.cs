using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VertexList", menuName = "Procedural Structures/Vertex List", order = 1)]
public class VertexListRecorder : ScriptableObject {
    public string listName;
    public List<Vector3> vertices = new List<Vector3>();

    int index;

    public void Reset() {
        index = 0;
    }
    
    public bool HasNext() {
        return index < vertices.Count;
    }

    public Vector3 Next() {
        if (index < vertices.Count)
            return vertices[index++];
        throw new System.IndexOutOfRangeException("No more vertices in this list.");
    }
}
