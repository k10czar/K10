using UnityEngine;

public static class HsoRefExtension
{
	public static bool NeedMigration<T>( this HsoRef<T> hsoRef, T t ) where T : HashedScriptableObject
	{
		var need = hsoRef == null || ( t != null && hsoRef.ReferenceHashID != t.HashID ) || ( t == null && hsoRef.ReferenceHashID != -1 );
		// if( need ) Debug.Log( $"Need Migration on {hsoRef.ToStringOrNull()} from {t.ToStringOrNull()}" );
		return need;
	}
}

[System.Serializable]
public class HsoRef<T> : IReferenceOf<T> where T : HashedScriptableObject
{
	[SerializeField] int _referenceHashID = -1;
	[System.NonSerialized] T _reference;

	private static T _dummyInstance;
	private static T DummyInstance => _dummyInstance != null ? _dummyInstance : ( _dummyInstance = ScriptableObject.CreateInstance<T>() );

	public int ReferenceHashID => _referenceHashID;

	public T Reference
	{
		get
		{
			if( _reference == null && _referenceHashID >= 0 ) _reference = (T)GetCollection().GetElementBase( _referenceHashID );
			return _reference;
		}
	}
	
	public HsoRef() :this(-1) {}

	public HsoRef( T reference )
	{
		_reference = reference;
		_referenceHashID = _reference != null ? _reference.HashID : -1;
	}

	public HsoRef( int hashId )
	{
		_reference = null;
		_referenceHashID = hashId;
	}

	public void Set( T refrence )
	{
		_reference = refrence;
		_referenceHashID = refrence.HashID;
	}

	public void Set( int id )
	{
		_reference = GetCollection().GetElementBase( id ) as T;
		_referenceHashID = id;
	}

	protected virtual IHashedSOCollection GetCollection()
	{
		// if( _dummyInstance == null ) _dummyInstance = new T();
		return DummyInstance.GetCollection();
	}

	public override string ToString() => $"[{_referenceHashID}]=>{_reference.ToStringOrNull()}({_reference.NameOrNull()})";
}