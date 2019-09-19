using UnityEngine;

[System.Serializable]
public abstract class SerializablePermanentHashedSOReference<T> where T : PermanentHashedScriptableObject, new()
{
	private static readonly T _dummyInstance = new T();
	// HACK to get the collection that you only can generic get with a class instance

	[SerializeField] int _referenceHashID;
	[System.NonSerialized] T _reference;

	public SerializablePermanentHashedSOReference( T reference )
	{
		_reference = reference;
		_referenceHashID = _reference.HashID;
	}

	public SerializablePermanentHashedSOReference() { }

	public T Reference
	{
		get
		{
			if( _reference == null )
			{
				// if( _dummyInstance == null ) _dummyInstance = new T();
				_reference = (T)_dummyInstance.GetCollection().GetElementBase( _referenceHashID );
			}
			return _reference;
		}
	}
}