using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoFSM
{
	public enum States
	{
		WaitForInput,
		MoveParticipants
	}

	#region Editor Interface

	[SerializeField]
	private float characterMoveSpeed;

	[SerializeField, Tooltip( "The time between intervals, for pseudo character movement animation." )]
	private float characterMoveTime;

	// TESTING: Remove
	[SerializeField]
	Controllable currentChar;

	#endregion

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
	public Controllable CurrentControllable { get; private set; }

	public int NumColumns { get; private set; }
	public int NumRows { get; private set; }
	public CellStatus[,] FloorCells { get { return floorCells; } }

	#endregion

	#region FSM Overrides

	protected override void Initialize()
	{
		Instance = this;

		FloorGenerator floorGen = GetComponent<FloorGenerator>();

		int numColumns = 0, numRows = 0;
		floorCells = floorGen.GenerateFloor( ref numColumns, ref numRows );
		NumColumns = numColumns;
		NumRows = numRows;

		CurrentControllable = currentChar;
	}

	#endregion

	#region Private Fields

	/// <summary>
	/// The Singleton instance of this class
	/// </summary>
	private static LevelManager instance;

	/// <summary>
	/// The 2D Array representation of the floor
	/// </summary>
	private CellStatus[,] floorCells;

	#endregion

	#region Private Methods

	private IEnumerator MoveCharacters( List<Tuple<Participant, Cell>> participants )
	{
		bool finished = false;

		while ( finished == false )
		{
			for ( int i = 0; i < participants.Count; i++ )
			{
				Participant p = participants[i].Item1;
				Cell target = participants[i].Item2;
				float moveX = Mathf.MoveTowards( p.transform.position.x, target.column, characterMoveSpeed * Time.deltaTime );
				float moveZ = Mathf.MoveTowards( p.transform.position.z, target.row, characterMoveSpeed * Time.deltaTime );
				p.transform.position = p.transform.position + new Vector3( moveX, 0f, moveZ );
			}

			finished = true;

			for ( int i = 0; i < participants.Count; i++ )
			{
				if ( participants[i].Item1.transform.position != participants[i].Item2.ToVector3() )
				{
					finished = false;
					break;
				}
			}

			yield return new WaitForSeconds( characterMoveTime );
		}
	}

	#endregion

}
