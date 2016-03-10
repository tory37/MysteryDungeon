using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Implement this to make a class a state
/// </summary>
public abstract class State : MonoBehaviour
{
	#region FSM Inspector Variables

	[SerializeField, HideInInspector]
	public bool IsStateExpanded = false;

	#endregion

	public string Identifier
	{
		get { return identifier; }
#if UNITY_EDITOR
		set { identifier = value; }
#endif
	}
	[SerializeField]
	private string identifier;

	/// <summary>
	/// ** Nothing in Base. **
	/// Use this function to do anything you would need to do in
	/// start for this state, and also to set a reference to the 
	/// FSM if you need.
	/// </summary>
	/// <param name="callingfsm"></param>
    public virtual void Initialize(MonoFSM callingfsm) { }

	/// <summary>
	/// /// Gets called when the machine transitions into this state
	/// ** Nothing in Base. **
	/// </summary>
    public virtual void OnEnter() { }

	/// <summary>
	/// Gets called every Unity Update
	/// ** Nothing in Base. **
	/// </summary>
    public virtual void OnUpdate() { }

	/// <summary>
	/// Get called every Unity FixedUpdate
	/// ** Nothing in Base. **
	/// </summary>
    public virtual void OnFixedUpdate() { }

	/// <summary>
	/// Get called ever Unity LateUpdate
	/// ** Nothing in Base. **
	/// </summary>
    public virtual void OnLateUpdate() { }

	/// <summary>
	/// Get called when an fsm leaves this state
	/// ** Nothing in Base. **
	/// </summary>
    public virtual void OnExit() { }

	/// <summary>
	/// Gets called at the end of LateUpdate,
	/// this is where you put your if else statements for transtioning
	/// ** Nothing in Base. **
	/// </summary>
    public virtual void CheckTransitions() { }
}