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

        int newC = Column;
        int newR = Row;

		if ( vertical > .5f )
            newR = newR + 1;
		else if ( vertical < -.5f )
            newR = newR - 1;

		if ( horizontal > .5f )
            newC = newC + 1;
		else if ( horizontal < -.5f )
            newC = newC - 1;

        if (LevelManager.Instance.FloorCells[newC, newR] == CellStatus.UnOccupied)
        {
            newColumn = newC;
            newRow = newR;

            return true;
        }
        else
           return false;
	}

	#endregion
}