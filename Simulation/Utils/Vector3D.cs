using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.Utils;

public class Vector3D<T>(T x, T y, T z)
{
    private T _x = x;
    private T _y = y;
    private T _z = z;

    public T X => _x;
    public T Y => _y;
    public T Z => _z;

    public void SetVector3D(T x, T y, T z)
    {
        _x = x;
        _y = y;
        _z = z;
    }

    public void SetVector3D(Vector3D<T> v)
    {
        _x = v.X;
        _y = v.Y;
        _z = v.Z;
    }

    public override string ToString()
    {
        return $"X: {_x}, Y: {_y}, Z: {_z}";
    }
}

public static class VectorExtensions
{
    /**
     * <summary>
     * Computes the dot product of two vectors.
     * </summary>
     */
    public static float Dot(this Vector3D<float> v1, Vector3D<float> v2)
    {
        return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
    }

    /**
     * <summary>
     * Computes the dot product of two vectors.
     * </summary>
     */
    public static double Dot(this Vector3D<double> v1, Vector3D<double> v2)
    {
        return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
    }

    /**
     * <summary>
     * Scales this vector by scalar.
     * </summary>
     */
    public static Vector3D<float> Scale(this Vector3D<float> v1, float scalar)
    {
        return new Vector3D<float>(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
    }

    /**
     * <summary>
     * Adds one vector to another, returning a new vector.
     * </summary>
     */
    public static Vector3D<float> Add(this Vector3D<float> v1, Vector3D<float> v2)
    {
        return new Vector3D<float>(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    }

    /**
     * <summary>
     * Adds one vector to another, returning a new vector.
     * </summary>
     */
    public static Vector3D<double> Add(this Vector3D<double> v1, Vector3D<double> v2)
    {
        return new Vector3D<double>(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    }

    /**
     * <summary>
     * Subtract a vector from this vector, returning a new vector.
     * </summary>
     */
    public static Vector3D<float> Sub(this Vector3D<float> v1, Vector3D<float> v2)
    {
        return new Vector3D<float>(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
    }

    /**
     * <summary>
     * Subtract a vector from this vector, returning a new vector.
     * </summary>
     */
    public static Vector3D<double> Sub(this Vector3D<double> v1, Vector3D<double> v2)
    {
        return new Vector3D<double>(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
    }

    /**
     * <summary>
     * Normalizes a vector to a vector of length 1.
     * </summary>
     */
    public static Vector3D<float> Normalize(this Vector3D<float> v)
    {
        var magnitude = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return new Vector3D<float>((float)(v.X / magnitude), (float)(v.Y / magnitude), (float)(v.Z / magnitude));
    }

    /**
     * <summary>
     * Normalizes a vector to a vector of length 1.
     * </summary>
     */
    public static Vector3D<double> Normalize(this Vector3D<double> v)
    {
        var magnitude = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return new Vector3D<double>(v.X / magnitude, v.Y / magnitude, v.Z / magnitude);
    }

    /**
     * <summary>
     * Converts a vector of type double to float by simple typecasting of the coordinates and construction of a new vector.
     * </summary>
     */
    public static Vector3D<float> AsFloat(this Vector3D<double> v) => new((float)v.X, (float)v.Y, (float)v.Z);

    /**
     * <summary>
     * Provides a comparison between vectors but with a small acceptable difference.
     * </summary>
     */
    public static bool EqualsWithEpsilon(this Vector3D<float> v1, Vector3D<float> v2)
    {
        float epsilon = 1e-6F;

        return EqualsWithEpsilon(v1, v2, epsilon);
    }

    /**
     * <summary>
     * Provides a comparison between vectors but with a small acceptable difference.
     * </summary>
     */
    public static bool EqualsWithEpsilon(this Vector3D<float> v1, Vector3D<float> v2, float epsilon)
    {
        return Math.Abs(v1.X - v2.X) < epsilon &&
            Math.Abs(v1.Y - v2.Y) < epsilon &&
            Math.Abs(v1.Z - v2.Z) < epsilon;
    }

    /**
     * <summary>
     * Provides a comparison between vectors but with a small acceptable difference.
     * </summary>
     */
    public static bool EqualsWithEpsilon(this Vector3D<double> v1, Vector3D<double> v2)
    {
        double epsilon = 1e-6F;

        return Math.Abs(v1.X - v2.X) < epsilon &&
            Math.Abs(v1.Y - v2.Y) < epsilon &&
            Math.Abs(v1.Z - v2.Z) < epsilon;
    }

    public static List<(Vector3D<float>, int, int)> SweepTests(List<Vector3D<float>> before, List<Vector3D<float>> after)
    {
        var orderedPairs = before.Zip(after).Select((e, i) =>
                e.First.X < e.Second.X ?
                    (e.First.X, e.Second.X) :
                    (e.Second.X, e.First.X)
            ).ToList();

        List<HashSet<int>> overlapIndeces = [];

        for (int i = 0; i < orderedPairs.Count; i++)
        {
            HashSet<int> set = [i];
            var p1 = orderedPairs[i];
            for (int j = i + 1; j < orderedPairs.Count; j++)
            {
                var p2 = orderedPairs[j];
                if (
                    (p1.Item1 <= p2.Item1 && p2.Item1 <= p1.Item2) ||
                    (p1.Item1 <= p2.Item2 && p2.Item2 <= p1.Item2) ||

                    (p2.Item1 <= p1.Item1 && p1.Item1 <= p2.Item2) ||
                    (p2.Item1 <= p1.Item2 && p1.Item2 <= p2.Item2)
                )
                {
                    set.Add(j);
                }
            }

            // Add only unique sets
            var wasFound = false;
            foreach (var s in overlapIndeces)
            {
                wasFound = wasFound || s.SetEquals(set);
                if (wasFound) break;
            }

            if (!wasFound)
                overlapIndeces.Add(set);
        }

        // Remove any subsets thay may be found.
        var overlaps = overlapIndeces.Where(s1 => overlapIndeces.All(s2 => !s1.IsProperSubsetOf(s2))).ToList();

        // Now that all overlaps in 1D have been found, we need to check if they actually did collide.
        // Start by construct the pairs to check
        List<(int, int)> overlapPairs = [];
        foreach (var overlap in overlaps)
        {
            var overlapList = overlap.ToList();
            for (int i = 0; i < overlapList.Count; i++)
            {
                for (int j = i + 1; j < overlapList.Count; j++)
                {
                    overlapPairs.Add((overlapList[i], overlapList[j]));
                }
            }
        }

        List<(Vector3D<float>, int, int)> points = [];
        foreach (var (i, j) in overlapPairs)
        {
            var v1b = before[i];
            var v1a = after[i];

            var v2b = before[j];
            var v2a = after[j];

            // Movement vectors
            var v1Mov = v1a.Sub(v1b);
            var v2Mov = v2a.Sub(v2b);
            var deltaMov = v1Mov.Sub(v2Mov);

            var P = new Vector3D<float>(0, 0, 0);
            var A = v1b.Sub(v2b);
            var B = A.Add(deltaMov);
            var point = ProjectPointOntoSegment(P, A, B);

            points.Add((point, i, j));
        }

        return points;
    }

    /*
     * <summary>
     * Project a point P onto a line segment AB
     * </summary>
     */
    private static Vector3D<float> ProjectPointOntoSegment(Vector3D<float> P, Vector3D<float> A, Vector3D<float> B)
    {
        Vector3D<float> d = B.Sub(A);
        float dDotD = d.Dot(d);

        if (dDotD == 0f)
            return A;

        float t = Math.Clamp(P.Sub(A).Dot(d) / dDotD, 0f, 1f);
        return A.Add(d.Scale(t));
    }
}
