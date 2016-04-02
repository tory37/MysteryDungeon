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

		Node nextNode;
		if ( Paths.FindNextNode( lm.FloorNodes[Column, Row], lm.FloorNodes[lm.ControlledLeader.Column, lm.ControlledLeader.Row], out nextNode ) )
			return nextNode.Cell;
		else
		{

			int minCol = 0, maxCol= 0, minRow = 0, maxRow = 0;

			if ( lm.FloorColumns > Column + 1 && lm.FloorCells[Column + 1, Row].Status == CellStatus.Open )
				maxCol = 1;
			if ( Column - 1 >= 0 && lm.FloorCells[Column - 1, Row].Status == CellStatus.Open )
				minCol = -1;
			if ( lm.FloorRows > Row + 1 && lm.FloorCells[Column, Row + 1].Status == CellStatus.Open )
				maxRow = 1;
			if ( Row - 1 >= 0 && lm.FloorCells[Column, Row - 1].Status == CellStatus.Open )
				minRow = -1;

			int newCol = Column + Random.Range( minCol, maxCol );
			int newRow = Row + Random.Range( minRow, maxRow );

			return new Cell( newCol, newRow );
		}
	}
}
