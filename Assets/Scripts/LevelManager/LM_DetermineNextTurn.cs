﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LM_DetermineNextTurn : State
{
	#region Private Fields

	private LevelManager fsm;

	#endregion

	#region State Overrides

	LevelManager_States nextState = LevelManager_States.DetermineNextParticipantTurn;

	public override void Initialize( MonoFSM callingfsm )
	{
		fsm = (LevelManager)callingfsm;
	}

	public override void OnEnter()
	{
		// If there is already someone in the move queue, then we just want to get everyone else who needs to move
		if ( fsm.ParticipantsToMove.Count > 0 )
		{
			FindAllToMove();
		}
		else
		{
			Participant currentParticipant = fsm.PeekCurrentParticipant();

			if ( currentParticipant as Controllable != null && currentParticipant == GameManager.Instance.Leader)
			{
				nextState = LevelManager_States.GetPlayerInput;
				return;
			}

			Participant.TurnType turnType = currentParticipant.DetermineTurn();

			if ( turnType == Participant.TurnType.Action )
			{
				nextState = LevelManager_States.TakeParticipantAction;
				return;
			}
			else
			{
				FindAllToMove();
			}
		}

		nextState = LevelManager_States.MoveParticipants;
	}

	public override void CheckTransitions()
	{
		if ( nextState == LevelManager_States.DetermineNextParticipantTurn )
			Debug.LogError( "Examine Next Participant did not switch states correctly." );
		else
			fsm.AttemptTransition( nextState );
	}

	#endregion

	#region Private Methods

	private void FindAllToMove()
	{
		Participant currentParticipant = fsm.PeekCurrentParticipant();
		while ( currentParticipant != fsm.ControlledLeader && currentParticipant.DetermineTurn() == Participant.TurnType.Move )
		{
			currentParticipant = fsm.ActOnCurrentParticipant();
			currentParticipant.SetNewPosition( currentParticipant.FindNewCell() );
			fsm.ParticipantsToMove.Add( currentParticipant );

			currentParticipant = fsm.PeekCurrentParticipant();
		} 
	}

	#endregion

}