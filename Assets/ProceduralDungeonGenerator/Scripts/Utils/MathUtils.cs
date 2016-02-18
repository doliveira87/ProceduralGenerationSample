using UnityEngine;
using System.Collections;

public class MathUtils
{
	public static Vector2 Vec2ToInt(Vector2 v)
    {
        v.x = (int)v.x;
        v.y = (int)v.y;
        return v;
    }

    public static Vector2 Vec2ToCeilInt(Vector2 v)
    {
        v.x = Mathf.CeilToInt(v.x);
        v.y = Mathf.CeilToInt(v.y);
        return v;
    }

    public static Vector2 Vec3ToVec2(Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector3 Vec2ToVec3(Vector2 v)
    {
        return new Vector3(v.x, v.y);
    }

    public static bool IsVec2Valid(Vector2 v)
    {
        return !(float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsNaN(v.x) || float.IsNaN(v.y));
    }

    public static bool LineIntersects(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        Vector2 a = p2 - p1;
        Vector2 b = p3 - p4;
        Vector2 c = p1 - p3;

        float alphaNumerator = b.y * c.x - b.x * c.y;
        float alphaDenominator = a.y * b.x - a.x * b.y;
        float betaNumerator = a.x * c.y - a.y * c.x;
        float betaDenominator = a.y * b.x - a.x * b.y;

        bool doIntersect = true;

        if (alphaDenominator == 0 || betaDenominator == 0)
        {
            doIntersect = false;
        }
        else
        {

            if (alphaDenominator > 0)
            {
                if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                {
                    doIntersect = false;

                }
            }
            else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
            {
                doIntersect = false;
            }

            if (doIntersect && betaDenominator > 0) {
                if (betaNumerator < 0 || betaNumerator > betaDenominator)
                {
                    doIntersect = false;
                }
            } else if (betaNumerator > 0 || betaNumerator < betaDenominator)
            {
                doIntersect = false;
            }
        }

        return doIntersect;
    }

    public static bool LineIntersectsCircle(Vector2 startPoint, Vector2 endPoint, Vector2 circleCenter, float circleRadius)
    {
        Vector2 d = endPoint - startPoint;
        Vector2 f = startPoint - circleCenter;

        float a = Vector2.Dot(d, d);
        float b = 2 * Vector2.Dot(f, d);
        float c = Vector2.Dot(f, f) - circleRadius * circleRadius;

        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            return false;
        }
        else
        {
            // ray didn't totally miss sphere,
            // so there is a solution to
            // the equation.

            discriminant = Mathf.Sqrt(discriminant);

            // either solution may be on or off the ray so need to test both
            // t1 is always the smaller value, because BOTH discriminant and
            // a are nonnegative.
            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            // 3x HIT cases:
            //          -o->             --|-->  |            |  --|->
            // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

            // 3x MISS cases:
            //       ->  o                     o ->              | -> |
            // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

            if (t1 >= 0 && t1 <= 1)
            {
                // t1 is the intersection, and it's closer than t2
                // (since t1 uses -b - discriminant)
                // Impale, Poke
                return true;
            }

            // here t1 didn't intersect so we are either started
            // inside the sphere or completely past it
            if (t2 >= 0 && t2 <= 1)
            {
                // ExitWound
                return true;
            }

            // no intn: FallShort, Past, CompletelyInside
            return false;
        }
    }

    public static float TriangleArea(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        return Mathf.Abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0f);
    }

    /* A function to check whether point P(x, y) lies inside the triangle formed 
       by A(x1, y1), B(x2, y2) and C(x3, y3) */
    public static bool IsPointInsideTriangle(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p)
    {
        var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
        var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

        if ((s < 0) != (t < 0))
            return false;

        var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;
        if (A < 0.0)
        {
            s = -s;
            t = -t;
            A = -A;
        }
        return s > 0 && t > 0 && (s + t) < A;
    }

    public static Vector3 GetNearestColliderPoint(PolygonCollider2D pc, Vector3 point)
    {
        float minDistanceSqr = Mathf.Infinity;
        Vector3 nearestColliderPoint = Vector3.zero;

        // Scan all collider points to find nearest
        foreach (Vector3 colliderPoint in pc.points)
        {
            // Convert to world point
            Vector3 colliderPointWorld = pc.transform.TransformPoint(colliderPoint);

            Vector3 diff = point - MathUtils.Vec2ToVec3((Vector2)colliderPointWorld);
            float distSqr = diff.sqrMagnitude;

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestColliderPoint = colliderPointWorld;
            }
        }

        return nearestColliderPoint;
    }

    public static Vector3 ClampVector3(Vector3 vector, Vector3 minValue, Vector3 maxValue)
    {
        vector.x = (vector.x > maxValue.x) ? maxValue.x : (vector.x < minValue.x) ? minValue.x : vector.x;
        vector.y = (vector.y > maxValue.y) ? maxValue.y : (vector.y < minValue.y) ? minValue.y : vector.y;
        vector.z = (vector.z > maxValue.z) ? maxValue.z : (vector.z < minValue.z) ? minValue.z : vector.z;
        return vector;
    }

    public static bool IsOnScreen(Vector3 position)
    {
        Vector3 viewportPosition = Camera.main.WorldToScreenPoint(position);

        return !(viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1);
    }

    public static float Vector2Angle(Vector2 fromVector, Vector2 toVector)
    {
        float ang = Vector2.Angle(fromVector, toVector);
        Vector3 cross = Vector3.Cross(fromVector, toVector);

        if (cross.z > 0)
            ang = 360 - ang;

        return ang;
    }
}
