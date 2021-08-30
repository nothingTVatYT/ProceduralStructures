using UnityEngine;
using System.Collections.Generic;

public class BezierSpline {

    private List<Vector3> points;
    private Vector3[] controlPoints1;
    private Vector3[] controlPoints2;
    private float[] estimatedSegmentLength;
    private float estimatedLength;

    public float EstimatedLength { get { return estimatedLength; } }

    public BezierSpline(List<Vector3> points) {
        this.points = points;
        estimatedSegmentLength = new float[points.Count-1];
        estimatedLength = 0;
        GetCurveControlPoints(points, out controlPoints1, out controlPoints2);
        for (int i = 0; i < points.Count-1; i++) {
            estimatedSegmentLength[i] = EstimateSegmentLength(i, 10);
            estimatedLength += estimatedSegmentLength[i];
        }
    }

    public Vector3 GetPoint(float t) {
        int n = points.Count-1;
        float s;
        float segmentT;
        int segment = GetSegment(t, out s, out segmentT);
        if (segment > points.Count-2) segment = points.Count-2;
        return GetInterpolatedPoint(points[segment], controlPoints1[segment], controlPoints2[segment], points[segment+1], segmentT);
    }

    private float EstimateSegmentLength(int segment, int steps) {
        float result = 0;
        Vector3 prev = points[segment];
        for (int i = 1; i <= steps; i++) {
            float t = i * 1f/steps;
            Vector3 v = GetInterpolatedPoint(points[segment], controlPoints1[segment], controlPoints2[segment], points[segment+1], t);
            result += (v-prev).magnitude;
            prev = v;
        }
        return result;
    }

    private int GetSegment(float t, out float relativeSegmentStart, out float relativeT) {
        relativeSegmentStart = 0;
        relativeT = 0;
        for (int i = 0; i < estimatedSegmentLength.Length; i++) {
            float relativeSegmentEnd = relativeSegmentStart + estimatedSegmentLength[i] / estimatedLength;
            if (t >= relativeSegmentStart && t <= relativeSegmentEnd) {
                relativeT = (t-relativeSegmentStart) / (relativeSegmentEnd - relativeSegmentStart);
                return i;
            }
            relativeSegmentStart = relativeSegmentEnd;
        }
        relativeT = 1;
        return estimatedSegmentLength.Length-1;
    }

    Vector3 GetInterpolatedPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        float u = 1f - t;
        return p0 * u * u * u + p1 * 3 * u * u * t + p2 * 3 * u * t * t + p3 * t * t *t;
    }

    // from https://www.codeproject.com/Articles/31859/Draw-a-Smooth-Curve-through-a-Set-of-2D-Points-wit
    /// <summary>
	/// Get open-ended Bezier Spline Control Points.
	/// </summary>
	/// <param name="knots">Input Knot Bezier spline points.</param>
	/// <param name="firstControlPoints">Output First Control points
	/// array of knots.Length - 1 length.</param>
	/// <param name="secondControlPoints">Output Second Control points
	/// array of knots.Length - 1 length.</param>
	/// <exception cref="ArgumentNullException"><paramref name="knots"/>
	/// parameter must be not null.</exception>
	/// <exception cref="ArgumentException"><paramref name="knots"/>
	/// array must contain at least two points.</exception>
	public static void GetCurveControlPoints(List<Vector3> knots,
		out Vector3[] firstControlPoints, out Vector3[] secondControlPoints)
	{
		int n = knots.Count - 1;
		if (n == 1)
		{ // Special case: Bezier curve should be a straight line.
			firstControlPoints = new Vector3[1];
			// 3P1 = 2P0 + P3
            firstControlPoints[0] = (knots[0] * 2 + knots[1]) / 3f;

			secondControlPoints = new Vector3[1];
			// P2 = 2P1 – P0
            secondControlPoints[0] = 2 * firstControlPoints[0] - knots[0];
			return;
		}

		// Calculate first Bezier control points
		// Right hand side vector
		float[] rhs = new float[n];

		// Set right hand side X values
		for (int i = 1; i < n - 1; ++i)
			rhs[i] = 4 * knots[i].x + 2 * knots[i + 1].x;
		rhs[0] = knots[0].x + 2 * knots[1].x;
		rhs[n - 1] = (8 * knots[n - 1].x + knots[n].x) / 2f;
		// Get first control points X-values
		float[] x = GetFirstControlPoints(rhs);

		// Set right hand side Y values
		for (int i = 1; i < n - 1; ++i)
			rhs[i] = 4 * knots[i].y + 2 * knots[i + 1].y;
		rhs[0] = knots[0].y + 2 * knots[1].y;
		rhs[n - 1] = (8 * knots[n - 1].y + knots[n].y) / 2.0f;
		// Get first control points Y-values
		float[] y = GetFirstControlPoints(rhs);

		// Set right hand side Z values
		for (int i = 1; i < n - 1; ++i)
			rhs[i] = 4 * knots[i].z + 2 * knots[i + 1].z;
		rhs[0] = knots[0].z + 2 * knots[1].z;
		rhs[n - 1] = (8 * knots[n - 1].z + knots[n].z) / 2.0f;
		// Get first control points Y-values
		float[] z = GetFirstControlPoints(rhs);

		// Fill output arrays.
		firstControlPoints = new Vector3[n];
		secondControlPoints = new Vector3[n];
		for (int i = 0; i < n; ++i)
		{
			// First control point
			firstControlPoints[i] = new Vector3(x[i], y[i], z[i]);
			// Second control point
			if (i < n - 1)
				secondControlPoints[i] = new Vector3(
                    2 * knots[i + 1].x - x[i + 1],
                    2 * knots[i + 1].y - y[i + 1],
                    2 * knots[i + 1].z - z[i + 1]);
			else
				secondControlPoints[i] = new Vector3(
                    (knots[n].x + x[n - 1]) / 2,
					(knots[n].y + y[n - 1]) / 2,
                    (knots[n].z + z[n - 1]) / 2);
		}
	}

	/// <summary>
	/// Solves a tridiagonal system for one of coordinates (x or y)
	/// of first Bezier control points.
	/// </summary>
	/// <param name="rhs">Right hand side vector.</param>
	/// <returns>Solution vector.</returns>
	private static float[] GetFirstControlPoints(float[] rhs)
	{
		int n = rhs.Length;
		float[] x = new float[n]; // Solution vector.
		float[] tmp = new float[n]; // Temp workspace.

		float b = 2f;
		x[0] = rhs[0] / b;
		for (int i = 1; i < n; i++) // Decomposition and forward substitution.
		{
			tmp[i] = 1 / b;
			b = (i < n - 1 ? 4f : 3.5f) - tmp[i];
			x[i] = (rhs[i] - x[i - 1]) / b;
		}
		for (int i = 1; i < n; i++)
			x[n - i - 1] -= tmp[n - i] * x[n - i]; // Backsubstitution.

		return x;
	}
}