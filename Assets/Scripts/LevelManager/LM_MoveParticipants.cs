using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LM_MoveParticipants : State
{
	#region Editor Interface

	[SerializeField]
	private float eMoveSpeed;

	#endregion

	#region Private Fields

	private LevelManager fsm;

	/// <summary>
	/// The distance the characters have moved so far
	/// </summary>
	private float deltaMove;

	private float moveSpeed;

	// A list of the directions each character needs to move
	private List<Vector3> directions;

	#endregion

	#region State Overrides

	public override void Initialize( MonoFSM callingfsm )
	{
		fsm = (LevelManager)callingfsm;

		// TODO: Make this use a Game Manager
		moveSpeed = eMoveSpeed;
	}

	public override void OnEnter()
	{
		deltaMove = 0;

		directions = new List<Vector3>();
		// Set the directions, we assume the Column and Row of the participant has been set to the target location
		for ( int i = 0; i < fsm.ParticipantsToMove.Count; i++ )
		{
			Participant p = fsm.ParticipantsToMove[i];
			directions.Add( (new Vector3( p.Column, 0f, p.Row ) - p.transform.position).normalized );
		}
	}

	public override void OnUpdate()
	{
		for (int i = 0; i < fsm.ParticipantsToMove.Count; i++)
		{
			Participant p = fsm.ParticipantsToMove[i];
			p.transform.Translate(directions[i] * moveSpeed * Time.deltaTime);
		}
	}

	public override void CheckTransitions()
	{
		if (deltaMove >= 1)
		{
			// Lock them into place, accounting for any small error
			for (int i = 0; i < fsm.ParticipantsToMove.Count; i++)
			{
				Participant p = fsm.ParticipantsToMove[i];
				p.transform.position = new Vector3( p.Column, 0f, p.Row );
			}

			// Signify everyones been moved
			fsm.FinishedMovingParticipants();

			fsm.AttemptTransition( LevelManager_States.DetermineNextParticipantTurn );
		}
	}

	#endregion
}
