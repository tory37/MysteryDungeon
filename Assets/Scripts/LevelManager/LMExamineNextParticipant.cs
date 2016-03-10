using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LMExamineNextParticipant : State
{
	#region Private Fields

	private LevelManager fsm;

	#endregion

	#region State Overrides

	public override void Initialize( MonoFSM callingfsm )
	{
		fsm = (LevelManager)callingfsm;
	}

	public override void OnEnter()
	{
		Participant firstParticipant = fsm.FloorParticipants[fsm.CurrentParticipantIndex];

		if (firstParticipant.DetermineTurn() == Participant.TurnType.Action)
		{
			fsm.AttemptTransition( LevelManager_States.WaitForParticipantAction );
			return;
		}
	}

	public override void CheckTransitions()
	{
		
	}

	#endregion

}
