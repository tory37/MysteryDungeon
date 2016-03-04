using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ChunkStatus { Occupied, Unoccupied };
public enum TileStatus { HasWall, NoWall };

public struct Cell { public int column, row;}

public class Chunk
{
	public int X, Z;
    public int Partition;

	public class Room
	{
		public int columns, rows, startingColumn, startingRow;
	}

	public ChunkStatus Status;
	public Room MyRoom;

	public GameObject CenterMarker;

	public Chunk( int x, int z )
	{
		X = x;
		Z = z;
		Status = ChunkStatus.Unoccupied;
		MyRoom = null;
	}

	public void SetRoom( int columns, int rows, int startingColumn, int startingRow )
	{
		MyRoom = new Room();
		MyRoom.columns = columns;
		MyRoom.rows = rows;
		MyRoom.startingColumn = startingColumn;
		MyRoom.startingRow = startingRow;
	}
}

public class FloorGenerator : MonoBehaviour
{

	#region Editor Interface

	[Header( "Floor" )]
	[SerializeField]
	private int numColumnsInFloor;
	[SerializeField]
	private int numRowsInFloor;

	[Header( "Chunks" )]
	[SerializeField]
	private int numChunksX;
	[SerializeField]
	private int numChunksZ;

	[Header( "Rooms" )]
	[SerializeField]
	private int minRooms;
	[SerializeField]
	private int maxRooms;

	[Header( "Min Max Columns" )]
	[SerializeField]
	private int minRoomColumns;
	[SerializeField]
	private int maxRoomColumns;

	[Header( "Min Max Rows" )]
	[SerializeField]
	private int minRoomRows;
	[SerializeField]
	private int maxRoomRows;

	[Header( "Parent Transforms" )]
	[SerializeField]
	private Transform chunkObjectsParent;
	[SerializeField]
	private Transform connectorsParent;
	[SerializeField]
	private Transform wallsParent;
	[SerializeField]
	private Transform floorsParent;

	[Header( "Other" )]
	[Tooltip( "This represents how many times the a random starting point for a room is attempted to be found before incrementally looping through the spaces.  This is to avoid the possibility of 'never finding an open space.'" )]
	[SerializeField]
	private int findRoomStartTries;

	[SerializeField]
	private GameObject wallTestObject;
	[SerializeField]
	private GameObject floorTestObject;
	[SerializeField]
	private GameObject ConnectorRenderer;

	#endregion

	#region Private Fields

	/// <summary>
	/// Represents the tiles in the floor
	/// </summary>
	private TileStatus[,] tileInFloor;

	private int totalNumChunks;

	private int numberColumnsInChunk;
	private int numberRowsInChunk;

	private Chunk[,] chunks;

	private int partitionCount;

	#endregion

	#region Mono Methods

	private void Start()
	{
		totalNumChunks = numChunksX * numChunksZ;

		numberColumnsInChunk = Mathf.FloorToInt( numColumnsInFloor / numChunksX );
		numberRowsInChunk = Mathf.FloorToInt( numRowsInFloor / numChunksZ );

		ResetFloor();
		GenerateRooms();
		ConnectAllRooms();
		DrawFloor();
	}

	#endregion

	#region Private Methods

	private void ResetFloor()
	{
		tileInFloor = new TileStatus[numColumnsInFloor, numRowsInFloor];

		chunks = new Chunk[numChunksX, numChunksZ];

		partitionCount = 0;

		// Reset chunks
		for ( int x = 0; x < numChunksX; x++ )
		{
			for ( int z = 0; z < numChunksZ; z++ )
			{
				chunks[x, z] = new Chunk( x, z );
                chunks[x, z].Partition = partitionCount;
                partitionCount++;
				chunks[x, z].CenterMarker = new GameObject();
				chunks[x, z].CenterMarker.transform.position = new Vector3( (x * numberColumnsInChunk) + (.5f * numberColumnsInChunk), 2f, (z * numberRowsInChunk) + (.5f * numberRowsInChunk) );
				chunks[x, z].CenterMarker.transform.parent = chunkObjectsParent;
			}
		}

		// Reset Tiles
		for ( int column = 0; column < numColumnsInFloor; column++ )
		{
			for ( int width = 0; width < numRowsInFloor; width++ )
			{
				tileInFloor[column, width] = TileStatus.HasWall;
			}
		}
	}

