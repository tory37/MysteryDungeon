using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoFSM
{
	public enum States
	{
		GetPlayerInput,

	}

	#region Editor Interface

	[SerializeField]
	private float characterMoveSpeed;

	[SerializeField, Tooltip( "The time between intervals, for pseudo character movement animation." )]
	private float characterMoveTime;

	// TESTING: Remove
	[SerializeField]
	Controllable currentChar;

	#endregion

	#region Public Interface

	public static LevelManager Instance
	{
		get { return instance; }
		set
		{
			if ( instance != null )
				Destroy( value.gameObject );
			else
				instance = value;
		}
	}

	/// <summary>
	/// This represents the current character being controlled.
	/// There will be public functions for switching characters, outside scripts should not be ablet o directly modify this.
	/// </summary>
	[HideInInspector]
	public Controllable CurrentControllable { get; private set; }

	public int NumColumns { get; private set; }
	public int NumRows { get; private set; }
	public CellStatus[,] FloorCells { get { return floorCells; } }

    /// <summary>
    /// The list of the participants on the floor
    /// </summary>
    public List<Participant> floorParticipants;

	#endregion

	#region FSM Overrides

	protected override void Initialize()
	{
		Instance = this;

		FloorGenerator floorGen = GetComponent<FloorGenerator>();

		int numColumns = 0, numRows = 0;
		floorCells = floorGen.GenerateFloor( ref numColumns, ref numRows );
		NumColumns = numColumns;
		NumRows = numRows;

		CurrentControllable = currentChar;
	}

	#endregion

	#region Private Fields

	/// <summary>
	/// The Singleton instance of this class
	/// </summary>
	private static LevelManager instance;

	/// <summary>
	/// The 2D Array representation of the floor
	/// </summary>
	private CellStatus[,] floorCells;

	#endregion
}
