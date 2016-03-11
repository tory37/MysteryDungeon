using UnityEngine;
using System.Collections;
using System;

public class LM_GetPlayerInput : State
{

	#region Private Interface

	LevelManager fsm;

	Controllable currentControllable;

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

		if ( vertical > .5f || horizontal > .5f )
		{
			int newX = 0, newZ = 0;
			if (currentControllable.CanMove(vertical, horizontal, ref newX, ref newZ))
			{
				currentControllable.SetNewPosition( new Cell( newX, newZ ) );
			}
		}
	}

	public override void CheckTransitions()
	{
		if (currentControllable == null)
			fsm.AttemptTransition( LevelManager_States.DetermineNextParticipantTurn );
	}

	#endregion

}
