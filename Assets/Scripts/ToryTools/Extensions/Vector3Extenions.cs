using UnityEngine;
using System.Collections;

public static class Vector3Extenions {

	public static void SetX(this Vector3 v, float newX)
    {
        v = new Vector3(newX, v.y, v.z);
    }

    public static void SetY(this Vector3 v, float newY)
    {
        v = new Vector3(v.x, newY, v.z);
    }

    public static void SetZ(this Vector3 v, float newZ)
    {
        v = new Vector3(v.x, v.y, newZ);
    }
}
