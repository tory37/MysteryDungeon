using UnityEngine;
using System.Collections;

public abstract class Controllable : Participant
{
	#region Private Interface


	#endregion

	#region Public Interface


	public bool IsLeader
	{
		get;
		set;
	}

	public bool CanMove(float vertical, float horizontal, ref int newColumn, ref int newRow)
	{
		newColumn = Column;
		newRow = Row;

		if ( vertical > .5f )
			newColumn = newColumn + 1;
		else if ( vertical < -.5f )
			newColumn = newColumn - 1;

		if ( horizontal > .5f )
			newColumn = newColumn + 1;
		else if ( horizontal < -.5f )
			newColumn = newColumn - 1;

		return true;
	}

	#endregion
}