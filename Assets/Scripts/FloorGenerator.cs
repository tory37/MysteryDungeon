using UnityEngine;
using System.Collections;

public enum ChunkStatus { Occupied, Unoccupied };
public enum TileStatus { HasWall, NoWall };

public struct Room
{

}

public class FloorGenerator : MonoBehaviour
{

	#region Editor Interface

	[Header("Floor")]
	[SerializeField] private int numColumnsInFloor;
	[SerializeField] private int numRowsInFloor;

	[Header( "Chunks" )]
    [SerializeField] private int numChunksX;
    [SerializeField] private int numChunksZ;

	[Header( "Rooms" )]
	[SerializeField] private int minRooms;
	[SerializeField] private int maxRooms;

	[Header( "Min Max Columns" )]
	[SerializeField] private int minRoomColumns;
	[SerializeField] private int maxRoomColumns;

	[Header( "Min Max Rows" )]
	[SerializeField] private int minRoomRows;
	[SerializeField] private int maxRoomRows;

	[Header("Other")]
	[Tooltip("This represents how many times the a random starting point for a room is attempted to be found before incrementally looping through the spaces.  This is to avoid the possibility of 'never finding an open space.'")]
	[SerializeField] private int findRoomStartTries;

	[SerializeField] private GameObject wallTestObject;
	[SerializeField] private GameObject floorTestObject;

	#endregion

	#region Private Fields

	/// <summary>
	/// Represents the tiles in the floor
	/// </summary>
	private TileStatus[,] tileInFloor;

    private int totalNumChunks;

    private int numberColumnsInChunk;
    private int numberRowsInChunk;

    /// <summary>
    /// True: the chunk is occupied by a room
    /// False: the chunk is not occupied by a room
    /// </summary>
    private ChunkStatus[,] chunksInFloor;

	#endregion

	#region Mono Methods

	private void Start()
	{
		tileInFloor = new TileStatus[numColumnsInFloor, numRowsInFloor];
        chunksInFloor = new ChunkStatus[numChunksX, numChunksZ];

        totalNumChunks = numChunksX * numChunksZ;

        numberColumnsInChunk = Mathf.FloorToInt(numColumnsInFloor / numChunksX);
        numberRowsInChunk = Mathf.FloorToInt(numRowsInFloor / numChunksZ);

		ResetFloor();
		GenerateRooms();
		DrawFloor();
	}

	#endregion

	#region Private Methods

