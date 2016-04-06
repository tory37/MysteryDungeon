using UnityEngine;
using System.Collections;

public abstract class Participant : MonoBehaviour
{
	public delegate void FinishedActionCallback();

	public enum TurnType
	{
		Move,
		Action
	}

    #region Editor Interface

    [SerializeField] private int speed;

	[SerializeField] private Animator anim;

    #endregion

    #region Public Interface

	public Animator Anim { get { return anim; } }

	// Stats
	public int Speed { get { return speed; } }

	// Position
	public int Column { get; protected set; }
	public int Row { get; protected set; }
	public int OldColumn { get; protected set; }
	public int OldRow { get; protected set; }

	/// <summary>
	/// Whether or not this participant is in the scene.
	/// </summary>
	public bool Alive { get; private set; }

	public abstract TurnType DetermineTurn();

	/// <summary>
	/// This function should return the new cell that this participant will move to 
	/// on a normal move.
	/// </summary>
	/// <returns></returns>
	public abstract Cell FindNewCell();

	public abstract void TakeAction( FinishedActionCallback callback );

	/// <summary>
	/// This functions sets the participants position based on the passed cell
	/// </summary>
	/// <param name="newCell"></param>
	public virtual void SetNewPosition(Cell newCell)
	{
		this.OldColumn = this.Column;
		this.OldRow = this.Row;
		LevelManager.Instance.FloorCells[this.OldColumn, this.OldRow].Status = CellOccupancy.Open;
		this.Column = newCell.column;
		this.Row = newCell.row;
		LevelManager.Instance.FloorCells[this.Column, this.Row].Status = CellOccupancy.Enemy;
	}

	public void SetInitialPosition(int column, int row)
	{
		this.Column = column;
		this.Row = row;
	}

    #endregion

	#region FSM Overrides


	#endregion
}
