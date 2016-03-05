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

	/// <summary>
	/// This represents the current character being controlled.
	/// There will be public functions for switching characters, outside scripts should not be ablet o directly modify this.
	/// </summary>
	[HideInInspector]
	public IControllable CurrentControllable { get; private set; }

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
