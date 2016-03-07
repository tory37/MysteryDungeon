using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum ChunkStatus { Occupied, Unoccupied };
public enum CellStatus { HasWall, NoWall };

public class Cell
{
	public int column, row;

	public Cell()
	{
		column = row = 0;
	}

	public Cell( int column, int row )
	{
		this.column = column;
		this.row = row;
	}

	public Vector3 ToVector3()
	{
		return new Vector3( column, 0f, row );
	}
}

/// <summary>
/// This class is used to generate the floor at the beginning of a level, or "floor."  
/// The process is described below:
///		* Add these points later
///	The terms North, East, South and West represent the position of chunks relative to other chunks.
///		North represents the saame X value, but a higher Z value
///		East represents the same Z value, but a higher X value
///		South represents the same X value, but  lower Z value
///		West represents the same Z value, but a lower X value
/// </summary>
public class FloorGenerator : MonoBehaviour
{
	#region Private Data Structures

	private class Chunk
	{
		/// <summary>
		/// X and Z index of the chunk
		/// </summary>
		public int X, Z;
		/// <summary>
		/// The partition this chunk lies in for connecting
		/// chunks together
		/// </summary>
		public int Partition;
		/// <summary>
		/// Random X and Z index that paths will connect to 
		/// if this chunk doesn't have a room
		/// </summary>
		public int randX, randZ;

		public class Room
		{
			public int columns, rows, startingColumn, startingRow;
		}

		public ChunkStatus Status;
		public Room MyRoom;

		public GameObject CenterMarker;

