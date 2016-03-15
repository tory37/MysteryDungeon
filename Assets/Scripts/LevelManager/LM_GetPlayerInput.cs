using UnityEngine;
using System.Collections;
using System;

public class LM_GetPlayerInput : State
{

	#region Private Interface

	LevelManager fsm;

	Controllable currentControllable;

	private bool gotInput;

	#endregion

	#region State Overrides

	public override void Initialize( MonoFSM callingfsm )
	{
		fsm = (LevelManager)callingfsm;
	}

	public override void OnEnter()
	{
		currentControllable = null;

		Participant p = fsm.PeekCurrentParticipant();
		if ( p != fsm.ControlledLeader )
		{
			return;
		}

		currentControllable = (Controllable)p;
	}

	public override void OnUpdate()
	{
		if ( currentControllable == null )
			return;

		float vertical = Input.GetAxis( "Vertical" );
		float horizontal = Input.GetAxis( "Horizontal" );

		if ( vertical >= .5f || horizontal >= .5f  || vertical <= -.5f || horizontal <= -.5f)
		{
			Debug.Log( "Vertical: " + vertical + ", Horizontal: " + horizontal );
			int newX = 0, newZ = 0;
			if (currentControllable.CanMove(vertical, horizontal, ref newX, ref newZ))
			{
				currentControllable.SetNewPosition( new Cell( newX, newZ ) );
				fsm.ParticipantsToMove.Add( currentControllable );
				gotInput = true;
			}
		}
	}

	public override void CheckTransitions()
	{
		if (gotInput == true)
			fsm.AttemptTransition( LevelManager_States.DetermineNextParticipantTurn );
	}

	#endregion

}