	private void GenerateRooms()
	{
		maxRooms = maxRooms > totalNumChunks ? totalNumChunks : maxRooms;
		int numRooms = Random.Range( minRooms, maxRooms );

		for ( int room = 0; room < numRooms; room++ )
		{
			int chunkColumn = Random.Range( 0, numChunksX );
			int chunkRow = Random.Range( 0, numChunksZ );

			for ( int i = 0; i < findRoomStartTries; i++ )
			{
				if ( chunks[chunkColumn, chunkRow].Status == ChunkStatus.Unoccupied )
					break;
				else
				{
					chunkColumn = Random.Range( 0, numChunksX );
					chunkRow = Random.Range( 0, numChunksZ );
				}
			}

			if ( chunks[chunkColumn, chunkRow].Status == ChunkStatus.Occupied )
			{
				for ( int chunkX = 0; chunkX < numChunksX; chunkX++ )
				{
					for ( int chunkZ = 0; chunkZ < numRowsInFloor; chunkZ++ )
					{
						chunkColumn = chunkX;
						chunkRow = chunkZ;

						if ( chunks[chunkColumn, chunkRow].Status == ChunkStatus.Unoccupied )
							break;
					}
					if ( chunks[chunkColumn, chunkRow].Status == ChunkStatus.Unoccupied )
						break;
				}
			}

			GenerateRoom( chunkColumn, chunkRow );
		}
	}

	private void ConnectAllRooms()
	{
		// Randomly select a chunk for a number of tries, and find a connection for it
		for ( int i = 0; i < findRoomStartTries; i++ )
		{
			if ( CheckAllRoomsConnected() )
				break;

			Chunk startingChunk = FindRandomChunk();
			Chunk endingChunk = FindEndingChunk(startingChunk);

			if ( endingChunk != null )
				ConnectTwoChunks( startingChunk, endingChunk );
		}

		// Loop through all rooms to ensure all are in same partition if random finding is taking too long
		if (!CheckAllRoomsConnected())
		{
			for ( int x = 0; x < numChunksX; x++ )
			{
				for ( int z = 0; z < numChunksZ; z++ )
				{
                    Chunk startingChunk = chunks[x, z];
					Chunk endingChunk = FindEndingChunk( startingChunk );

					if ( endingChunk != null )
						ConnectTwoChunks( startingChunk, endingChunk );
				}
			}
		}
	}

	// Returns true if all rooms are in the same partiion
	private bool CheckAllRoomsConnected()
	{
		int zerothPartition = -1;  

		for ( int x = 0; x < numChunksX; x++ )
		{
			for ( int z = 0; z < numChunksZ; z++ )
			{
                if (chunks[x, z].MyRoom != null)
                {
                    zerothPartition = chunks[x, z].Partition;
                    break;
                }
			}
			if (zerothPartition != -1)
				break;
		}

		for ( int x = 0; x < numChunksX; x++ )
		{
			for ( int z = 0; z < numChunksZ; z++ )
			{
				if ( chunks[x, z].MyRoom != null && chunks[x, z].Partition != zerothPartition )
					return false;
			}
		}

		return true;
	}

