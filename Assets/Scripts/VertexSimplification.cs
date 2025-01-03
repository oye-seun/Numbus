using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class VertexSimplification
{
    [System.Serializable]
    public class SimplificationSettings
    {
        public float minPointDistance = 0.01f;    // Minimum distance between points
        public float angleThreshold = 15f;        // Minimum angle for corner detection
        public int smoothingPasses = 1;           // Number of smoothing passes
        public float smoothingFactor = 0.5f;      // Smoothing strength (0-1)
        public bool removeColinear = true;        // Remove points that form straight lines
        public float colinearTolerance = 0.01f;   // Tolerance for colinear detection
    }

    public static List<Vector2> SimplifyVertices(List<Vector2> vertices, SimplificationSettings settings)
    {
        if (vertices == null || vertices.Count < 3)
            return vertices;

        List<Vector2> result = new List<Vector2>(vertices);

        // Apply each simplification step
        result = RemoveClosePoints(result, settings.minPointDistance);
        
        if (settings.removeColinear)
            result = RemoveColinearPoints(result, settings.colinearTolerance);
        
        for (int i = 0; i < settings.smoothingPasses; i++)
            result = SmoothPoints(result, settings.smoothingFactor);
        
        result = RetainCornerPoints(result, settings.angleThreshold);

        return result;
    }

    // Remove points that are too close to each other
    private static List<Vector2> RemoveClosePoints(List<Vector2> vertices, float minDistance)
    {
        List<Vector2> result = new List<Vector2>();
        float sqrMinDistance = minDistance * minDistance;

        result.Add(vertices[0]); // Always keep first point

        for (int i = 1; i < vertices.Count; i++)
        {
            Vector2 current = vertices[i];
            Vector2 last = result[result.Count - 1];

            if (Vector2.SqrMagnitude(current - last) >= sqrMinDistance)
            {
                result.Add(current);
            }
        }

        return result;
    }

    // Remove points that form nearly straight lines
    private static List<Vector2> RemoveColinearPoints(List<Vector2> vertices, float tolerance)
    {
        if (vertices.Count < 3)
            return vertices;

        List<Vector2> result = new List<Vector2>();
        result.Add(vertices[0]);

        for (int i = 1; i < vertices.Count - 1; i++)
        {
            Vector2 prev = vertices[i - 1];
            Vector2 current = vertices[i];
            Vector2 next = vertices[i + 1];

            Vector2 dir1 = (current - prev).normalized;
            Vector2 dir2 = (next - current).normalized;

            // If the point is not colinear (forms a significant angle), keep it
            if (Mathf.Abs(Vector2.Dot(dir1, dir2)) < (1 - tolerance))
            {
                result.Add(current);
            }
        }

        result.Add(vertices[vertices.Count - 1]); // Always keep last point
        return result;
    }

    // Smooth points using Laplacian smoothing
    private static List<Vector2> SmoothPoints(List<Vector2> vertices, float factor)
    {
        List<Vector2> result = new List<Vector2>(vertices);
        
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            Vector2 prev = vertices[i - 1];
            Vector2 current = vertices[i];
            Vector2 next = vertices[i + 1];

            Vector2 smoothed = Vector2.Lerp(current, (prev + next) * 0.5f, factor);
            result[i] = smoothed;
        }

        return result;
    }

    // Retain only significant corner points
    private static List<Vector2> RetainCornerPoints(List<Vector2> vertices, float angleThreshold)
    {
        if (vertices.Count < 3)
            return vertices;

        List<Vector2> result = new List<Vector2>();
        result.Add(vertices[0]);

        for (int i = 1; i < vertices.Count - 1; i++)
        {
            Vector2 prev = vertices[i - 1];
            Vector2 current = vertices[i];
            Vector2 next = vertices[i + 1];

            float angle = Vector2.Angle(prev - current, next - current);

            if (angle < angleThreshold)
            {
                result.Add(current);
            }
        }

        result.Add(vertices[vertices.Count - 1]);
        return result;
    }

    // Douglas-Peucker algorithm for curve simplification
    public static List<Vector2> DouglasPeuckerSimplification(List<Vector2> vertices, float epsilon)
    {
        if (vertices.Count < 3)
            return vertices;

        List<bool> keepPoint = Enumerable.Repeat(false, vertices.Count).ToList();
        keepPoint[0] = keepPoint[vertices.Count - 1] = true;

        DouglasPeuckerRecursive(vertices, 0, vertices.Count - 1, epsilon, keepPoint);

        return vertices.Where((v, i) => keepPoint[i]).ToList();
    }

    private static void DouglasPeuckerRecursive(List<Vector2> vertices, int startIndex, int endIndex, float epsilon, List<bool> keepPoint)
    {
        if (endIndex <= startIndex + 1)
            return;

        float maxDistance = 0;
        int maxIndex = startIndex;

        Vector2 start = vertices[startIndex];
        Vector2 end = vertices[endIndex];

        // Find point with max distance from line
        for (int i = startIndex + 1; i < endIndex; i++)
        {
            float distance = PointLineDistance(vertices[i], start, end);

            if (distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = i;
            }
        }

        // If max distance is greater than epsilon, recursively simplify
        if (maxDistance > epsilon)
        {
            keepPoint[maxIndex] = true;
            DouglasPeuckerRecursive(vertices, startIndex, maxIndex, epsilon, keepPoint);
            DouglasPeuckerRecursive(vertices, maxIndex, endIndex, epsilon, keepPoint);
        }
    }

    private static float PointLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        float numerator = Mathf.Abs(
            (lineEnd.y - lineStart.y) * point.x -
            (lineEnd.x - lineStart.x) * point.y +
            lineEnd.x * lineStart.y -
            lineEnd.y * lineStart.x
        );

        float denominator = Vector2.Distance(lineStart, lineEnd);

        return numerator / denominator;
    }

    // // Example usage
    // public static void Example()
    // {
    //     // Create test vertices
    //     List<Vector2> vertices = new List<Vector2>();
    //     // ... add vertices ...

    //     // Create settings
    //     SimplificationSettings settings = new SimplificationSettings
    //     {
    //         minPointDistance = 0.01f,
    //         angleThreshold = 15f,
    //         smoothingPasses = 1,
    //         smoothingFactor = 0.5f,
    //         removeColinear = true,
    //         colinearTolerance = 0.01f
    //     };

    //     // Simplify vertices
    //     List<Vector2> simplified = SimplifyVertices(vertices, settings);

    //     // Or use Douglas-Peucker
    //     List<Vector2> dpSimplified = DouglasPeuckerSimplification(vertices, 0.01f);
    // }

    // // Debug visualization
    // public static void DebugDrawPoints(List<Vector2> points, Color color, float duration = 2f)
    // {
    //     for (int i = 0; i < points.Count - 1; i++)
    //     {
    //         Debug.DrawLine(
    //             new Vector3(points[i].x, points[i].y, 0),
    //             new Vector3(points[i + 1].x, points[i + 1].y, 0),
    //             color,
    //             duration
    //         );
    //     }
    // }
}
