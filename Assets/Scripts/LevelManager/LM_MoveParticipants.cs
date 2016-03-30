using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LM_MoveParticipants : State
{
	#region Editor Interface

	[SerializeField]
	private float moveIntervals;

	#endregion

	#region Private Fields

	private LevelManager fsm;

	private float moveIntervalTime;

	private float intervalFraction;

	// A list of the directions each character needs to move
	private List<Vector3> directions;

	private bool done;

	#endregion

	#region State Overrides

	public override void Initialize( MonoFSM callingfsm )
	{
		fsm = (LevelManager)callingfsm;

		// TODO: Make this use a Game Manager
		intervalFraction = 1 / moveIntervals;
	}

	public override void OnEnter()
	{
		done = false;

		//foreach ( Participant p in LevelManager.Instance.ParticipantsToMove )
		//	Debug.Log( p.transform.position + ", " + p.Column + ", " + p.Row );

		directions = new List<Vector3>();
		// Set the directions, we assume the Column and Row of the participant has been set to the target location
		for ( int i = 0; i < fsm.ParticipantsToMove.Count; i++ )
		{
			Participant p = fsm.ParticipantsToMove[i];
			directions.Add( (new Vector3( p.Column, p.transform.position.y, p.Row ) - p.transform.position).normalized );
		}

		StartCoroutine( Move() );
	}

	//public override void OnUpdate()
	//{
	//	for (int i = 0; i < fsm.ParticipantsToMove.Count; i++)
	//	{
	//		Participant p = fsm.ParticipantsToMove[i];
	//		p.transform.Translate(directions[i] * moveSpeed * Time.deltaTime);
	//	}
	//	deltaMove += moveSpeed * Time.deltaTime;
	//}

	public override void CheckTransitions()
	{
		if (done)
		{
			// Signify everyones been moved
			fsm.FinishedMovingParticipants();

			fsm.AttemptTransition( LevelManager_States.DetermineNextParticipantTurn );
		}
	}

	#endregion

	private IEnumerator Move()
	{
		for (int i = 0; i < moveIntervals - 1; i++)
		{
			for (int par = 0; par < fsm.ParticipantsToMove.Count; par++)
			{
				Participant p = fsm.ParticipantsToMove[par];
				p.transform.Translate( directions[par] * intervalFraction );
			}

			yield return new WaitForFixedUpdate();
		}

		for ( int par = 0; par < fsm.ParticipantsToMove.Count; par++ )
		{
			Participant p = fsm.ParticipantsToMove[par];
			p.transform.position = new Vector3( p.Column, p.transform.position.y, p.Row );
		}

		done = true;
	}
}
