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
		//Cell newCell = new Cell(Column, Row);

		//for ( int i = 0; i < 50; i++ )
		//{
		//	int rC = Random.Range( -1, 1 );
		//	int rR = Random.Range( -1, 1 );

		//	if ( (rC != 0 || rR != 0) && LevelManager.Instance.FloorCells[rC + Column, rR + Row] == CellStatus.UnOccupied )
		//	{
		//		newCell = new Cell( rC + Column, rR + Row );
		//		break;
		//	}
		//}

		return new Cell(Column + Random.Range(-1, 1), Row + Random.Range(-1, 1));
	}
}
