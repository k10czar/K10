using UnityEngine;

public abstract class SerializablePermanentHashedSOReference<T> : BaseSerializablePermanentHashedSOReference<T>
	 															where T : HashedScriptableObject, new()
{
	private static readonly T _dummyInstance = ScriptableObject.CreateInstance<T>();
	// HACK to get the collection that you only can generic get with a class instance

	public SerializablePermanentHashedSOReference( T reference ) : base( reference ) { }

	protected override IHashedSOCollection GetCollection()
	{
		// if( _dummyInstance == null ) _dummyInstance = new T();
		return _dummyInstance.GetCollection();
	}
}

[System.Serializable]
public abstract class BaseSerializablePermanentHashedSOReference<T> where T : HashedScriptableObject
{
	[SerializeField] int _referenceHashID;
	[System.NonSerialized] T _reference;

	public BaseSerializablePermanentHashedSOReference( T reference )
	{
		_reference = reference;
		_referenceHashID = _reference.HashID;
	}

	protected abstract IHashedSOCollection GetCollection();

	public T Reference
	{
		get
		{
			if( _reference == null ) _reference = (T)GetCollection().GetElementBase( _referenceHashID );
			return _reference;
		}
	}
}