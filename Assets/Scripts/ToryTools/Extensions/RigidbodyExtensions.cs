using UnityEngine;

public static class RigidbodyExtensions {

	public static void SetVelocityX( this Rigidbody r, float newX )
	{
		r.velocity = new Vector3( newX, r.velocity.y, r.velocity.z );
	}

	public static void SetVelocityY( this Rigidbody r, float newY )
	{
		r.velocity = new Vector3( r.velocity.x, newY, r.velocity.z );
	}

	public static void SetVelocityZ( this Rigidbody r, float newZ )
	{
		r.velocity = new Vector3( r.velocity.x, r.velocity.y, newZ );
	}
}
