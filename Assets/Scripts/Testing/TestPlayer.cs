using UnityEngine;
using System.Collections;

public class TestPlayer : Controllable {

	//[SerializeField]
	//private float moveSpeed, rotateSpeed;


	//// Update is called once per frame
	//void FixedUpdate () {
	//	float vert = Input.GetAxis( "Vertical" ) * moveSpeed * Time.deltaTime;
	//	float hor = Input.GetAxis( "Horizontal" ) * moveSpeed * Time.deltaTime;

	//	float rotate = Input.GetAxis( "Mouse X" ) * rotateSpeed * Time.deltaTime;

	//	GetComponent<Rigidbody>().MoveRotation( GetComponent<Rigidbody>().rotation * Quaternion.Euler( Vector3.up * rotate ) );
	//	GetComponent<Rigidbody>().MovePosition( GetComponent<Rigidbody>().position + (transform.forward * vert) + (transform.right * hor) );
	//}

	public override Participant.TurnType DetermineTurn()
	{
		return TurnType.Move;
	}

	public override Cell FindNewCell()
	{
		Node[,] floorNodes = LevelManager.Instance.FloorNodes;
		Controllable leader = LevelManager.Instance.ControlledLeader;
		if ( ((Mathf.Abs( leader.Column - Column ) == 1 && leader.Row == Row) || (Mathf.Abs( leader.Row - Row ) == 1 && leader.Column == Column) || (Mathf.Abs( leader.Column - Column ) == 1 && Mathf.Abs( leader.Row - Row ) == 1))
			&& !(leader.Column == Column + 1 && leader.Row == Row + 1 &&
					(Paths.cornerPassableOccupancies.Contains( floorNodes[Column + 1, Row].Cell.Status ) ||
					Paths.cornerPassableOccupancies.Contains( floorNodes[Column, Row + 1].Cell.Status )))
			&& !(leader.Column == Column - 1 && leader.Row == Row + 1 &&
					(Paths.cornerPassableOccupancies.Contains( floorNodes[Column - 1, Row].Cell.Status ) ||
					Paths.cornerPassableOccupancies.Contains( floorNodes[Column, Row + 1].Cell.Status )))
			&& !(leader.Column == Column - 1 && leader.Row == Row - 1 &&
					(Paths.cornerPassableOccupancies.Contains( floorNodes[Column - 1, Row].Cell.Status ) ||
					Paths.cornerPassableOccupancies.Contains( floorNodes[Column, Row - 1].Cell.Status )))
			&& !(leader.Column == Column + 1 && leader.Row == Row - 1 &&
					(Paths.cornerPassableOccupancies.Contains( floorNodes[Column + 1, Row].Cell.Status ) ||
					Paths.cornerPassableOccupancies.Contains( floorNodes[Column, Row - 1].Cell.Status ))) )
			return new Cell( Column, Row );
		else if ( leader.Column == Column && leader.Row == Row )
			return new Cell( leader.OldColumn, leader.OldRow );
		else
		{
			Node newNode = null;

			if ( Paths.FindNextNode( LevelManager.Instance.FloorNodes[Column, Row], floorNodes[leader.Column, leader.Row], out newNode )
				&& newNode.Cell.Status != CellOccupancy.Player )
				return newNode.Cell;
			else
				return new Cell( Column, Row );
		}
	}

	public Node FindFreeAdjacentNode(Controllable leader, Node[,] floorNodes)
	{
		if ( floorNodes[leader.Column, leader.Row + 1].Cell.Status == CellOccupancy.Open )
			return floorNodes[leader.Column, leader.Row + 1];
		else if ( floorNodes[leader.Column + 1, leader.Row].Cell.Status == CellOccupancy.Open )
			return floorNodes[leader.Column + 1, leader.Row];
		else if ( floorNodes[leader.Column, leader.Row - 1].Cell.Status == CellOccupancy.Open )
			return floorNodes[leader.Column, leader.Row - 1];
		else if ( floorNodes[leader.Column - 1, leader.Row].Cell.Status == CellOccupancy.Open )
			return floorNodes[leader.Column - 1, leader.Row];
		else if ( floorNodes[leader.Column + 1, leader.Row + 1].Cell.Status == CellOccupancy.Open )
			return floorNodes[leader.Column + 1, leader.Row + 1];
		else if ( floorNodes[leader.Column - 1, leader.Row + 1].Cell.Status == CellOccupancy.Open )
			return floorNodes[leader.Column - 1, leader.Row + 1];
		else if ( floorNodes[leader.Column - 1, leader.Row - 1].Cell.Status == CellOccupancy.Open )
			return floorNodes[leader.Column - 1, leader.Row - 1];
		else if ( floorNodes[leader.Column - 1, leader.Row + 1].Cell.Status == CellOccupancy.Open )
			return floorNodes[leader.Column - 1, leader.Row + 1];
		else
			return floorNodes[leader.Column, leader.Row];
	}

	public override void TakeAction( Participant.FinishedActionCallback callback )
	{
		StartCoroutine( TestAction( callback ) );
	}

	private IEnumerator TestAction(Participant.FinishedActionCallback callback)
	{
		Anim.SetTrigger( "Attack" );
		yield return new WaitForSeconds( 1.7f );
		callback();
	}
}