	private void GenerateRoom( int chunkColumn, int chunkRow )
	{
		//Debug.Log( "Room In Chunk: " + chunkColumn + ", " + chunkRow );
		chunks[chunkColumn, chunkRow].Status = ChunkStatus.Occupied;

		int roomColumns = Random.Range( minRoomColumns, maxRoomColumns < numberColumnsInChunk ? maxRoomColumns : numberColumnsInChunk );
		int roomRows = Random.Range( minRoomRows, maxRoomRows < numberRowsInChunk ? maxRoomRows : numberRowsInChunk );
		//Debug.Log( "Columns: " + roomColumns );
		//Debug.Log( "Rows: " + roomRows );

		int leastColumn = chunkColumn * numberColumnsInChunk;
		int maxColumn = ((chunkColumn + 1) * numberColumnsInChunk) - roomColumns;
		int leastRow = chunkRow * numberRowsInChunk;
		int maxRow = ((chunkRow + 1) * numberRowsInChunk) - roomRows;

		int startingColumn = Random.Range( leastColumn, maxColumn );
		int startingRow = Random.Range( leastRow, maxRow );

		// Create the room, assign it the latest partition number, and then update the partition counter;
		chunks[chunkColumn, chunkRow].SetRoom( roomColumns, roomRows, startingColumn, startingRow );

		// DEBUG: Place center object here
		chunks[chunkColumn, chunkRow].CenterMarker.transform.position = new Vector3( startingColumn + (roomColumns / 2), 1f, startingRow + (roomRows / 2) );

		for ( int column = startingColumn; column < roomColumns + startingColumn; column++ )
		{
			for ( int row = startingRow; row < roomRows + startingRow; row++ )
			{
				try
				{
					if ( (column == startingColumn && column > 0 && tileInFloor[column - 1, row] == TileStatus.NoWall)
					   || (column == (roomColumns + startingColumn) - 1 && column < numColumnsInFloor - 1 && tileInFloor[column + 1, row] == TileStatus.NoWall)
					   || (row == startingRow && row > 0 && tileInFloor[column, row - 1] == TileStatus.NoWall)
					   || (row == (roomRows + startingRow) - 1 && row < numRowsInFloor - 1 && tileInFloor[column, row + 1] == TileStatus.NoWall) )
						Debug.Log( "Tile " + column + ", " + row + " did not get placed to maintain a wall on a side." );
					else if ( tileInFloor[column, row] == TileStatus.HasWall )
						tileInFloor[column, row] = TileStatus.NoWall;
					else
						Debug.LogError( "During room generation, we tried to remove wall: " + column + ", " + row + ".  It had already been removed." );
				}
				catch ( System.Exception e )
				{
					Debug.LogError( "Chunk: " + chunkColumn + ", " + chunkRow );
					Debug.LogError( "Column: " + column + ", Row: " + row + ", StartingRow: " + startingRow + ", MaxRow: " + maxRow );
				}
			}
		}
	}

	private void DrawFloor()
	{
		// DEBUG: Display the partitions
		for ( int x = 0; x < numChunksX; x++ )
		{
			for ( int z = 0; z < numChunksZ; z++ )
			{
				if (chunks[x,z].MyRoom != null)
					Debug.Log( "Chunk " + x + ", " + z + " in partition " + chunks[x, z].Partition );
			}
		}

		for ( int column = 0; column < numColumnsInFloor; column++ )
		{
			for ( int row = 0; row < numRowsInFloor; row++ )
			{
				if ( tileInFloor[column, row] == TileStatus.HasWall )
				{
					GameObject wall = GameObject.Instantiate( wallTestObject, new Vector3( column, .5f, row ), Quaternion.identity ) as GameObject;
					wall.transform.parent = wallsParent;
				}
				else
				{
					GameObject floor = GameObject.Instantiate( floorTestObject, new Vector3( column, 0f, row ), Quaternion.identity ) as GameObject;
					floor.transform.parent = floorsParent;
				}
			}
		}

		for ( int column = -1; column <= numColumnsInFloor; column++ )
		{
			GameObject wall = GameObject.Instantiate( wallTestObject, new Vector3( column, .5f, -1 ), Quaternion.identity ) as GameObject;
			wall.transform.parent = wallsParent;
			wall = GameObject.Instantiate( wallTestObject, new Vector3( column, .5f, numRowsInFloor ), Quaternion.identity ) as GameObject;
			wall.transform.parent = wallsParent;
		}

		for ( int row = -1; row <= numRowsInFloor; row++ )
		{
			GameObject wall = GameObject.Instantiate( wallTestObject, new Vector3( -1, .5f, row ), Quaternion.identity ) as GameObject;
			wall.transform.parent = wallsParent;
			wall = GameObject.Instantiate( wallTestObject, new Vector3( numColumnsInFloor, .5f, row ), Quaternion.identity ) as GameObject;
			wall.transform.parent = wallsParent; ;
		}
	}

