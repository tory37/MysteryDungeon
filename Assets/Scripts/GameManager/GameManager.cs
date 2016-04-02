using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

	#region Editor Interface

	// DEBUG: 
	[SerializeField] private Controllable testLeader;

	#endregion

	#region Public Interface

	public static GameManager Instance 
	{ 
		get { return instance; }
		private set
		{
			if ( instance == null )
				instance = value;
			else
				Destroy( value.gameObject );

		}
	}

	public Controllable Leader { get { return leader; } }
	public List<Controllable> PartyMinusLeader { get { return partyMinusLeader; } }

	#endregion

	#region Private Fields

	private static GameManager instance;

	#endregion

	#region Game Data

	private Controllable leader;
	private List<Controllable> partyMinusLeader;

	#endregion

	#region Mono Methods

	private void Awake()
	{
		Instance = this;

		// DEBUG: Remove this, this is testing
		leader = testLeader;
	}

	#endregion
}
