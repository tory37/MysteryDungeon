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
		Controllable leader = LevelManager.Instance.ControlledLeader;
		if ( (Mathf.Abs( leader.Column - Column ) == 1 && leader.Row == Row) || (Mathf.Abs( leader.Row - Row ) == 1 && leader.Column == Column) || (Mathf.Abs( leader.Column - Column ) == 1 && Mathf.Abs( leader.Row - Row ) == 1) )
			return new Cell( Column, Row );
		else if ( leader.Column == Column && leader.Row == Row )
			return new Cell( leader.OldColumn, leader.OldRow );
		else
		{
			Node newNode = null;
			if ( Paths.FindNextNode( LevelManager.Instance.FloorNodes[Column, Row], LevelManager.Instance.FloorNodes[LevelManager.Instance.ControlledLeader.Column, LevelManager.Instance.ControlledLeader.Row], out newNode ) )
				return newNode.Cell;
			else
				return new Cell( Column, Row );
		}
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
