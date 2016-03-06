using UnityEngine;
using System.Collections;

public class TestPlayer : MonoBehaviour {

	[SerializeField]
	private float moveSpeed, rotateSpeed;


	// Update is called once per frame
	void FixedUpdate () {
		float vert = Input.GetAxis( "Vertical" ) * moveSpeed * Time.deltaTime;
		float hor = Input.GetAxis( "Horizontal" ) * moveSpeed * Time.deltaTime;

		float rotate = Input.GetAxis( "Mouse X" ) * rotateSpeed * Time.deltaTime;

		GetComponent<Rigidbody>().MoveRotation( GetComponent<Rigidbody>().rotation * Quaternion.Euler( Vector3.up * rotate ) );
		GetComponent<Rigidbody>().MovePosition( GetComponent<Rigidbody>().position + (transform.forward * vert) + (transform.right * hor) );
	}
}
