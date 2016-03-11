﻿using UnityEngine;
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

	protected override void Initialize()
	{
		IsLeader = true;
	}

	public override Participant.TurnType DetermineTurn()
	{
		return TurnType.Move;
	}

	public override Cell FindNewCell()
	{
		return new Cell( 1, 1 );
	}

	public override void TakeAction( Participant.FinishedActionCallback callback )
	{
		callback();
	}
}
