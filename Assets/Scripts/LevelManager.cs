using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{

	#region Public Interface

	public static LevelManager Instance
	{
		get { return instance; }
		set
		{
			if ( instance != null )
				Destroy( value.gameObject );
			else
				instance = value;
		}
	}

	#endregion

	#region Private Interface

	private static LevelManager instance;

	private CellStatus[,] floorCells;

	#endregion

	#region Mono Methods

	private void Start()
	{
		Instance = this;

		FloorGenerator floorGen = GetComponent<FloorGenerator>();

		floorCells = floorGen.GenerateFloor();
	}

	#endregion

}
