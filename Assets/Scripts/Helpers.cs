using Unity;
using UnityEngine;


public static class Helpers
{
    public static Vector3 LocationNear(this Vector3 a, Vector3 b, float tollerance)
    {
        float left = b.x - tollerance;
        float right = b.x - tollerance;
        float top = b.x - tollerance;
        float bottom = b.x - tollerance;
        float front = b.x - tollerance;
        float back = b.x - tollerance;


    }
}