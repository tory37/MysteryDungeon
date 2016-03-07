using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Implement this to have a functioning finite state machine that is also a monobehaviour.
/// This class, when implemented, provides a Dictionary of Enum to class State, and calls 
/// the update methods of the states in their corresponding Unity functions.
/// This also provides a method for transitioning between states correctly.
/// </summary>
public abstract class MonoFSM : MonoBehaviour
{

	#region Editor Interface;

	[SerializeField, HideInInspector]
	public string StateEnumName;

	[SerializeField, HideInInspector]
	public bool LockEnumName = false;

	[SerializeField, HideInInspector]
	public bool IsStatesExpanded = false;

	[SerializeField, HideInInspector]
	public bool IsTransitionsExpanded = false;

	[SerializeField, HideInInspector]
	public bool IsChildValuesExpanded = false;

	public List<int> StateKeys
	{
		get { return stateKeys; }
		#if UNITY_EDITOR
		set { stateKeys = value;}
		#endif
	}
	[SerializeField, HideInInspector]
	protected List<int> stateKeys = new List<int>();

	public List<State> StateValues
	{
		get { return stateValues; }
		#if UNITY_EDITOR
		set { stateValues = value;}
		#endif
	}
	[SerializeField, HideInInspector]
	protected List<State> stateValues = new List<State>();

	public List<FSMTransition> ValidTransitions
	{
		get { return validTransitions; }
		#if UNITY_EDITOR
		set { validTransitions = value;}
		#endif
	}

	#endregion

	/// <summary>
	/// This is a list of states in the fsm
	/// Each state can easily be referenced by their corresponding enum value.
	/// Best Practice:
	///		Declare the enum within the state machine
	/// </summary>
	protected Dictionary<int, State> states;

	[SerializeField, HideInInspector]
	protected List<FSMTransition> validTransitions = new List<FSMTransition>();

	/// <summary>
	/// This represents what state the machine is currently in
	/// </summary>
	State currentState = null;

	/// <summary>
	/// Calls SetStates, SetTransitions, Initialize, then calls initialize on each state in the fsm. 
	/// </summary>
	protected virtual void Start()
	{
		SetStates();

		Initialize();

		foreach ( State state in states.Values )
		{
			state.Initialize(this);
		}

		currentState = states.Values.ElementAt( 0 );
		currentState.OnEnter();
	}

	/// <summary>
	/// Initialize the dictionary of Enum to States in this function.
	/// This gets called first before the initilazation of the machine itself
	/// </summary>
	private void SetStates()
	{
		states = new Dictionary<int,State>();
		for (int i = 0; i < StateKeys.Count; i++)
		{
			if (stateValues[i] == null)
			{
				stateValues.RemoveAt( i );
				i--;
			}
			states.Add(StateKeys[i], StateValues[i]);
		}
	}

	/// <summary>
	/// This is used for anything that would need to get done at the start of the 
	/// state machine, after the initialization of the machine
	/// </summary>
	protected virtual void Initialize() { }

	/// <summary>
	/// Gets called once per frame
	/// </summary>
	protected void Update()
	{
		currentState.OnUpdate();
	} 

	/// <summary>
	/// Gets called once per frame on a fixed timescale
	/// </summary>
	protected void FixedUpdate()
	{
		currentState.OnFixedUpdate();
	} 

	/// <summary>
	/// Gets called at the end of every frame
	/// </summary>
	protected void LateUpdate()
	{
		currentState.OnLateUpdate();
		currentState.CheckTransitions();
	}

	/// <summary>
	/// Transitions the FSM to the passed state.
	/// </summary>
	/// <param name="toState"></param>
	protected void Transition(Enum toState)
	{
		int toStateInt = Convert.ToInt32( toState );

		currentState.OnExit();
		if ( states.TryGetValue( toStateInt, out currentState ) )
		{
			currentState.OnEnter();
		}
		else
			throw new Exception( "State " + toState + " is not defined, check if your string is correct." );
	}

	/// <summary>
	/// This checks to see if the  
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <returns></returns>
	public bool ContainsTransition(int from, int to)
	{
		return validTransitions.Contains(new FSMTransition(from, to));
	}

	/// <summary>
	/// Use this function as a proxy to do any checks for outside sources 
	/// to makes a transition in the FSM.
	/// </summary>
	public virtual void AttemptTransition(Enum to)
	{
		int fromStateInt = Convert.ToInt32( states.First(m => m.Value == currentState).Key );
		int toStateInt = Convert.ToInt32( to );

		if ( states.ContainsKey( toStateInt ) && ContainsTransition( fromStateInt, toStateInt ) )
			Transition( to );
		else
			Debug.Log( "FSM does not cannot transition to state " + to.ToString() + " from current state: " + currentState.Identifier );
	}

}

/// <summary>
/// Provides a way to keep track of valid transtions int he FSM
/// </summary>
[Serializable]
public class FSMTransition : IEquatable<FSMTransition>
{
	public int From 
	{
		get { return from; }
#if UNITY_EDITOR
		set { from = value; }
#endif
	}
	[SerializeField]
	private int from;

	public int To 
	{
		get { return to; } 
#if UNITY_EDITOR
		set { to = value; }
#endif
	}
	[SerializeField]
	private int to;

	public FSMTransition(int from, int to)
	{
		this.From = from;
		this.To = to;
	}

	public bool Equals( FSMTransition other )
	{
		return this.From == other.From && this.To == other.To;
	}

	public static bool operator ==(FSMTransition left, FSMTransition right)
	{
		return left.Equals( right );
	}

	public static bool operator !=(FSMTransition left, FSMTransition right)
	{
		return !left.Equals( right );
	}
}
