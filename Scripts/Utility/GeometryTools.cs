using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class GeometryTools {
        public static float Epsilon = 1e-3f;
        public static float EpsilonSquared = Epsilon * Epsilon;

        public static bool RayHitTriangle(Vector3 origin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 intersection, out float rayHitLength) {
            Vector3 edge1 = v1-v0;
            Vector3 edge2 = v2-v0;
            Vector3 normal = Vector3.Cross(edge1, edge2);
            intersection = Vector3.zero;
            rayHitLength = 0;
            if (Vector3.Dot(normal, origin-v0) < 0) {
                // origin is on the backside
                return false;
            }
            Vector3 h = Vector3.Cross(direction, edge2);
            float aa = Vector3.Dot(edge1, h);
            if (aa > -1e-3 && aa < 1e-3) {
                // ray is parallel to triangle
                return false;
            }
            float ff = 1f/aa;
            Vector3 s = origin - v0;
            float uu = ff * Vector3.Dot(s, h);
            if (uu < 0 || uu > 1)
                return false;
            Vector3 q = Vector3.Cross(s, edge1);
            float vv = ff * Vector3.Dot(direction, q);
            if (vv < 0 || uu+vv > 1)
                return false;
            float tt = ff * Vector3.Dot(edge2, q);
            if (tt > 1e-3f) {
                intersection = origin + direction * tt;
                rayHitLength = tt;
                return true;
            }
            return false;
        }

        public static bool EdgeEdgeIntersectIgnoreEnds(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersection) {
            float m;
            float n;
            if (LineLineIntersect(a, b, c, d, out intersection, out m, out n)) {
                if (m>0 && m<1 && n>=0 && n<=1) return true;
                if (m>=0 && m<=1 && n>0 && n<1) return true;
            }
            return false;
        }

        public static bool EdgeEdgeIntersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersection) {
            float m;
            float n;
            if (LineLineIntersect(a, b, c, d, out intersection, out m, out n)) {
                return m >= 0 && m <= 1 && n >= 0 && n <= 1;
            }
            return false;
        }

        public static bool EdgeLineIntersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersection) {
            float m;
            float n;
            if (LineLineIntersect(a, b, c, d, out intersection, out m, out n)) {
                return m >= 0 && m <= 1;
            }
            return false;
        }

        public static bool LineLineIntersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersection, out float m, out float n) {
            m = 0;
            n = 0;
            intersection = Vector3.zero;
            // direction from a to b
            Vector3 u = b-a;
            // direction from c to d
            Vector3 v = d-c;
            // an intersection must fulfil: a+m*u=c+n*v
            // we get three equations by splitting the x, y and z components and we can eliminate m
            // by reordering the equations so that we can set the one for x and y in one equation and solve
            // for n: n=(c.y*u.x-a.y*u.x-u.y*c.x+a.x*u.y) / (v.x*u.y - v.y*u.x)
            // m=(c.y+n*v.y-a.y) / u.y
            float divisorXY = v.x*u.y - v.y*u.x;
            float divisorXZ = v.x*u.z - v.z*u.x;
            float divisorYZ = v.y*u.z - v.z*u.y;
            if (divisorXY == 0 && divisorXZ == 0 && divisorYZ == 0) {
                // cannot devide by 0 => there is no solution
                //Debug.Log("no intersection in XY,XZ and YZ");
                return false;
            }
            if (divisorXY != 0) {
                n = (c.y*u.x-a.y*u.x-u.y*c.x+a.x*u.y) / divisorXY;
            } else if (divisorXZ != 0) {
                n = (c.z*u.x-a.z*u.x-u.z*c.x+a.x*u.z) / divisorXZ;
            } else {
                n = (c.z*u.y-a.z*u.y-u.z*c.y+a.y*u.z) / divisorYZ;
            }
            if (u.y != 0) {
                m = (c.y+n*v.y-a.y) / u.y;
            } else if (u.x != 0) {
                m = (c.x+n*v.x-a.x) / u.x;
            } else {
                m = (c.z+n*v.z-a.z) / u.z;
            }
            // intersection point according to first line equation
            Vector3 h = a + m * u;
            // intersection point according to second line equation
            Vector3 i = c + n * v;
            if ((h-i).sqrMagnitude < 1e-3f) {
                // good enough. we have a valid solution in 3d
                intersection = a + m*u;
                return true;
            }
            //Debug.Log("no intersection but found h=" + h + " and i=" + i);
            return false;
        }

        public static bool PointOnEdge(Vector3 p, Vector3 a, Vector3 b, out float m) {
            if (PointOnLine(p, a, b, out m)) {
                if (m >= 0 && m <= 1) {
                    return true;
                }
            }
            return false;
        }
        
        public static bool PointOnLine(Vector3 p, Vector3 a, Vector3 b, out float m) {
            Vector3 u = b-a;
            m = 0;
            if (u.x != 0) {
                m = (p.x-a.x)/u.x;
            } else if (u.y != 0) {
                m = (p.y-a.y)/u.y;
            } else if (u.z != 0) {
                m = (p.z-a.z)/u.z;
            }
            Vector3 iv = a+m*u;
            if ((iv-p).sqrMagnitude < EpsilonSquared) {
                return true;
            }
            return false;
        }
    }
}