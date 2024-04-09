using Unity;
using UnityEngine;


public static class Helpers
{
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
}