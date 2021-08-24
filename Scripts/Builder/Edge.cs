using System;
using System.Collections.Generic;
using UnityEngine;

public class Edge : IEquatable<Edge>
{
    public Vector3 a;
    public Vector3 b;

    public Edge(Vector3 a, Vector3 b) {
        this.a = a;
        this.b = b;
    }

    public Edge Flipped() {
        return new Edge(b, a);
    }

    public bool OppositeDirection(Edge other) {
        float d = Vector3.Dot(b-a, other.b-other.a);
        return d < 0;
    }
    
    public bool Equals(Edge other)
    {
        return (a-other.a).sqrMagnitude < 1e-3f && (b-other.b).sqrMagnitude < 1e-3f;
    }

    public override int GetHashCode()
    {
        return GetHashCode(this);
    }

    public int GetHashCode(Edge obj)
    {
        return obj.a.GetHashCode() + 31 * obj.b.GetHashCode();
    }

    public override string ToString()
    {
        return string.Format("E(a={0},b={1})", a, b);
    }
}