		public Chunk( int x, int z, int randX, int randZ )
		{
			X = x;
			Z = z;
			Status = ChunkStatus.Unoccupied;
			MyRoom = null;
			this.randX = randX;
			this.randZ = randZ;
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

	private class Node : IEquatable<Node>
	{
		public Cell cell;
		public int gCost;
		public int hCost;
		public Node parent;

		public Node( Cell cell )
		{
			this.cell = cell;
		}

		public int fCost
		{
			get { return gCost + hCost; }
		}

		public bool Equals( Node other)
		{
			if ( other == null )
				return false;

			return this.cell.column == other.cell.column && this.cell.row == other.cell.row;
		}
	}

	#endregion

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

	#region Public Interface

	public CellStatus[,] GenerateFloor(ref int numColumns, ref int numRows)
	{
		numColumns = numColumnsInFloor;
		numRows = numRowsInFloor;

		ResetFloor();
		GenerateRooms();
		ConnectAllRooms();
		DrawFloor();

		return floorCells;
	}

	#endregion

	#region Private Fields

	/// <summary>
	/// Represents the tiles in the floor
	/// </summary>
	private CellStatus[,] floorCells;

	private int totalNumChunks;

	private int numColumnsInChunk;
	private int numRowsInChunk;

	private Chunk[,] chunks;

	private int partitionCount;

	#endregion

	#region Mono Methods

	private void Start()
	{
		totalNumChunks = numChunksX * numChunksZ;

		numColumnsInChunk = Mathf.FloorToInt( numColumnsInFloor / numChunksX );
		numRowsInChunk = Mathf.FloorToInt( numRowsInFloor / numChunksZ );
	}

	#endregion

	#region Private Methods

	private void ResetFloor()
	{
		floorCells = new CellStatus[numColumnsInFloor, numRowsInFloor];

		chunks = new Chunk[numChunksX, numChunksZ];

		partitionCount = 0;

		// Reset chunks
		for ( int x = 0; x < numChunksX; x++ )
		{
			for ( int z = 0; z < numChunksZ; z++ )
			{
				chunks[x, z] = new Chunk( x, z, UnityEngine.Random.Range( numColumnsInChunk * x + 1, numColumnsInChunk * (x + 1) - 1 ), UnityEngine.Random.Range( numRowsInChunk * z + 1, numRowsInChunk * (z + 1) - 1 ) );
				chunks[x, z].Partition = partitionCount;
				partitionCount++;
				chunks[x, z].CenterMarker = new GameObject();
				chunks[x, z].CenterMarker.transform.position = new Vector3( (x * numColumnsInChunk) + (.5f * numColumnsInChunk), 2f, (z * numRowsInChunk) + (.5f * numRowsInChunk) );
				chunks[x, z].CenterMarker.transform.parent = chunkObjectsParent;
			}
		}

		// Reset Tiles
		for ( int column = 0; column < numColumnsInFloor; column++ )
		{
			for ( int width = 0; width < numRowsInFloor; width++ )
			{
				floorCells[column, width] = CellStatus.HasWall;
			}
		}
	}

	private void GenerateRooms()
	{
		maxRooms = maxRooms > totalNumChunks ? totalNumChunks : maxRooms;
		int numRooms = UnityEngine.Random.Range( minRooms, maxRooms );

		for ( int room = 0; room < numRooms; room++ )
		{
			int chunkColumn = UnityEngine.Random.Range( 0, numChunksX );
			int chunkRow = UnityEngine.Random.Range( 0, numChunksZ );

			for ( int i = 0; i < findRoomStartTries; i++ )
			{
				if ( chunks[chunkColumn, chunkRow].Status == ChunkStatus.Unoccupied )
					break;
				else
				{
					chunkColumn = UnityEngine.Random.Range( 0, numChunksX );
					chunkRow = UnityEngine.Random.Range( 0, numChunksZ );
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
			Chunk endingChunk = FindEndingChunk( startingChunk );

			if ( endingChunk != null )
				ConnectTwoChunks( startingChunk, endingChunk );
		}

		// Loop through all rooms to ensure all are in same partition if random finding is taking too long
		if ( !CheckAllRoomsConnected() )
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
				if ( chunks[x, z].MyRoom != null )
				{
					zerothPartition = chunks[x, z].Partition;
					break;
				}
			}
			if ( zerothPartition != -1 )
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

		int roomColumns = UnityEngine.Random.Range( minRoomColumns, maxRoomColumns < numColumnsInChunk ? maxRoomColumns : numColumnsInChunk );
		int roomRows = UnityEngine.Random.Range( minRoomRows, maxRoomRows < numRowsInChunk ? maxRoomRows : numRowsInChunk );
		//Debug.Log( "Columns: " + roomColumns );
		//Debug.Log( "Rows: " + roomRows );

		int leastColumn = chunkColumn * numColumnsInChunk;
		int maxColumn = ((chunkColumn + 1) * numColumnsInChunk) - roomColumns;
		int leastRow = chunkRow * numRowsInChunk;
		int maxRow = ((chunkRow + 1) * numRowsInChunk) - roomRows;

		int startingColumn = UnityEngine.Random.Range( leastColumn, maxColumn );
		int startingRow = UnityEngine.Random.Range( leastRow, maxRow );

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
					if ( (column == startingColumn && column > 0 && floorCells[column - 1, row] == CellStatus.NoWall)
					   || (column == (roomColumns + startingColumn) - 1 && column < numColumnsInFloor - 1 && floorCells[column + 1, row] == CellStatus.NoWall)
					   || (row == startingRow && row > 0 && floorCells[column, row - 1] == CellStatus.NoWall)
					   || (row == (roomRows + startingRow) - 1 && row < numRowsInFloor - 1 && floorCells[column, row + 1] == CellStatus.NoWall) )
						Debug.Log( "Tile " + column + ", " + row + " did not get placed to maintain a wall on a side." );
					else if ( floorCells[column, row] == CellStatus.HasWall )
						floorCells[column, row] = CellStatus.NoWall;
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
				if ( chunks[x, z].MyRoom != null )
					Debug.Log( "Chunk " + x + ", " + z + " in partition " + chunks[x, z].Partition );
			}
		}

		for ( int column = 0; column < numColumnsInFloor; column++ )
		{
			for ( int row = 0; row < numRowsInFloor; row++ )
			{
				if ( floorCells[column, row] == CellStatus.HasWall )
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

		int x = UnityEngine.Random.Range( 0, numChunksX - 1 );
		int z = UnityEngine.Random.Range( 0, numChunksZ - 1 );

		// Pick starting chunk with a room
		while ( startingChunk == null )
		{
			Chunk temp = chunks[x, z];
			if ( temp.MyRoom != null )
				startingChunk = temp;

			x = UnityEngine.Random.Range( 0, numChunksX - 1 );
			z = UnityEngine.Random.Range( 0, numChunksZ - 1 );
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
		bool canEast, canSouth, canWest, canNorth;
		canEast = canSouth = canWest = canNorth = true;

		// Search the surrounding chunks for a room that can be patched with a hall that's not in the starting partition
		while ( x + modifier < numChunksX || x - modifier >= 0 || z + modifier < numChunksZ || z - modifier >= 0 )
		{
			// Check East
			if ( canEast && x + modifier < numChunksX )
			{
				if ( startingChunk.Partition != chunks[x + modifier, z].Partition )
				{
					endingChunk = chunks[x + modifier, z];
					break;
				}
				else
					canEast = false;
			}
			// Check South
			if ( canSouth && z - modifier >= 0 )
			{
				if ( startingChunk.Partition != chunks[x, z - modifier].Partition )
				{
					endingChunk = chunks[x, z - modifier];
					break;
				}
				else
					canSouth = false;
			}
			// Check West
			if ( canWest && x - modifier >= 0 )
			{
				if ( startingChunk.Partition != chunks[x - modifier, z].Partition )
				{
					endingChunk = chunks[x - modifier, z];
					break;
				}
				else
					canWest = false;
			}
			// Check North
			if ( canNorth && z + modifier < numChunksZ )
			{
				if ( startingChunk.Partition != chunks[x, z + modifier].Partition )
				{
					endingChunk = chunks[x, z + modifier];
					break;
				}
				else
					canNorth = false;
			}

			modifier++;
		}

		return endingChunk;
	}

	private Cell CellRound(float column, float row)
	{
		return new Cell( Mathf.FloorToInt( column ), Mathf.FloorToInt( row ) );
	}

	private void ConnectTwoChunks( Chunk startingChunk, Chunk endingChunk )
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

		List<Cell> path = FindPathSimple( startingChunk, endingChunk );
		//List<Cell> path = FindPathInterpolate( startingChunk, endingChunk );

		for (int i = 0; i < path.Count; i++)
		{
			int column = path[i].column;
			int row = path[i].row;
			floorCells[column, row] = CellStatus.NoWall;
		}

		//FindPath( startingChunk, endingChunk );

		//DrawDebugLinesBetweenChunks(startingChunk, endingChunk);
		
	}

	private void DrawDebugLinesBetweenChunks(Chunk startingChunk, Chunk endingChunk)
	{
		// Draw Lines
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

	private List<Cell> FindPathSimple (Chunk startingChunk, Chunk endingChunk)
	{
		List<Cell> path = new List<Cell>();

		// Find the starting tile
		Cell startCell = new Cell();

		FindFirstChunkPathCell( ref startCell, startingChunk, endingChunk );

		// Find ending tile
		Cell endCell = new Cell();

		FindFirstChunkPathCell( ref endCell, endingChunk, startingChunk );

		int dx = endCell.column - startCell.column;
		int dz = endCell.row - startCell.row;

		path.Add( startCell );

		// End is North
		if (endingChunk.Z > startingChunk.Z)
		{
			//// Cells are too close for a jag
			//if (dz == 3 || dz == 2)
			//	startCell.column = endCell.column;

			int randomMid = UnityEngine.Random.Range( startCell.row + 1, endCell.row );
			// 1. Go North
			for (int row = startCell.row+1; row <= randomMid; row++)
				path.Add( new Cell( startCell.column, row ) );
			// 2. Go East
			if (dx > 0)
			{
				for ( int col = startCell.column; col <= endCell.column; col++ )
					path.Add( new Cell( col, randomMid ) );
			}
			// 2. Go West
			else
			{
				for ( int col = startCell.column; col >= endCell.column; col-- )
					path.Add( new Cell( col, randomMid ) );
			}
			// 3. Go North
			for ( int row = randomMid; row <= endCell.row; row++ )
				path.Add( new Cell( endCell.column, row ) );
		}
		// End is East
		if ( endingChunk.X > startingChunk.X )
		{
			//// Cells are too close for a jag
			//if ( dx == 3 || dx == 2 )
			//	startCell.row = endCell.row;

			int randomMid = UnityEngine.Random.Range( startCell.column + 1, endCell.column );
			// 1. Go East
			for ( int col = startCell.column+1; col <= randomMid; col++ )
			{
				path.Add( new Cell(col, startCell.row ) );
			}
			// 2. Go North
			if ( dz > 0 )
			{
				for ( int row = startCell.row; row <= endCell.row; row++ )
					path.Add( new Cell( randomMid, row ) );
			}
			// 2. Go South
			else
			{
				for ( int row = startCell.row; row >= endCell.row; row-- )
					path.Add( new Cell( randomMid, row ) );
			}
			// 3. Go East
			for ( int col = randomMid; col <= endCell.column; col++ )
				path.Add( new Cell( col, endCell.row ) );
		}
		// End is South
		if ( endingChunk.Z < startingChunk.Z )
		{
			//// Cells are too close for a jag
			//if ( dz == -3 || dz == -2 )
			//	startCell.column = endCell.column;

			int randomMid = UnityEngine.Random.Range( endCell.row-1, startCell.row );
			// 1. Go South
			for ( int row = startCell.row-1; row >= randomMid; row-- )
			{
				path.Add( new Cell( startCell.column, row ) );
			}
			// 2. Go East
			if ( dx > 0 )
			{
				for ( int col = startCell.column; col <= endCell.column; col++ )
					path.Add( new Cell( col, randomMid ) );
			}
			// 2. Go West
			else
			{
				for ( int col = startCell.column; col >= endCell.column; col-- )
					path.Add( new Cell( col, randomMid ) );
			}
			// 3. Go North
			for ( int row = randomMid; row >= endCell.row; row-- )
				path.Add( new Cell( endCell.column, row ) );
		}
		// End is West
		if ( endingChunk.X < startingChunk.X )
		{
			//// Cells are too close for a jag
			//if ( dx == -3 || dx == -2 )
			//	startCell.row = endCell.row;

			int randomMid = UnityEngine.Random.Range( endCell.column-1, startCell.column );
			// 1. Go West
			for ( int col = startCell.column-1; col >= randomMid; col-- )
			{
				path.Add( new Cell( col, startCell.row ) );
			}
			// 2. Go North
			if ( dz > 0 )
			{
				for ( int row = startCell.row; row <= endCell.row; row++ )
					path.Add( new Cell( randomMid, row ) );
			}
			// 2. Go South
			else
			{
				for ( int row = startCell.row; row >= endCell.row; row-- )
					path.Add( new Cell( randomMid, row ) );
			}
			// 3. Go West
			for ( int col = randomMid; col >= endCell.column; col-- )
				path.Add( new Cell( col, endCell.row ) );
		}

		return path;
	}

	private List<Cell> FindPathInterpolate( Chunk startingChunk, Chunk endingChunk )
	{
		List<Node> path = new List<Node>();

		// Find the starting tile
		Cell startCell = new Cell();

		FindFirstChunkPathCell( ref startCell, startingChunk, endingChunk );

		// Find ending tile
		Cell endCell = new Cell();

		FindFirstChunkPathCell( ref endCell, endingChunk, startingChunk );

		int dx = endCell.column - startCell.column, dz = endCell.row - startCell.row;
		int nx = Mathf.Abs( dx ), nz = Mathf.Abs( dz );
		int sign_x = dx > 0 ? 1 : -1, sign_z = dz > 0 ? 1 : -1;

		Cell c = new Cell(startCell.column, startCell.row);
		List<Cell> cells = new List<Cell> { new Cell(c.column, c.row) };
		for (int ix = 0, iz = 0; ix < nx || iz < nz; )
		{
			if ((0.5+ix) / nx < (0.5+iz) / nz)
			{
				// next step is horizontal
				c.column += sign_x;
				ix++;
			}
			else
			{
				// next step is vertical
				c.row += sign_z;
				iz++;
			}
			cells.Add( new Cell( c.column, c.row ) );
		}
		return cells;
	}

	private void FindPath( Chunk startingChunk, Chunk endingChunk )
	{
		List<Node> path = new List<Node>();

		// Find the starting tile
		Cell startCell = new Cell();

		FindFirstChunkPathCell( ref startCell, startingChunk, endingChunk );

		// Find ending tile
		Cell endCell = new Cell();

		FindFirstChunkPathCell( ref endCell, endingChunk, startingChunk );

		// Find path
		Node startNode = new Node( startCell );
		Node endNode = new Node( endCell );

		List<Node> openSet = new List<Node>();
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add( startNode );

		while(openSet.Count > 0)
		{
			Node currentNode = openSet[0];
			for (int i = 1; i < openSet.Count; i++)
			{
				if ( openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
				{
					currentNode = openSet[i];
				}
			}

			openSet.Remove( currentNode );
			closedSet.Add( currentNode );

			if (currentNode == endNode)
			{
				RetracePath(startNode, endNode);
				return;
			}

			foreach (Node neighbour in FindNeighbourNodes(currentNode))
			{
				if ( closedSet.Contains( neighbour ) )
					continue;

				int newMovementCostToNeighbour = currentNode.gCost + GetNodeDistance( currentNode, neighbour );
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetNodeDistance( neighbour, endNode );
					neighbour.parent = currentNode;

					if ( !openSet.Contains( neighbour ) )
						openSet.Add( neighbour );
				}
			}
		}

		for (int i = 0; i < path.Count; i++)
		{
			floorCells[path[i].cell.column, path[i].cell.row] = CellStatus.NoWall;
		}
	}

	private List<Node> RetracePath( Node startNode, Node endNode )
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add( currentNode );
			currentNode = currentNode.parent;
		}
		path.Reverse();

		return path;
	}

	private int GetNodeDistance(Node a, Node b)
	{
		int dstX = Mathf.Abs( a.cell.column - b.cell.column );
		int dstY = Mathf.Abs( a.cell.row - b.cell.row );

		if ( dstX > dstY )
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
	}

	private List<Node> FindNeighbourNodes( Node baseNode )
	{
		List<Node> result = new List<Node>();

		// Find North
		if ( baseNode.cell.row < numRowsInFloor - 1 )
			result.Add( new Node( new Cell { column = baseNode.cell.column, row = baseNode.cell.row + 1 } ) );
		// Find East
		if ( baseNode.cell.column < numColumnsInFloor - 1 )
			result.Add( new Node( new Cell { column = baseNode.cell.column + 1, row = baseNode.cell.row } ) );
		// Find South
		if ( baseNode.cell.row > 0 )
			result.Add( new Node( new Cell { column = baseNode.cell.column, row = baseNode.cell.row - 1 } ) );
		// Find West
		if ( baseNode.cell.column > 0 )
			result.Add( new Node( new Cell { column = baseNode.cell.column - 1, row = baseNode.cell.row } ) );

		return result;
	}

	/// <summary>
	/// Find a random tile either on the edge of a room, or finds an appropriate tile in a roomless chunk 
	///		for the pathfinding algorithm to use as an end point.  It finds the tile for \firstChunk\ based
	///		on the location of \secondChunk\
	/// </summary>
	/// <param name="col"></param>
	/// <param name="row"></param>
	/// <param name="firstChunk"></param>
	/// <param name="secondChunk"></param>
	private void FindFirstChunkPathCell( ref Cell cell, Chunk firstChunk, Chunk secondChunk )
	{
		if ( firstChunk.MyRoom != null )
		{
			// If end if north
			if ( secondChunk.Z > firstChunk.Z && secondChunk.X == firstChunk.X )
			{
				cell.column = UnityEngine.Random.Range( firstChunk.MyRoom.startingColumn, firstChunk.MyRoom.startingColumn + firstChunk.MyRoom.columns );
				cell.row = firstChunk.MyRoom.startingRow + firstChunk.MyRoom.rows;
			}
			// If end is south
			else if ( secondChunk.Z < firstChunk.Z && secondChunk.X == firstChunk.X )
			{
				cell.column = UnityEngine.Random.Range( firstChunk.MyRoom.startingColumn, firstChunk.MyRoom.startingColumn + firstChunk.MyRoom.columns );
				cell.row = firstChunk.MyRoom.startingRow - 1;
			}
			// If end is east
			else if ( secondChunk.Z == firstChunk.Z && secondChunk.X > firstChunk.X )
			{
				cell.column = firstChunk.MyRoom.startingColumn + firstChunk.MyRoom.columns;
				cell.row = UnityEngine.Random.Range( firstChunk.MyRoom.startingRow, firstChunk.MyRoom.startingRow + firstChunk.MyRoom.rows );
			}
			else if ( secondChunk.Z == firstChunk.Z && secondChunk.X < firstChunk.X )
			{
				cell.column = firstChunk.MyRoom.startingColumn - 1;
				cell.row = UnityEngine.Random.Range( firstChunk.MyRoom.startingRow, firstChunk.MyRoom.startingRow + firstChunk.MyRoom.rows );
			}
			else
				Debug.LogError( "Ending chunk is diagnol from the starting chunk.  This shouldnt happen." );
		}
		else
		{
			cell.column = firstChunk.randX; //Mathf.FloorToInt( (firstChunk.X * numColumnsInChunk) + (.5f * numColumnsInChunk) );
			cell.row = firstChunk.randZ; //Mathf.FloorToInt( (firstChunk.Z * numRowsInChunk) + (.5f * numRowsInChunk) );
		}
	}

	#endregion
}
