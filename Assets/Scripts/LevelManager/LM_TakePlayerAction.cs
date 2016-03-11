using UnityEngine;
using System.Collections;

public class LM_TakePlayerAction : State
{

	#region Private Fields

	LevelManager fsm;

	private bool actionFinished;

	#endregion

	#region State Overrides

	public override void Initialize( MonoFSM callingfsm )
	{
		fsm = (LevelManager)callingfsm;
		actionFinished = false;
	}

	public override void OnEnter()
	{
		Participant participant = fsm.ActOnCurrentParticipant();
		participant.TakeAction( FinishedAction );
	}

	public override void CheckTransitions()
	{
		if ( actionFinished )
			fsm.AttemptTransition( LevelManager_States.DetermineNextParticipantTurn );
	}

	#endregion

	#region Private Methods

	private void FinishedAction()
	{
		actionFinished = true;
	}

	#endregion

}
