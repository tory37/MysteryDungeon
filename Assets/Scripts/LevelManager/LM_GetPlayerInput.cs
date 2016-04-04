using UnityEngine;
using System.Collections;
using System;

public class LM_GetPlayerInput : State
{

	#region Private Interface

	LevelManager fsm;

	Controllable currentControllable;

	private bool gotInput;

	private LevelManager_States nextState;

	#endregion

	#region State Overrides

	public override void Initialize( MonoFSM callingfsm )
	{
		fsm = (LevelManager)callingfsm;
	}

	public override void OnEnter()
	{
		gotInput = false;

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

		float vertical = Input.GetAxisRaw( "Vertical" );
		float horizontal = Input.GetAxisRaw( "Horizontal" );

		if ( vertical > 0 || horizontal > 0  || vertical < 0 || horizontal < 0)
		{
			int newX = 0, newZ = 0;
			if (currentControllable.CanMove(vertical, horizontal, ref newX, ref newZ))
			{
				currentControllable.SetNewPosition( new Cell( newX, newZ ) );
				fsm.ParticipantsToMove.Add( currentControllable );
				gotInput = true;
				nextState = LevelManager_States.DetermineNextParticipantTurn;
				return;
			}
		}
		if ( Input.GetKeyDown( KeyCode.K ) )
		{
			gotInput = true;
			nextState = LevelManager_States.TakeParticipantAction;
			return;
		}
		if ( Input.GetButtonDown( "RB" ) )
		{
			LevelManager.Instance.ChangeLeaderRight();
			return;
		}
		if ( Input.GetButtonDown( "LB" ) )
		{
			LevelManager.Instance.ChangeLeaderLeft();
			return;
		}

		//if ( gotInput == true )
		//	fsm.AttemptTransition( LevelManager_States.DetermineNextParticipantTurn );
	}

	public override void CheckTransitions()
	{
		if ( gotInput == true )
			fsm.AttemptTransition( nextState );
	}

	#endregion

}
