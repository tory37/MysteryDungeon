using UnityEngine;
using System.Collections;

public class LMWaitForInput : State
{

	#region Private Interface

	LevelManager fsm;

	#endregion

	#region State Overrides

	public override void Initialize( MonoFSM callingfsm )
	{
		
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
