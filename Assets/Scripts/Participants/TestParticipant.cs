using UnityEngine;
using System.Collections;

public class TestParticipant : Participant {

	public override TurnType DetermineTurn()
	{
		if ( (LevelManager.Instance.ControlledLeader.transform.position - transform.position).magnitude < 10 )
			return TurnType.Action;
		else
			return TurnType.Move;
	}

	public override void TakeAction( FinishedActionCallback callback )
	{
		StartCoroutine( TestAction(callback) );
	}

	private IEnumerator TestAction(FinishedActionCallback callback)
	{
		Debug.Log( "Taking Action" );
		int counter = 0;
		while (counter < 45)
		{
			transform.Rotate( Vector3.up * 2 );
			counter += 1;
			yield return new WaitForEndOfFrame();
		}

		callback();
	}

	public override Cell FindNewCell()
	{
		LevelManager lm = LevelManager.Instance;

		Node nextNode = null;
		if ( Paths.FindNextNode( lm.FloorNodes[Column, Row], lm.FloorNodes[lm.ControlledLeader.Column, lm.ControlledLeader.Row], out nextNode ) )
			return nextNode.Cell;
		else
		{

			int minCol = 0, maxCol= 0, minRow = 0, maxRow = 0;

			if ( lm.FloorColumns > Column + 1  && lm.FloorCells[Column + 1, Row].Status == CellOccupancy.Open)
				maxCol = 1;
			if ( Column - 1 >= 0 && lm.FloorCells[Column - 1, Row].Status == CellOccupancy.Open )
				minCol = -1;
			if ( lm.FloorRows > Row + 1 && lm.FloorCells[Column, Row + 1].Status == CellOccupancy.Open )
				maxRow = 1;
			if ( Row - 1 >= 0 && lm.FloorCells[Column, Row - 1].Status == CellOccupancy.Open )
				minRow = -1;

			int newCol = Column + Random.Range(minCol, maxCol);
			int newRow = Row + Random.Range(minRow, maxRow);
			return new Cell( newCol, newRow );
		}
	}
}
