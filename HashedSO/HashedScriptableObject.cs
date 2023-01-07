using System;
using UnityEngine;

#if UNITY_EDITOR
public interface IHashedSOEditor
{
	string GUID { get; }
	void SetHashID( int newID );
}
#endif

public interface IExportIgnorable
{
	bool Ignore { get; }
}

public interface IHashedSO
#if UNITY_EDITOR
: IHashedSOEditor
#endif
{
	int HashID { get; }
	IHashedSOCollection GetCollection();
}

public abstract class HashedScriptableObject : ScriptableObject, IHashedSO, IExportIgnorable
{
	[SerializeField] private bool _ignoreFromExport = false;
	[HideInInspector, SerializeField] int _hashId = -1;

	#if UNITY_EDITOR
	[HideInInspector, SerializeField] string _guid;
	public string GUID { get { return _guid; } }
	#endif

	public int HashID { get { return _hashId; } }

    public abstract IHashedSOCollection GetCollection();

#if UNITY_EDITOR
	void IHashedSOEditor.SetHashID( int newID )
	{
		_hashId = newID;
		var path = UnityEditor.AssetDatabase.GetAssetPath( this );
		_guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
		UnityEditor.EditorUtility.SetDirty( this );
	}
#endif

	public override bool Equals( object obj )
	{
		return !object.ReferenceEquals( obj, null ) && ( obj is HashedScriptableObject hso ) && ( GetType() == obj.GetType() && _hashId == hso._hashId );
		// if( obj == null ) return false;
		// if( !( obj is HashedScriptableObject hso ) ) return false;
		// if( GetType() != obj.GetType() ) return false;
		// return _hashId == hso._hashId;
	}

    public static bool operator ==( HashedScriptableObject a, HashedScriptableObject b )
	{
		var aNull = object.ReferenceEquals( a, null );
		var bNull = object.ReferenceEquals( b, null );

		return !( aNull ^ bNull ) && ( aNull || ( a._hashId == b._hashId && a.GetType() == b.GetType() ) );

		// if( aNull && bNull ) return true;
		// if( aNull ^ bNull ) return false;
		// if( a.GetType() != b.GetType() ) return false;
		// return a._hashId == b._hashId;
	}

	public static bool operator !=( HashedScriptableObject a, HashedScriptableObject b ) => !( a == b );

	public override int GetHashCode()
	{
		return ( GetType().GetHashCode() << 10 ) + _hashId;
	}

	public override string ToString() => $"{name}[{HashID}]";
	public bool Ignore => _ignoreFromExport;
}

public static class HashedSOExtentions
{
	public static int HashIdOrNull( this IHashedSO hso ) => ( hso != null ) ? hso.HashID : -1;
}