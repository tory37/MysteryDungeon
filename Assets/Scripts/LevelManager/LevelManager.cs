using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LevelManager_States
{
	ExamineNextParticipant,
	WaitForParticipantAction,
	MoveParticipants,
	GetPlayerInput
}

public class LevelManager : MonoFSM
{
	#region Editor Interface

	[SerializeField]
	private float characterMoveSpeed;

	[SerializeField, Tooltip( "The time between intervals, for pseudo character movement animation." )]
	private float characterMoveTime;

	// TEMP: Remove this guy, he's just for practice and testing
	[SerializeField]
	private Controllable tempLeader;

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

	public int NumColumns { get; private set; }
	public int NumRows { get; private set; }
	public CellStatus[,] FloorCells { get { return floorCells; } }
	public int CurrentParticipantIndex { get { return currentParticipantIndex; } }
	public Controllable ControlledLeader { get { return controlledLeader; } }
	public List<Participant> FloorParticipants { get { return floorParticipants; } }

	public void ChangeLeader(Controllable controllable)
	{
		// Get references to old and new leader
		Controllable oldLeader = controlledLeader;
		Controllable newLeader = controllable;

		// Get their indexes in the list
		int oldLeaderindex = floorParticipants.IndexOf( oldLeader );
		int newLeaderIndex = floorParticipants.IndexOf( newLeader );

		// Switch their order in the list
		floorParticipants[oldLeaderindex] = newLeader;
		floorParticipants[newLeaderIndex] = oldLeader;

		// Set the new leader
		controlledLeader = controllable;
	}

	/// <summary>
	/// Registers a participant to the turn queue
	/// </summary>
	/// <param name="participant"></param>
	public void RegisterParticipant(Participant participant)
	{
		newParticipants.Enqueue( participant );
	}

	/// <summary>
	/// Removes a participant from the turn queue
	/// </summary>
	/// <param name="participant"></param>
	public void UnregisterParticipant(Participant participant)
	{
		deadParticipants.Enqueue( participant );
	}

	public void RegisterNewParticipants()
	{
		while ( newParticipants.Count > 0 )
		{
			Participant participant = newParticipants.Dequeue();

			int index = 0;

			for ( int i = 0; i < floorParticipants.Count; i++ )
			{
				if ( participant.Speed > floorParticipants[i].Speed )
				{
					index = i;
					break;
				}
			}

			floorParticipants.Insert( index, participant );
		}
	}

	public void RemoveDeadParticipants()
	{
		while ( newParticipants.Count > 0 )
		{
			Participant participant = deadParticipants.Dequeue();

			floorParticipants.Remove( participant );
		}
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

	/// <summary>
	/// The current leader, the main controllable
	/// </summary>
	private Controllable controlledLeader;

	/// <summary>
	/// The list of the participants on the floor
	/// </summary>
	private List<Participant> floorParticipants;

	private Queue<Participant> newParticipants;

	private Queue<Participant> deadParticipants;

	private int currentParticipantIndex;

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

		floorParticipants = new List<Participant>();
		currentParticipantIndex = 0;
	}

	#endregion
}
