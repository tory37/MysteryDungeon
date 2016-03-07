using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[CustomEditor(typeof(MonoFSM), true)]
public class MonoFSMInspector : Editor {

	public override void OnInspectorGUI()
	{
		MonoFSM fsm = (MonoFSM)target;

		if ( fsm.LockEnumName )
			EditorGUILayout.LabelField("State Enum Name: " + fsm.StateEnumName );
		else
			fsm.StateEnumName = EditorGUILayout.TextField( "State Enum Name", fsm.StateEnumName );

		fsm.LockEnumName = EditorGUILayout.Toggle( "Lock Name", fsm.LockEnumName);

		Type enumType = Type.GetType( fsm.StateEnumName + ",Assembly-CSharp" );

		if ( enumType != null && enumType.IsEnum)
		{
			string[] enumNames = Enum.GetNames( enumType );

			if (enumNames.Length > 0)
			{
				fsm.IsStatesExpanded = EditorGUILayout.Foldout( fsm.IsStatesExpanded, "States" );
			
				if ( fsm.IsStatesExpanded )
				{
					EditorGUI.indentLevel++;

					EditorGUILayout.BeginHorizontal();
					{
						if ( GUILayout.Button( "Add State" ) )
						{
							fsm.StateKeys.Add( 1 );
							fsm.StateValues.Add( null );
						}
					}
					EditorGUILayout.EndHorizontal();

					for ( int i = 0; i < fsm.StateKeys.Count; i++ ) 
					{
						string stateLabel = "EMPTY STATE / NULL";

						if ( fsm.StateValues[i] != null )
						{
							if ( fsm.StateValues[i].Identifier == "" )
								stateLabel = "EMPTY IDENTIFIER FOR THIS STATE";
							else
								stateLabel = fsm.StateValues[i].Identifier;
						}

						bool isStateExpanded = true;
						EditorGUILayout.BeginHorizontal();
						{
								if ( fsm.StateValues[i] == null )
									isStateExpanded = true;
								else
								{
									isStateExpanded = fsm.StateValues[i].IsStateExpanded;
									fsm.StateValues[i].IsStateExpanded = EditorGUILayout.Foldout( fsm.StateValues[i].IsStateExpanded, stateLabel );
								}

								GUILayout.FlexibleSpace();

								if ( GUILayout.Button( "Up", GUILayout.Width( 50f ), GUILayout.Height( 15f ) ) )
								{
									if ( i > 0 )
									{
										int tempKey = fsm.StateKeys[i];
										State tempState = fsm.StateValues[i];

										fsm.StateKeys[i] = fsm.StateKeys[i - 1];
										fsm.StateValues[i] = fsm.StateValues[i - 1];

										fsm.StateKeys[i - 1] = tempKey;
										fsm.StateValues[i - 1] = tempState;
									}
								}
								if ( GUILayout.Button( "Down", GUILayout.Width( 50f ), GUILayout.Height( 15f ) ) )
								{
									if ( i < enumNames.Length - 1 )
									{
										int tempKey = fsm.StateKeys[i];
										State tempState = fsm.StateValues[i];

										fsm.StateKeys[i] = fsm.StateKeys[i + 1];
										fsm.StateValues[i] = fsm.StateValues[i + 1];

										fsm.StateKeys[i + 1] = tempKey;
										fsm.StateValues[i + 1] = tempState;
									}
								}

							if ( GUILayout.Button( "X", GUILayout.Width( 20f ), GUILayout.Height( 20f ) ) )
							{
								fsm.StateKeys.RemoveAt( i );
								fsm.StateValues.RemoveAt( i );
								i--;
								continue;
							}
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();
						{
							if ( isStateExpanded )
							{
								EditorGUILayout.BeginVertical();
								{
									EditorGUILayout.LabelField( "State Enum", GUILayout.Width( 90 ) );
									EditorGUILayout.LabelField( "State", GUILayout.Width( 90 ) );
								}
								EditorGUILayout.EndVertical();

								EditorGUILayout.BeginVertical();
								{
									if ( fsm.StateKeys[i] > enumNames.Length )
										fsm.StateKeys[i] = 1;

									fsm.StateKeys[i] = EditorGUILayout.Popup( fsm.StateKeys[i], enumNames );

									State state = (State)EditorGUILayout.ObjectField( fsm.StateValues[i], typeof( State ), true );
									if ( state != null )
									{
										Type stateType = state.GetType();
										bool contains = false;
										for ( int j = 0; j < fsm.StateValues.Count; j++ )
										{
											if ( i != j && fsm.StateValues[j] != null )
												if ( fsm.StateValues[j].GetType() == stateType )
													contains = true;
										}
										if ( contains == false )
											fsm.StateValues[i] = state;
									}
									else
										fsm.StateValues[i] = null;
								}
								EditorGUILayout.EndVertical();
							}
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.Space();
					}

					EditorGUI.indentLevel--;
				}


				EditorGUILayout.Space();

				fsm.IsTransitionsExpanded = EditorGUILayout.Foldout( fsm.IsTransitionsExpanded, "Transitions" );

				if ( fsm.IsTransitionsExpanded )
				{
					EditorGUI.indentLevel++;

					EditorGUILayout.BeginHorizontal();
					{
						if ( GUILayout.Button( "Add Transition" ) )
						{
							fsm.ValidTransitions.Add( new FSMTransition( 0, 0 ) );
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField( "From" );
						EditorGUILayout.LabelField( "To" );
					}
					EditorGUILayout.EndHorizontal();

					if ( enumType != null )
					{
						for ( int i = 0; i < fsm.ValidTransitions.Count; i++ )
						{
							EditorGUILayout.BeginHorizontal();
							{
								if ( fsm.ValidTransitions[i].From > enumNames.Length )
									fsm.ValidTransitions[i].From = 1;
								if ( fsm.ValidTransitions[i].To > enumNames.Length )
									fsm.ValidTransitions[i].To = 1;

								fsm.ValidTransitions[i].From = EditorGUILayout.Popup( fsm.ValidTransitions[i].From, enumNames );
								fsm.ValidTransitions[i].To = EditorGUILayout.Popup( fsm.ValidTransitions[i].To, enumNames );

								if ( GUILayout.Button( "X", GUILayout.Width( 20f ), GUILayout.Height( 20f ) ) )
								{
									fsm.ValidTransitions.RemoveAt( i );
									i--;
									continue;
								}
							}
							EditorGUILayout.EndHorizontal();

							EditorGUILayout.Space();
						}
					}

					EditorGUI.indentLevel--;
				}
			}
			else
				EditorGUILayout.LabelField( "The enum " + enumType.ToString() + " contains 0 elements.  \n The enum for a state machine must have at least one element." );
		}
		else
		{
			EditorGUILayout.LabelField( "There is no found Enum of type '" + fsm.StateEnumName + "'." );
		}

		EditorGUILayout.Space();

		fsm.IsChildValuesExpanded = EditorGUILayout.Foldout( fsm.IsChildValuesExpanded, "Child Values" );

		if ( fsm.IsChildValuesExpanded )
		{
			EditorGUI.indentLevel++;

			base.OnInspectorGUI();

			EditorGUI.indentLevel--;
		}
	}
}
