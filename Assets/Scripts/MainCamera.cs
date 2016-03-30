using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour
{

	#region Editor Interface

	[SerializeField]
	private Vector3 offset;

	#endregion

	#region Mono Methods

	private void FixedUpdate()
	{
		transform.position = LevelManager.Instance.ControlledLeader.transform.position + offset;
	}

	#endregion

}
