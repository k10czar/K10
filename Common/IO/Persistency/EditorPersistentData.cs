using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EditorPersistentBoolState : IBoolState, ISettingsValue
{
	bool _defaltValue;
	bool _undoValue;
	EditorPersistentValueState<bool> _persistentValueState;

	EventSlot _onTrueState;
	EventSlot _onFalseState;

	private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	static Dictionary<string, EditorPersistentBoolState> _dict = new();

	public IEventRegister OnTrueState => Lazy.Request( ref _onTrueState );
	public IEventRegister OnFalseState => Lazy.Request( ref _onFalseState );

	public bool Value => _persistentValueState.Value;
	public IEventRegister<bool> OnChange => _persistentValueState.OnChange;

	public IBoolStateObserver Not => _not.Request( this );

	private EditorPersistentBoolState( string path, bool initialValue )
	{
		_persistentValueState = EditorPersistentValueState<bool>.At( path, initialValue );
		_undoValue = _defaltValue = initialValue;
		_persistentValueState.OnChange.Register( OnValueChange );
	}

	private void OnValueChange( bool value )
	{
		if( value ) _onTrueState?.Trigger();
		else _onFalseState?.Trigger();
	}

	public static EditorPersistentBoolState At( string path, bool startValue = default( bool ) )
	{
		EditorPersistentBoolState val;
		if( !_dict.TryGetValue( path, out val ) )
		{
			val = new EditorPersistentBoolState( path, startValue );
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
public class EditorPersistentValueState<T> : ValueState<T>, ISettingsValue where T : struct, System.IComparable
{
	T _defaltValue;
	T _undoValue;
	EditorPersistentValue<T> _persistentData;

	static Dictionary<string, EditorPersistentValueState<T>> _dict = new Dictionary<string, EditorPersistentValueState<T>>();

	public static EditorPersistentValueState<T> At( string path, T startValue = default( T ) )
	{
		EditorPersistentValueState<T> val;
		if( !_dict.TryGetValue( path, out val ) )
		{
			val = new EditorPersistentValueState<T>( path, startValue );
			_dict[path] = val;
		}
		return val;
	}


	protected EditorPersistentValueState( string path, T initialValue )
	{
		_persistentData = EditorPersistentValue<T>.At( path, initialValue );
		_undoValue = _defaltValue = initialValue;
		Value = _persistentData.Get;
		OnChange.Register( OnValueChange );
	}

	private void OnValueChange( T value ) { _persistentData.Set = value; }

	public void ResetDefault() { Setter( _defaltValue ); }
	public void SaveUndo() { _undoValue = Value; }
	public void Undo() { Setter( _undoValue ); }

	public override string ToString() { return string.Format( $"EPVS<{typeof( T )}>({Value})" ); }
}

public class EditorPersistentValue<T> : IValueCapsule<T> where T : struct, System.IComparable
{
	string _realitvePath;
	T? _t = null;

	public string PathToUse => FullPath( _realitvePath );
	static string FullPath( string realitvePath ) => "Editor/" + realitvePath;

	static Dictionary<string, EditorPersistentValue<T>> _dict = new Dictionary<string, EditorPersistentValue<T>>();

	public static bool Exists( string realitvePath ) { return _dict.ContainsKey( realitvePath ) || File.Exists( FullPath( realitvePath ) ); }

	public static EditorPersistentValue<T> At( string realitvePath, T startValue )
	{
		var has = Exists( realitvePath );
		var ret = At( realitvePath );
		if( !has ) ret.Set = startValue;
		return ret;
	}

	public static EditorPersistentValue<T> At( string realitvePath )
	{
		if( !_dict.TryGetValue( realitvePath, out var val ) )
		{
			val = new EditorPersistentValue<T>( realitvePath );
			_dict[realitvePath] = val;
		}
		return val;
	}

	EditorPersistentValue( string realitvePath )
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
				if( File.Exists( fullPath ) )
				{
					var readedData = File.ReadAllBytes( fullPath );
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
				// if( default( T ).Equals( value ) ) File.Delete( PathToUse );
				// else 
				var filePath = PathToUse;
				int pathFileDivisor = filePath.Length - 1;
				while( pathFileDivisor > 0 && filePath[pathFileDivisor] != '/' && filePath[pathFileDivisor] != '\\' )
					pathFileDivisor--;
				var dir = filePath.Substring( 0, pathFileDivisor );
				if( !Directory.Exists( dir ) ) Directory.CreateDirectory( dir );
				File.WriteAllBytes( PathToUse, BinaryAdapter.Serialize( value ) );
			}
		}
	}
}