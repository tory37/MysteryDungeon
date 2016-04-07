﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

	public int FloorColumns { get; private set; }
	public int FloorRows { get; private set; }
	public Cell[,] FloorCells { get { return floorCells; } }
	public int CurrentParticipantIndex { get { return currentParticipantIndex; } }
	public Controllable ControlledLeader { get { return controlledLeader; } }
	public ReadOnlyCollection<Participant> FloorParticipants { get { return floorParticipants.AsReadOnly(); } }
	public Node[,] FloorNodes { get; private set; }
	public ReadOnlyCollection<Controllable> Players { get { return players.AsReadOnly(); } }
	public ReadOnlyCollection<Participant> ParticipantsToMove { get { return participantsToMove.AsReadOnly(); } }

	public void ChangeLeader(Controllable controllable)
	{
		if ( controlledLeader == null )
			controlledLeader = controllable;
		else
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
	}

	public void ChangeLeaderLeft()
	{
		Controllable oldLeader = controlledLeader;
		Controllable newLeader = null;

		Participant currentP = oldLeader;
		

		while (newLeader == null)
		{
			if ( floorParticipants.IndexOf( currentP ) > 0 )
				currentP = floorParticipants[floorParticipants.IndexOf( currentP ) - 1];
			else
				currentP = floorParticipants[floorParticipants.Count - 1];

			if ( currentP is Controllable )
				newLeader = currentP as Controllable;
		}

		int oldLeaderindex = floorParticipants.IndexOf( oldLeader );
		int newLeaderIndex = floorParticipants.IndexOf( newLeader );

		// Switch their order in the list
		floorParticipants[oldLeaderindex] = newLeader;
		floorParticipants[newLeaderIndex] = oldLeader;

		controlledLeader = newLeader;
	}

	public void ChangeLeaderRight()
	{
		Controllable oldLeader = controlledLeader;
		Controllable newLeader = null;

		Participant currentP = oldLeader;


		while ( newLeader == null )
		{
			if ( floorParticipants.IndexOf( currentP ) < floorParticipants.Count - 1 )
				currentP = floorParticipants[floorParticipants.IndexOf( currentP ) + 1];
			else
				currentP = floorParticipants[0];

			if ( currentP is Controllable )
				newLeader = currentP as Controllable;
		}

		int oldLeaderindex = floorParticipants.IndexOf( oldLeader );
		int newLeaderIndex = floorParticipants.IndexOf( newLeader );

		// Switch their order in the list
		floorParticipants[oldLeaderindex] = newLeader;
		floorParticipants[newLeaderIndex] = oldLeader;

		controlledLeader = newLeader;
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

		if ( players == null )
			players = new List<Controllable>();
		players.Add( controllable );
	}

	/// <summary>
	/// Removes a participant from the turn queue
	/// </summary>
	/// <param name="participant"></param>
	public void UnregisterParticipant(Participant participant)
	{
		deadParticipants.Enqueue( participant );
		Controllable c = participant as Controllable;
		if ( c != null && Players.Contains( c ) )
			players.Remove( c );
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

	public void AddParticipantToMove(Participant p)
	{
		if ( participantsToMove.Contains( p ) )
			return;
		else
			participantsToMove.Add( p );
	}

	public void FinishedMovingParticipants()
	{
		participantsToMove = new List<Participant>();
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
	private Cell[,] floorCells;

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

	private List<Controllable> players;

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
		FloorColumns = numColumns;
		FloorRows = numRows;

		FloorNodes = new Node[FloorColumns, FloorRows];
		for ( int i = 0; i < FloorColumns; i++)
		{
			for (int j = 0; j < numRows; j++)
			{
				FloorNodes[i, j] = new Node( floorCells[i, j], null, 0, 0 );
			}
		}

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
