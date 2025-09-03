using System;
using UnityEngine;

public sealed class UnityKeyInputObserver : InputObserver
{
	KeyCode _keyCode;

	public UnityKeyInputObserver( InputGroup group, KeyCode keyCode, bool ignoreFirstEventIfAlreadyTrue ) : base( group, ignoreFirstEventIfAlreadyTrue )
	{
		_keyCode = keyCode;
		Start();
	}

	protected override bool CheckInput() { return Input.GetKey( _keyCode ); }
}

public abstract class InputObserver : IBoolStateObserver, IUpdatableOnDemand
{
	protected readonly BoolState _readedState = new BoolState();
	protected readonly BoolState _state = new BoolState();
	[System.NonSerialized] private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();
	private readonly InputGroup _group;

	bool _cancelOnDisable = true;
	bool _ignoreFirstEventIfAlreadyTrue;
	bool _ignoreNextEvent;

	public bool Value => _state.Value;
	public bool Get() => _state.Get();

	public IEventRegister<bool> OnChange => _state.OnChange;
	public IEventRegister OnTrueState => _state.OnTrueState;
	public IEventRegister OnFalseState => _state.OnFalseState;
	public IBoolStateObserver Not => _not.Request( this );

	public InputObserver( InputGroup group, bool ignoreFirstEventIfAlreadyTrue )
	{
		_group = group;
		_ignoreFirstEventIfAlreadyTrue = ignoreFirstEventIfAlreadyTrue;

		_readedState.OnTrueState.Register( group.Validator.Validated( TriggerOnTrueReadedState ) );
		_readedState.OnFalseState.Register( group.Validator.Validated( TriggerOnFalseReadedState ) );

		group.Validator.OnVoid.Register( new CallOnceCapsule( Kill ) );

		if( _cancelOnDisable ) _group.CanUpdate.OnFalseState.Register( group.Validator.Validated( _readedState.SetFalse ) );
	}

	protected virtual void Kill()
	{
		_readedState?.Kill();
		_state?.Kill();
	}

	void TriggerOnTrueReadedState() { if( !_ignoreNextEvent ) _state.SetTrue(); }
	void TriggerOnFalseReadedState() { if( !_ignoreNextEvent ) _state.SetFalse(); _ignoreNextEvent = false; }

	protected void Start()
	{
		_group.CanUpdate.RegisterOnTrue( OnUpdateEnable );
	}

	void OnUpdateEnable()
	{
		_ignoreNextEvent = _ignoreFirstEventIfAlreadyTrue && CheckInput();
		_group.RequestUpdate( this );
	}

	public bool Update( float delta )
	{
		if( !_group.CanUpdate.Free ) return false;
		_readedState.Value = CheckInput();
		return true;
	}

	protected abstract bool CheckInput();
}