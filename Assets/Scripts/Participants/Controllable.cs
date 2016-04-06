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
		Node[,] floorNodes = LevelManager.Instance.FloorNodes;

		newColumn = Column;
		newRow = Row;

        int newC = Column;
        int newR = Row;

		if ( vertical > 0 )
            newR = newR + 1;
		else if ( vertical < 0)
            newR = newR - 1;

		if ( horizontal > 0 )
            newC = newC + 1;
		else if ( horizontal < 0 )
            newC = newC - 1;

        if (newC >= 0 && newC < LevelManager.Instance.FloorColumns && newR >= 0 && newR < LevelManager.Instance.FloorRows 
			&& (LevelManager.Instance.FloorCells[newC, newR].Status == CellOccupancy.Open 
			|| LevelManager.Instance.FloorCells[newC, newR].Status == CellOccupancy.Player)
			&& !( newC == Column + 1 && newR == Row + 1 &&
					((floorNodes[Column + 1, Row].Cell.Status != CellOccupancy.Open && floorNodes[Column + 1, Row].Cell.Status != CellOccupancy.Passable && floorNodes[Column + 1, Row].Cell.Status != CellOccupancy.Player) ||
					(floorNodes[Column, Row + 1].Cell.Status != CellOccupancy.Open && floorNodes[Column, Row + 1].Cell.Status != CellOccupancy.Passable && floorNodes[Column, Row + 1].Cell.Status != CellOccupancy.Player)))
			&& !( newC == Column - 1 && newR == Row + 1 &&
					((floorNodes[Column - 1, Row].Cell.Status != CellOccupancy.Open && floorNodes[Column - 1, Row].Cell.Status != CellOccupancy.Passable && floorNodes[Column - 1, Row].Cell.Status != CellOccupancy.Player) ||
					(floorNodes[Column, Row + 1].Cell.Status != CellOccupancy.Open && floorNodes[Column, Row + 1].Cell.Status != CellOccupancy.Passable && floorNodes[Column, Row + 1].Cell.Status != CellOccupancy.Player)))
			&& !( newC == Column - 1 && newR == Row - 1 &&
					((floorNodes[Column - 1, Row].Cell.Status != CellOccupancy.Open && floorNodes[Column - 1, Row].Cell.Status != CellOccupancy.Passable && floorNodes[Column - 1, Row].Cell.Status != CellOccupancy.Player) ||
					(floorNodes[Column, Row - 1].Cell.Status != CellOccupancy.Open && floorNodes[Column, Row - 1].Cell.Status != CellOccupancy.Passable && floorNodes[Column, Row - 1].Cell.Status != CellOccupancy.Player)))
			&& !( newC == Column + 1 && newR == Row - 1 &&
					((floorNodes[Column + 1, Row].Cell.Status != CellOccupancy.Open && floorNodes[Column + 1, Row].Cell.Status != CellOccupancy.Passable && floorNodes[Column + 1, Row].Cell.Status != CellOccupancy.Player) ||
					(floorNodes[Column, Row - 1].Cell.Status != CellOccupancy.Open && floorNodes[Column, Row - 1].Cell.Status != CellOccupancy.Passable && floorNodes[Column, Row - 1].Cell.Status != CellOccupancy.Player))) )
        {
            newColumn = newC;
            newRow = newR;

            return true;
        }
        else
           return false;
	}

	public override void SetNewPosition( Cell newCell )
	{
		this.OldColumn = this.Column;
		this.OldRow = this.Row;
		if ((LevelManager.Instance.ControlledLeader == this) || (LevelManager.Instance.ControlledLeader.Column != this.Column || LevelManager.Instance.ControlledLeader.Row != this.Row))
			LevelManager.Instance.FloorCells[this.Column, this.Row].Status = CellOccupancy.Open;
		this.Column = newCell.column;
		this.Row = newCell.row;
		LevelManager.Instance.FloorCells[this.Column, this.Row].Status = CellOccupancy.Player;
	}

	#endregion
}