using UnityEngine;
using System.Collections;

public class FloorGenerator : MonoBehaviour
{

	#region Editor Interface

	[SerializeField] private int floorColumns;
	[SerializeField] private int floorRows;

	[SerializeField] private int minRoomColumns;
	[SerializeField] private int minRoomRows;

	[SerializeField] private int maxRoomColumns;
	[SerializeField] private int maxRoomRows;

	[SerializeField] private int minRooms;
	[SerializeField] private int maxRooms;

	[Tooltip("This represents how many times the a random starting point for a room is attempted to be found before incrementally looping through the spaces.  This is to avoid the possibility of 'never finding an open space.'")]
	[SerializeField] private int findRoomStartTries;

	[SerializeField] private GameObject wallTestObject;
	[SerializeField] private GameObject floorTestObject;

	#endregion

	#region Private Fields

	/// <summary>
	/// True:  Floor tile has a wall.  During generation, we remove these walls. 
	/// False: Floor tile is not occupied
	/// </summary>
	private bool[,] floor;

	#endregion

	#region Mono Methods

	private void Start()
	{
		floor = new bool[floorColumns, floorRows];

		ResetFloor();
		GenerateRooms();
		DrawFloor();
	}

	#endregion

	#region Private Methods

	private void ResetFloor()
	{
		for (int column = 0; column < floorColumns; column++)
		{
			for (int width = 0; width < floorRows; width++)
			{
				floor[column, width] = true;
			}
		}
	}

	private void GenerateRooms()
	{
		int numRooms = Random.Range( minRooms, maxRooms );

		for (int room = 0; room < numRooms; room++)
		{
			int startingColumn = Random.Range( 0, floorColumns - 1 );
			int startingRow = Random.Range( 0, floorRows - 1 );

			for ( int i = 0; i < findRoomStartTries; i++ )
			{
				if ( floor[startingColumn, startingRow] == true )
					break;

				startingColumn = Random.Range( 0, maxRoomColumns - 1 );
				startingRow = Random.Range( 0, maxRoomRows - 1 );
			}

			for ( int i = 0; i < floorColumns; i++ )
			{
				int column = (i + startingColumn) < floorColumns ? (i + startingColumn) : ((i + startingColumn) - floorColumns);
				for ( int j = 0; j < floorRows; j++ )
				{
					int row = (j + startingRow) < floorRows ? (j + startingRow) : ((j + startingRow) - floorRows);
					if (floor[column, row] == true)
					{
						startingColumn = column;
						startingRow = row;
						break;
					}
				}
				if ( floor[startingColumn, startingRow] == true )
					break;
			}

			GenerateRoom( startingColumn, startingRow );

		}
	}

	private void GenerateRoom(int startingColumn, int startingRow)
	{
		int roomColumns = Random.Range( minRoomColumns, maxRoomColumns );
		int roomRows = Random.Range( minRoomRows, maxRoomRows );

		for (int column = startingColumn; column < (startingColumn + roomColumns < floorColumns ? startingColumn + roomColumns : floorColumns); column++)
		{
			for ( int row = startingRow; row < (startingRow + roomRows < floorRows ? startingRow + roomRows : floorRows); row++ )
			{
				if ( floor[column, row] == false )
					break;
				else
					floor[column, row] = false;
			}
		}
	}

	private void DrawFloor()
	{
		for ( int column = 0; column < floorColumns; column++ )
		{
			for ( int row = 0; row < floorRows; row++ )
			{
				if ( floor[column, row] == true )
					GameObject.Instantiate( wallTestObject, new Vector3( column, .5f, row ), Quaternion.identity );
				else
					GameObject.Instantiate( floorTestObject, new Vector3( column, 0f, row ), Quaternion.identity );
			}
		}
	}

	#endregion

}
