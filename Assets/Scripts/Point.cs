using UnityEngine;
using System;

public class Point
{
    public int x, z;

    public Point(int X = -1, int Z = -1)
    {
        x = X; z = Z;
    }

    public Point(Point p)
    {
        x = p.x; z = p.z;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Point other)
            return false;

        return x == other.x && z == other.z;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(x, z);
    }

    public override string ToString()
    {
        return $"({x}, {z})";
    }
}
