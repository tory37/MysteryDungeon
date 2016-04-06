using UnityEngine;
using System.Collections;
using System;

public class LM_GetPlayerInput : State
{

	#region Private Interface

	LevelManager fsm;

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

		Participant p = fsm.PeekCurrentParticipant();
		if ( p != fsm.ControlledLeader )
		{
			return;
		}
	}

	public override void OnUpdate()
	{
		float vertical = Input.GetAxisRaw( "Vertical" );
		float horizontal = Input.GetAxisRaw( "Horizontal" );

		if ( vertical > 0 || horizontal > 0  || vertical < 0 || horizontal < 0)
		{
			int newX = 0, newZ = 0;
			if (fsm.ControlledLeader.CanMove(vertical, horizontal, ref newX, ref newZ))
			{
				fsm.ControlledLeader.SetNewPosition( new Cell( newX, newZ ) );
				fsm.ParticipantsToMove.Add( fsm.ControlledLeader );
				gotInput = true;
				nextState = LevelManager_States.DetermineNextParticipantTurn;
				return;
			}
			else
			{
				fsm.ControlledLeader.transform.LookAt(new Vector3( fsm.ControlledLeader.Column + horizontal, fsm.ControlledLeader.transform.position.y, fsm.ControlledLeader.Row + vertical ));
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
