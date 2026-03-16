using System;

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
}
