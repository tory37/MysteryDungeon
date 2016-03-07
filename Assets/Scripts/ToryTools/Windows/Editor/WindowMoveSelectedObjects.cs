using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// A window to move the selected objects in the Editor a certain Vector3 
/// in the scene when the Move button is pressed.
/// </summary>
public class WindowMoveSelectedObjects : EditorWindow {

	[MenuItem("Tory's Tools/ Move Selected Objects")]

	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(WindowMoveSelectedObjects));
	}

	private Vector3 moveVector = new Vector3(0, 0, 0);

	int globalOrLocal;

	string[] options = new string[] { "Global Space", "Local Space" };

	void OnGUI()
	{

		GUILayout.Label("Move Selected Objects", EditorStyles.boldLabel);

		GUILayout.Space(20);

		globalOrLocal = GUILayout.SelectionGrid(globalOrLocal, options, 2 );

		GUILayout.Space(20);

		moveVector = EditorGUILayout.Vector3Field("Amount to Move", moveVector);

		GUILayout.Space(20);

		if (GUILayout.Button("Move"))
		{
			GameObject[] objectsToMove = Selection.gameObjects;

			for ( int i = 0; i < objectsToMove.Length; i++ )
			{
				Undo.RecordObject(objectsToMove[i].transform, "Move Selected Object");
			}

			//Global
			if (globalOrLocal == 0)
			{
				for ( int i = 0; i < objectsToMove.Length; i++ )
				{
					objectsToMove[i].transform.position += moveVector;
				}
			}
			//Local
			else if (globalOrLocal == 1)
			{
				for ( int i = 0; i < objectsToMove.Length; i++ )
				{
					objectsToMove[i].transform.localPosition += moveVector;
				}
			}
			else
			{
				Debug.LogError("Move Selected Objects window is in a state of neither local nor global space.  It shouldn't be.");
			}
		}

		GUILayout.FlexibleSpace();
	}
}