	private void ResetFloor()
	{
		for (int column = 0; column < numColumnsInFloor; column++)
		{
			for (int width = 0; width < numRowsInFloor; width++)
			{
				tileInFloor[column, width] = TileStatus.HasWall;
			}
		}

        for (int chunkX = 0; chunkX < numChunksX; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < numChunksZ; chunkZ++)
            {
                chunksInFloor[chunkX, chunkZ] = ChunkStatus.Unoccupied;
            }
        }
	}

	private void GenerateRooms()
	{
        maxRooms = maxRooms > totalNumChunks ? totalNumChunks : maxRooms;
		int numRooms = Random.Range( minRooms, maxRooms );

		for (int room = 0; room < numRooms; room++)
		{
            int chunkColumn = Random.Range(0, numChunksX);
            int chunkRow = Random.Range(0, numChunksZ);

            for (int i = 0; i < findRoomStartTries; i++ )
            {
                if (chunksInFloor[chunkColumn, chunkRow] == ChunkStatus.Unoccupied)
                    break;
                else
                {
                    chunkColumn = Random.Range(0, numChunksX);
                    chunkRow = Random.Range(0, numChunksZ);
                }
            }

            if (chunksInFloor[chunkColumn, chunkRow] == ChunkStatus.Occupied)
            {
                for (int chunkX = 0; chunkX < numChunksX; chunkX++)
                {
                    for (int chunkZ = 0; chunkZ < numRowsInFloor; chunkZ++)
                    {
                        chunkColumn = chunkX;
                        chunkRow = chunkZ;

                        if (chunksInFloor[chunkColumn, chunkRow] == ChunkStatus.Unoccupied)
                            break;
                    }
                    if (chunksInFloor[chunkColumn, chunkRow] == ChunkStatus.Unoccupied)
                        break;
                }
            }    

            GenerateRoom(chunkColumn, chunkRow);
		}
	}

	private void GenerateRoom(int chunkColum, int chunkRow)
	{
        Debug.Log("Room In Chunk: " + chunkColum + ", " + chunkRow);
        chunksInFloor[chunkColum, chunkRow] = ChunkStatus.Occupied;

		int roomColumns = Random.Range( minRoomColumns, maxRoomColumns < numberColumnsInChunk ? maxRoomColumns : numberColumnsInChunk );
		int roomRows = Random.Range( minRoomRows, maxRoomRows < numberRowsInChunk ? maxRoomRows : numberRowsInChunk );
        Debug.Log("Columns: " + roomColumns);
        Debug.Log("Rows: " + roomRows);

        int leastColumn = chunkColum * numberColumnsInChunk;
        int maxColumn = ((chunkColum + 1) * numberColumnsInChunk) - roomColumns;
        int leastRow = chunkRow * numberRowsInChunk;
        int maxRow = ((chunkRow + 1) * numberRowsInChunk ) - roomRows;

        int startingColumn = Random.Range(leastColumn, maxColumn);
        int startingRow = Random.Range(leastRow, maxRow);

		for (int column = startingColumn; column < roomColumns + startingColumn; column++)
		{
            for (int row = startingRow; row < roomRows + startingRow; row++)
            {
                try
                {
                    if ((column == startingColumn && column > 0 && tileInFloor[column - 1, row] == TileStatus.NoWall)
                       || (column == (roomColumns + startingColumn) - 1 && column < numColumnsInFloor - 1 && tileInFloor[column + 1, row] == TileStatus.NoWall)
                       || (row == startingRow && row > 0 && tileInFloor[column, row - 1] == TileStatus.NoWall)
                       || (row == (roomRows + startingRow) - 1 && row < numRowsInFloor - 1 && tileInFloor[column, row + 1] == TileStatus.NoWall))
                        Debug.Log("Tile " + column + ", " + row + " did not get placed to maintain a wall on a side.");
                    else if (tileInFloor[column, row] == TileStatus.HasWall)
                        tileInFloor[column, row] = TileStatus.NoWall;
                    else
                        Debug.LogError("During room generation, we tried to remove wall: " + column + ", " + row + ".  It had already been removed.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Chunk: " + chunkColum + ", " + chunkRow);
                    Debug.LogError("Column: " + column + ", Row: " + row + ", StartingRow: " + startingRow + ", MaxRow: " + maxRow );
                }
            }
        }
    }

	private void DrawFloor()
	{
		for ( int column = 0; column < numColumnsInFloor; column++ )
		{
			for ( int row = 0; row < numRowsInFloor; row++ )
			{
				if ( tileInFloor[column, row] == TileStatus.HasWall )
					GameObject.Instantiate( wallTestObject, new Vector3( column, .5f, row ), Quaternion.identity );
				else
					GameObject.Instantiate( floorTestObject, new Vector3( column, 0f, row ), Quaternion.identity );
			}
		}

        for (int column = -1; column <= numColumnsInFloor; column++)
        {
            GameObject.Instantiate(wallTestObject, new Vector3(column, .5f, -1), Quaternion.identity);
            GameObject.Instantiate(wallTestObject, new Vector3(column, .5f, numRowsInFloor), Quaternion.identity);
        }

        for (int row = -1; row <= numRowsInFloor; row++)
        {
            GameObject.Instantiate(wallTestObject, new Vector3(-1, .5f, row), Quaternion.identity);
            GameObject.Instantiate(wallTestObject, new Vector3(numColumnsInFloor, .5f, row), Quaternion.identity);
        }
	}

	#endregion

}
