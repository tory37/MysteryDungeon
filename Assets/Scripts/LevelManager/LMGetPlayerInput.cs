using UnityEngine;
using System.Collections;
using System;

public class LMGetPlayerInput : State
{

	#region Private Interface

	LevelManager fsm;

	#endregion

	#region State Overrides

	public override void Initialize( MonoFSM callingfsm )
	{
		try	{
			fsm = (LevelManager)callingfsm;
		}
		catch (Exception e)	{
			Debug.LogError("State LMWaitForInput is being initialized by something other than a LevelManager.");
		}
	}

	public override void OnUpdate()
	{
		float vertical = Input.GetAxis( "Vertical" );
		float horizontal = Input.GetAxis( "Horizontal" );

		if ( vertical > .5f || horizontal > .5f )
		{

		}
	}

	#endregion

}
