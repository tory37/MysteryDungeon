using UnityEngine;
using System.Collections;

public abstract class Participant : MonoFSM
{
	public delegate void FinishedActionCallback();

	public enum TurnType
	{
		Move,
		Action
	}

    #region Editor Interface

    [SerializeField] private int speed;

    #endregion

    #region Public Interface

	// Stats
	public int Speed { get { return speed; } }

	// Position
	public int Column { get; private set; }
	public int Row { get; private set; }

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
	public void SetNewPosition(Cell newCell)
	{
		this.Column = newCell.column;
		this.Row = newCell.row;
	}

    #endregion

	#region FSM Overrides

	private void Awake()
	{
		LevelManager.Instance.RegisterParticipant( this );
	}

	#endregion
}
