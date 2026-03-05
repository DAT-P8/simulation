using System;
using System.Collections.Generic;

namespace Simulation.Utils;

public class Vector3D<T>(T x, T y, T z) : IEquatable<Vector3D<T>>
{
    private T _x = x;
    private T _y = y;
    private T _z = z;

    public T X => _x;
    public T Y => _y;
    public T Z => _z;

    public override bool Equals(object? obj)
        => Equals(obj as Vector3D<T>);

    public bool Equals(Vector3D<float>? other)
    {
        if (other is null)
            return false;

        const float epsilon = 1e-6f;

        // this doesn't pass typecheck:
        return MathF.Abs(X - other.X) < epsilon &&
               MathF.Abs(Y - other.Y) < epsilon &&
               MathF.Abs(Z - other.Z) < epsilon;
    }

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
}

public static class VectorExtensions
{
    public static Vector3D<float> Normalize(this Vector3D<float> v)
    {
        var magnitude = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return new Vector3D<float>((float)(v.X / magnitude), (float)(v.Y / magnitude), (float)(v.Z / magnitude));
    }
    
    public static Vector3D<double> Normalize(this Vector3D<double> v)
    {
        var magnitude = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return new Vector3D<double>(v.X / magnitude, v.Y / magnitude, v.Z / magnitude);
    }

    public static Vector3D<float> AsFloat(this Vector3D<double> v) => new((float)v.X, (float)v.Y, (float)v.Z);
}
