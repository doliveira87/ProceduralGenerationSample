using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShapeUtils
{
    /**
     * Calculate centroid of 2D non crossing polygon, To accommodate that points
     * are correct using Gift wrapping algorithm(Finding Convex Hull)
     * 
     * @ref http://en.wikipedia.org/wiki/Centroid#Centroid_of_polygon
     * @param vertices
     * @return
     */
    public static Vector2 FindCentroid2D(List<Vector2> vertices)
    {
        if (vertices == null)
            return new Vector2(0, 0);
 
        List<Vector2> hull = null;
        if (vertices.Count < 2)
            hull = new List<Vector2>(vertices);
        else
            hull = FindConvexHull(vertices);
 
        // Now we can calculate the centroid of polygon using standard mean
        int len = hull.Count;
        float[] xy = new float[] { 0, 0 };

        for (int i = 0; i < len; ++i)
        {
            Vector2 p = hull[i];
            xy[0] += p.x;
            xy[1] += p.y;
        }
 
        int x = (int) (xy[0] / len);
        int y = (int) (xy[1] / len);

        return new Vector2(x, y);
    }

    private const int ZERO = 0;
 
 
/**
     * Find Convex hull of given points
     * 
     * @ref http://en.wikipedia.org/wiki/Gift_wrapping_algorithm
     * @param vertices
     * @return
     */
    private static List<Vector2> FindConvexHull(List<Vector2> vertices)
    {
        if (vertices == null)
            return new List<Vector2>();
 
        if (vertices.Count < 3)
            return vertices;
 
        List<Vector2> points = new List<Vector2>(vertices);
        List<Vector2> hull = new List<Vector2>();
        Vector2 pointOnHull = GetExtremePoint(points);
        Vector2 endpoint = Vector2.zero;
        do
        {
            hull.Add(pointOnHull);
            endpoint = points[0];
 
            foreach (Vector2 r in points)
            {
                // Distance is used to find the outermost point -
                int turn = FindTurn(pointOnHull, endpoint, r);
                if (endpoint.Equals(pointOnHull) || turn == -1 || turn == 0
                    && Dist(pointOnHull, r) > Dist(endpoint, pointOnHull))
                {
                    endpoint = r;
                }
            }
            pointOnHull = endpoint;
        } while (!endpoint.Equals(hull[0])); // we are back at the start
 
        return hull;
    }
 
    private static Vector2 GetExtremePoint(List<Vector2> points)
    {
        for (int i = 1; i < points.Count-1; ++i)
        {
            if (points[i].x > points[i - 1].x & points[i].x > points[i + 1].x)
            {
                return points[i];
            }

            if (points[i].y > points[i - 1].y & points[i].y > points[i + 1].y)
            {
                return points[i];
            }
        }
        return points[0];
    }

    private static float Dist(Vector2 p, Vector2 q)
    {
        float dx = (q.x - p.x);
        float dy = (q.y - p.y);
        return dx * dx + dy * dy;
    }
 
 
   /**
     * Returns -1, 0, 1 if p,q,r forms a right, straight, or left turn. 
     * 1 = left, -1 = right, 0 = none
     * 
     * @ref http://www-ma2.upc.es/geoc/mat1q1112/OrientationTests.pdf
     * @param p
     * @param q
     * @param r
     * @return 1 = left, -1 = right, 0 = none
     */
    private static int FindTurn(Vector2 p, Vector2 q, Vector2 r)
    {
        int x1 = (int)((q.x - p.x) * (r.y - p.y));
        int x2 = (int)((r.x - p.x) * (q.y - p.y));
        int anotherInteger = x1 - x2;
        return ZERO.CompareTo(anotherInteger);
    }

    public static List<Vector2> OptimizeVertices(List<Vector2> vertices, float directionThreshold)
    {
        vertices = RemoveOutOfDirectionVertices(vertices, directionThreshold);

        //Reordering the list before trying to optmize again to fix vertices looping
        List<Vector2> auxList = new List<Vector2>();
        for (int i = vertices.Count / 2; i < vertices.Count; ++i)
        {
            auxList.Add(vertices[i]);
        }
        for (int i = 0; i < vertices.Count / 2; ++i)
        {
            auxList.Add(vertices[i]);
        }
        vertices = auxList;
        vertices = RemoveOutOfDirectionVertices(vertices, directionThreshold);

        return vertices;
    }

    private static List<Vector2> RemoveOutOfDirectionVertices(List<Vector2> vertices, float directionThreshold)
    {
        Vector2 curDirection = (vertices[1] - vertices[0]).normalized;
        Vector2 direction = Vector2.zero;

        for (int i = 1; i < vertices.Count - 1; ++i)
        {
            direction = (vertices[i + 1] - vertices[i]).normalized;

            if (Mathf.Abs(direction.x - curDirection.x) < directionThreshold
                && Mathf.Abs(direction.y - curDirection.y) < directionThreshold)
            {
                vertices.RemoveAt(i--);
            }
            else
            {
                curDirection = direction;
            }
        }
        return vertices;
    }


    public static Vector2 RandomPointInsideRect(Rect area, float sumToX = 0, float sumToY = 0)
    {
        Vector2 result = new Vector2();
        area.size = new Vector2(area.size.x + sumToX, area.size.y + sumToY);
        area.position = new Vector2(area.position.x + sumToX / 2, area.position.y - sumToY / 2);
        result.x = Random.Range(area.xMin, area.xMax);
        result.y = Random.Range(area.yMax, area.yMin);

        return result;
    }

    public static Vector2 GetRandomPointInElipse(int width, int height)
    {
        float t = 2 * Mathf.PI * Random.Range(0f, 1f);
        float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
        float r = 0;

        if (u > 1)
        {
            r = 2 - u;
        }
        else
        {
            r = u;
        }
        return new Vector2(Mathf.CeilToInt(width * r * Mathf.Cos(t)), Mathf.CeilToInt(height * r * Mathf.Sin(t)));
    }
}
