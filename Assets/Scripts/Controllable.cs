using UnityEngine;
using System.Collections;

public class Controllable : Participant
{
	#region Private Interface

	

	#endregion

	#region Public Interface

	/// <summary>
	/// Keeps track of the current position of 
	/// </summary>
	private int X, Z;

	public bool CanMove(float vertical, float horizontal, ref int newX, ref int newZ)
	{
		newX = X; 
		newZ = Z;

		if ( vertical > .5f )
			newX = newX + 1;
		else if ( vertical < -.5f )
			newX = newX - 1;

		if ( horizontal > .5f )
			newX = newX + 1;
		else if ( horizontal < -.5f )
			newX = newX - 1;

		return true;
	}

	#endregion
}