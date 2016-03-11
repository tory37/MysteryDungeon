using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

	#region Editor Interface

	[SerializeField]
	private GameData gameData;

	#endregion

	#region Public Interface

	public List<Controllable> GetUserParty()
	{
		List<Controllable> party = new List<Controllable>();

		party.Add(gameData.Leader);

		return party;
	}

	#endregion
}
