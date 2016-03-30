using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LevelManager_States
{
	DetermineNextParticipantTurn = 0,
	TakeParticipantAction = 1,
	MoveParticipants = 2,
	GetPlayerInput = 3
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
		if (newParticipants == null)
		{
			newParticipants = new Queue<Participant>();
		}

		Controllable controllable = participant as Controllable;
		if (controllable != null && controllable == GameManager.Instance.Leader)
		{
			if (controlledLeader == null)
			{
				controlledLeader = controllable;
			}
			else
			{
				ChangeLeader( controllable );
			}
		}
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
		if ( floorParticipants == null )
			floorParticipants = new List<Participant>();

		while ( newParticipants.Count > 0 )
		{
			Participant participant = newParticipants.Dequeue();

			//int index = 0;

			//for ( int i = 0; i < floorParticipants.Count; i++ )
			//{
			//	if ( participant.Speed > floorParticipants[i].Speed )
			//	{
			//		index = i;
			//		break;
			//	}
			//}

			//floorParticipants.Insert( index, participant );

			floorParticipants.Add( participant );
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

	/// <summary>
	/// This is for looking at data about the next participant in the list,
	/// but not actually applying it's turn.
	/// </summary>
	/// <returns>The current participant in the list.</returns>
	public Participant PeekCurrentParticipant()
	{
		return floorParticipants[currentParticipantIndex];
	}

	/// <summary>
	/// Use this when analyzing a participant for a turn.
	/// This increments the counter that keeps track of which participant's turn is next.
	/// </summary>
	/// <returns>The next participant in the list.</returns>
	public Participant ActOnCurrentParticipant()
	{
		Participant p = floorParticipants[currentParticipantIndex];
		IncrementCurrentParticipantIndex();
		return p;
	}

	public void FinishedMovingParticipants()
	{
		participantsToMove = new List<Participant>();
	}

	#endregion

	#region State Shared Variables
	public List<Participant> ParticipantsToMove { get { return participantsToMove; } }

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

	/// <summary>
	/// A list of participants that are waiting to be moved
	/// </summary>
	private List<Participant> participantsToMove;

	private Queue<Participant> newParticipants;

	private Queue<Participant> deadParticipants;

	private int currentParticipantIndex;

	#endregion

	#region Mono Methods

	private void Awake()
	{
		Instance = this;
	}

	#endregion

	#region FSM Overrides

	protected override void Initialize()
	{
		// Initialize Lists
		floorParticipants = new List<Participant>();
		participantsToMove = new List<Participant>();

		FloorGenerator floorGen = GetComponent<FloorGenerator>();

		int numColumns = 0, numRows = 0;
		floorCells = floorGen.GenerateFloor( ref numColumns, ref numRows);
		NumColumns = numColumns;
		NumRows = numRows;		

		currentParticipantIndex = 0;

		RegisterNewParticipants();
	}

	#endregion

	#region Private Methods

	private void IncrementCurrentParticipantIndex()
	{
		if ( currentParticipantIndex < floorParticipants.Count - 1 )
			currentParticipantIndex++;
		else
			currentParticipantIndex = 0;
	}

	private void DecrementCurrentParticipantIndex()
	{
		if ( currentParticipantIndex == 0 )
			currentParticipantIndex = floorParticipants.Count;
		else
			currentParticipantIndex--;
	}

	#endregion
}
