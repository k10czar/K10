using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface IValueCapsule<T>
{
	T Get { get; }
	T Set { set; }
}

//TO DO: Manage to update save on class fields an properties changes
public class Persistent<T> : IValueCapsule<T> where T : class
{
	string _realitvePath;
	T _defaultValue = null;
	T _t = null;
	bool _readed = false;

	public string PathToUse => FullPath( _realitvePath );
	public static string FullPath( string realitvePath ) => FileAdapter.persistentDataPath + "/" + realitvePath;

	static Dictionary<string, Persistent<T>> _dict = new Dictionary<string, Persistent<T>>();

	public static Persistent<T> At( string realitvePath, T defaultValue = default(T) )
	{
		Persistent<T> val;
		if( !_dict.TryGetValue( realitvePath, out val ) )
		{
			val = new Persistent<T>( realitvePath, defaultValue );
			_dict[realitvePath] = val;
		}
		return val;
	}

	Persistent( string realitvePath, T defaultValue = default(T) )
	{
		_defaultValue = defaultValue;
		_realitvePath = realitvePath;
	}

	public static void Clear( string realitvePath )
	{
		_dict.Remove( realitvePath );
		var fullPath = FullPath( realitvePath );
		if( FileAdapter.Exists( fullPath ) ) FileAdapter.Delete( fullPath );
	}

	public T Get
	{
		get
		{
			if( !_readed )
			{
				_readed = true;
				_t = _defaultValue;
				if( FileAdapter.Exists( PathToUse ) )
				{
					var readedData = FileAdapter.ReadAllBytes( PathToUse );
					if( readedData != null )
						_t = BinaryAdapter.Deserialize<T>( readedData );
				}
			}

			return (T)_t;
		}
	}

	public T Set
	{
		set
		{
			if( _t == null || !( (T)_t ).Equals( value ) )
			{
				_t = value;
				if( _t == null ) FileAdapter.Delete( PathToUse );
				else FileAdapter.WriteAllBytes( PathToUse, BinaryAdapter.Serialize( value ) );
			}
		}
	}
}


public class PersistentValue<T> : IValueCapsule<T> where T : struct, System.IComparable
{
	string _realitvePath;
	T? _t = null;

	public string PathToUse => FullPath( _realitvePath );
	static string FullPath( string realitvePath ) => "Editor/" + realitvePath;

	static Dictionary<string, PersistentValue<T>> _dict = new Dictionary<string, PersistentValue<T>>();

	public static bool Exists( string realitvePath ) { return _dict.ContainsKey( realitvePath ) || FileAdapter.Exists( FullPath( realitvePath ) ); }

	public static PersistentValue<T> At( string realitvePath, T startValue )
	{
		var has = Exists( realitvePath );
		var ret = At( realitvePath );
		if( !has ) ret.Set = startValue;
		return ret;
	}

	public static PersistentValue<T> At( string realitvePath )
	{
		if( !_dict.TryGetValue( realitvePath, out var val ) )
		{
			val = new PersistentValue<T>( realitvePath );
			_dict[realitvePath] = val;
		}
		return val;
	}

	PersistentValue( string realitvePath )
	{
		_realitvePath = realitvePath;
	}

	public T Get
	{
		get
		{
			if( _t == null )
			{
				_t = default( T );
				var fullPath = PathToUse;
				if( FileAdapter.Exists( fullPath ) )
				{
					var readedData = FileAdapter.ReadAllBytes( fullPath );
					if( readedData != null )
						_t = BinaryAdapter.Deserialize<T>( readedData );
				}
			}

			return (T)_t;
		}
	}

	public T Set
	{
		set
		{
			if( _t == null || !( (T)_t ).Equals( value ) )
			{
				_t = value;
				// if( default( T ).Equals( value ) ) FileAdapter.Delete( PathToUse );
				// else 
				FileAdapter.WriteAllBytes( PathToUse, BinaryAdapter.Serialize( value ) );
			}
		}
	}
}


public interface ISettingsValue
{
	void ResetDefault();
	void SaveUndo();
	void Undo();
}

public class PersistentBoolState : IBoolState, ISettingsValue
{
	bool _defaltValue;
	bool _undoValue;
	PersistentValueState<bool> _persistentValueState;

	EventSlot _onTrueState;
	EventSlot _onFalseState;

	private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	static Dictionary<string, PersistentBoolState> _dict = new Dictionary<string, PersistentBoolState>();

	public IEventRegister OnTrueState => Lazy.Request( ref _onTrueState );
	public IEventRegister OnFalseState => Lazy.Request( ref _onFalseState );

	public bool Value => _persistentValueState.Value;
	public IEventRegister<bool> OnChange => _persistentValueState.OnChange;

	public IBoolStateObserver Not => _not.Request( this );

	private PersistentBoolState( string path, bool initialValue )
	{
		_persistentValueState = PersistentValueState<bool>.At( path, initialValue );
		_undoValue = _defaltValue = initialValue;
		_persistentValueState.OnChange.Register( OnValueChange );
	}

	private void OnValueChange( bool value )
	{
		if( value ) _onTrueState?.Trigger();
		else _onFalseState?.Trigger();
	}

	public static PersistentBoolState At( string path, bool startValue = default( bool ) )
	{
		PersistentBoolState val;
		if( !_dict.TryGetValue( path, out val ) )
		{
			val = new PersistentBoolState( path, startValue );
			_dict[path] = val;
		}
		return val;
	}

	public void ResetDefault() { Setter( _defaltValue ); }
	public void SaveUndo() { _undoValue = Value; }
	public void Undo() { Setter( _undoValue ); }

	public void Setter( bool t ) => _persistentValueState.Setter( t );
	public bool Get() => _persistentValueState.Get();
}

[System.Serializable]
public class PersistentValueState<T> : ValueState<T>, ISettingsValue where T : struct, System.IComparable
{
	T _defaltValue;
	T _undoValue;
	PersistentValue<T> _persistentData;

	static Dictionary<string, PersistentValueState<T>> _dict = new Dictionary<string, PersistentValueState<T>>();

	public static PersistentValueState<T> At( string path, T startValue = default( T ) )
	{
		PersistentValueState<T> val;
		if( !_dict.TryGetValue( path, out val ) )
		{
			val = new PersistentValueState<T>( path, startValue );
			_dict[path] = val;
		}
		return val;
	}


	protected PersistentValueState( string path, T initialValue )
	{
		_persistentData = PersistentValue<T>.At( path, initialValue );
		_undoValue = _defaltValue = initialValue;
		Value = _persistentData.Get;
		OnChange.Register( OnValueChange );
	}

	private void OnValueChange( T value ) { _persistentData.Set = value; }

	public void ResetDefault() { Setter( _defaltValue ); }
	public void SaveUndo() { _undoValue = Value; }
	public void Undo() { Setter( _undoValue ); }

	public override string ToString() { return string.Format( $"PVS<{typeof( T )}>({Value})" ); }
}