	private Chunk FindRandomChunk()
	{
		Chunk startingChunk = null;

		int x = Random.Range( 0, numChunksX - 1 );
		int z = Random.Range( 0, numChunksZ - 1 );

		// Pick starting chunk with a room
		while ( startingChunk == null )
		{
			Chunk temp = chunks[x, z];
			if ( temp.MyRoom != null )
				startingChunk = temp;

			x = Random.Range( 0, numChunksX - 1 );
			z = Random.Range( 0, numChunksZ - 1 );
		}

		return startingChunk;
	}

	/// <summary>
	/// Finds a chunk for the passed in chunk to connect to that isn't in the same partition
	/// </summary>
	/// <param name="startingCell"></param>
	/// <param name="endingCell"></param>
	private Chunk FindEndingChunk( Chunk startingChunk )
	{
		int x = startingChunk.X;
		int z = startingChunk.Z;

		Chunk endingChunk = null;

		int modifier = 1;
        bool canRight, canDown, canLeft, canUp;
		canRight = canDown = canLeft = canUp = true;

		// Search the surrounding chunks for a room that can be patched with a hall that's not in the starting partition
		while ( x + modifier < numChunksX || x - modifier >= 0 || z + modifier < numChunksZ || z - modifier >= 0 )
		{
			// Check right
			if ( canRight && x + modifier < numChunksX)
			{
                //if (chunks[x + modifier, z].MyRoom != null)
                //    canRight = false;

                if (startingChunk.Partition != chunks[x + modifier, z].Partition)
                {
                    endingChunk = chunks[x + modifier, z];
                    break;
                }
                else
                    canRight = false;
			}
			// Check Down
			if ( canDown && z - modifier >= 0 )
			{
                //if (chunks[x, z - modifier].MyRoom != null)
                //    canDown = false;

                if (startingChunk.Partition != chunks[x, z - modifier].Partition)
                {
                    endingChunk = chunks[x, z - modifier];
                    break;
                }
                else
                    canDown = false;
			}
			// Check Left
			if ( canLeft && x - modifier >= 0 )
			{
                //if ( chunks[x - modifier, z].MyRoom != null )
                //    canLeft = false;

                if (startingChunk.Partition != chunks[x - modifier, z].Partition)
                {
                    endingChunk = chunks[x - modifier, z];
                    break;
                }
                else
                    canLeft = false;
			}
			// Check Up
			if ( canUp && z + modifier < numChunksZ )
			{
                //if (chunks[x, z + modifier].MyRoom != null)
                //    canUp = false;

                if (startingChunk.Partition != chunks[x, z + modifier].Partition)
                {
                    endingChunk = chunks[x, z + modifier];
                    break;
                }
                else
                    canUp = false;
			}

			modifier++;
		}

		return endingChunk;
	}

	private void ConnectTwoChunks(Chunk startingChunk, Chunk endingChunk)
	{
		int newPartition = startingChunk.Partition;
		int oldPartition = endingChunk.Partition;

		for ( int x = 0; x < numChunksX; x++ )
		{
			for ( int z = 0; z < numChunksZ; z++ )
			{
				if ( chunks[x, z].Partition == oldPartition )
					chunks[x, z].Partition = newPartition;
			}
		}

		LineRenderer newLine = (GameObject.Instantiate( ConnectorRenderer, Vector3.up * 2, Quaternion.identity ) as GameObject).GetComponent<LineRenderer>();
		newLine.transform.parent = connectorsParent;

		if ( newLine != null )
		{
			newLine.SetPosition( 0, startingChunk.CenterMarker.transform.position + Vector3.up * 2 );
			newLine.SetPosition( 1, endingChunk.CenterMarker.transform.position + Vector3.up * 2 );
		}
		else
		{
			Debug.LogError( "newLine is null." );
		}
	}

	#endregion
}
