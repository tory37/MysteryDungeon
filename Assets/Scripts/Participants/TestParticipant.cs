using UnityEngine;
using System.Collections;

public class TestParticipant : Participant {

	public override TurnType DetermineTurn()
	{
		return TurnType.Move;
	}

	public override void TakeAction( FinishedActionCallback callback )
	{
		callback();
	}

	public override Cell FindNewCell()
	{
		LevelManager lm = LevelManager.Instance;

		int minCol = 0, maxCol= 0, minRow = 0, maxRow = 0;

		if ( lm.NumColumns > Column + 1  && lm.FloorCells[Column + 1, Row] == CellStatus.UnOccupied)
			maxCol = 1;
		if ( Column - 1 >= 0 && lm.FloorCells[Column - 1, Row] == CellStatus.UnOccupied )
			minCol = -1;
		if ( lm.NumRows > Row + 1 && lm.FloorCells[Column, Row + 1] == CellStatus.UnOccupied )
			maxRow = 1;
		if ( Row - 1 >= 0 && lm.FloorCells[Column, Row - 1] == CellStatus.UnOccupied )
			minRow = -1;

		int newCol = Column + Random.Range(minCol, maxCol);
		int newRow = Row + Random.Range(minRow, maxRow);

		return new Cell( newCol, newRow);
	}
}
