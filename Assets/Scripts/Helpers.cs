using System.Drawing;
using UnityEngine;

public static class Helpers
{
    public static bool LocationBallPark(this Vector3 a, Vector3 b, float tollerance)
    {
        bool x = a.LocationNearX(b, tollerance);
        bool y = a.LocationNearY(b, tollerance);
        bool z = a.LocationNearZ(b, tollerance);

        return (x && y) || (x && z) || (y && z);
    }

    public static bool LocationNear(this Vector3 a, Vector3 b, float tollerance)
    {
       return LocationNearX(a, b, tollerance) && LocationNearY(a, b, tollerance) && LocationNearZ(a, b, tollerance);
    }

    public static bool isNear(this float a, float b, float tollerance)
    {
        float min = b - tollerance;
        float max = b + tollerance;

        return (a >= min && a <= max);
    }

    public static bool LocationNearX(this Vector3 a, Vector3 b, float tollerance)
    {
        return isNear(a.x, b.x, tollerance);
    }

    public static bool LocationNearY(this Vector3 a, Vector3 b, float tollerance)
    {
        return isNear(a.y, b.y, tollerance);
    }

    public static bool LocationNearZ(this Vector3 a, Vector3 b, float tollerance)
    {
        return isNear(a.z, b.z, tollerance);
    }

    //from https://forum.unity.com/threads/fix-inside-quaternion-fromtorotation-discontinuity.706514/
    static public Quaternion FromToRotation(this Vector3 dir1, Vector3 dir2)
    {
        float r = 1f + Vector3.Dot(dir1, dir2);
        Vector3 w;

        if (r < 1E-6f)
        {
            r = 0f;
            w = Mathf.Abs(dir1.x) > Mathf.Abs(dir1.z) ? new Vector3(-dir1.y, dir1.x, 0f) :
                                                       new Vector3(0f, -dir1.z, dir1.y);
        }
        else
        {
            w = Vector3.Cross(dir1, dir2);
        }

        return new Quaternion(w.x, w.y, w.z, r).normalized;
    }

    public static Point Add(this Point p, Point q)
    {
        return new Point(p.X + q.X, p.Y + q.Y);
    }

    public static Point Subtract(this Point p, Point q)
    {
        return new Point(p.X - q.X, p.Y - q.Y);
    }

    public static bool HasValue<T>(this T[,] values, T value)
    {
        foreach (T b in values)
        {
            if (b.Equals(value)) return true;
        }

        return false;
    }

    public static bool HasValue<T>(this T[,] values, T value, System.Func<T, T, bool> comperator)
    {
        foreach (T b in values)
        {
            if (comperator(b, value)) return true;
        }

        return false;
    }

    public static int CountValues<T>(this T[,] values, T value)
    {
        int ret = 0;

        foreach (T b in values)
        {
            if (b.Equals(value)) ret++;
        }

        return ret;
    }

    public static T Max<T>(this System.Collections.Generic.List<T> list, System.Func<T, T, T> maxFinder)
    {
        T maxThing = list[0];

        foreach (T b in list)
        {
            maxThing = maxFinder(b, maxThing);
        }

        return maxThing;
    }
}