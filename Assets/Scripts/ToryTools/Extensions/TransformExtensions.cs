using UnityEngine;
using System.Diagnostics;

public static class TransformExtensions {

	public static void SetPositionX(this Transform t, float newX)
    {
        t.position = new Vector3(newX, t.position.y, t.position.z);
    }

    public static void SetPositionY(this Transform t, float newY)
    {
        t.position = new Vector3(t.position.x, newY, t.position.z);
    }

    public static void SetPositionZ(this Transform t, float newZ)
    {
        t.position = new Vector3(t.position.x, t.position.y, newZ);
    }

	//public static bool HasTag(this Transform t, TagSystem tag)
	//{
	//	if ( t.GetComponent<Tags>() == null )
	//	{
	//		#if UNITY_EDITOR
	//		//UnityEngine.Debug.LogWarning( t + " does not have a Tags Component, but something is trying to access it.", t );
	//		//StackTrace stacakTrace = new StackTrace();
	//		//UnityEngine.Debug.LogWarning( stacakTrace.GetFrame( 1 ).GetMethod().Name );
	//		#endif

	//		return false;
	//	}
	//	else
	//	{
	//		return ((t.GetComponent<Tags>().tags & tag) == tag);
	//	}
	//}

	//public static void AddTag(this Transform t, TagSystem tag)
	//{
	//	if ( t.GetComponent<Tags>() == null )
	//	{
	//		UnityEngine.Debug.LogWarning( t + " does not have a Tags Component, but something is trying to add tag: " + tag.ToString() + ". Adding Tag Component.", t );
	//		StackTrace stacakTrace = new StackTrace();
	//		UnityEngine.Debug.LogWarning( stacakTrace.GetFrame( 1 ).GetMethod().Name );

	//		t.gameObject.AddComponent<Tags>();
	//	}

	//	if (!t.HasTag(tag))
	//	{
	//		t.GetComponent<Tags>().tags |= tag;
	//	}
	//}

	//public static void RemoveTag(this Transform t, TagSystem tag)
	//{
	//	if ( t.GetComponent<Tags>() == null )
	//	{
	//		UnityEngine.Debug.LogWarning( t + " does not have a Tags Component, but something is trying to remove tag: " + tag.ToString() + ". Adding Tag Component.", t );
	//		StackTrace stacakTrace = new StackTrace();
	//		UnityEngine.Debug.LogWarning( stacakTrace.GetFrame( 1 ).GetMethod().Name );

	//		t.gameObject.AddComponent<Tags>();
	//	}

	//	if (t.HasTag(tag))
	//	{
	//		t.GetComponent<Tags>().tags &= ~tag;
	//	}
	//}
}
