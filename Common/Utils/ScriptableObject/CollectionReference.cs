using UnityEngine;

[System.Serializable]
public abstract class CollectionReference<T> where T : PermanentHashedScriptableObject
{
	[SerializeField] int _referenceHashID;
	[System.NonSerialized] T _reference;

	public CollectionReference( T reference )
	{
		_reference = reference;
		_referenceHashID = _reference.HashID;
	}

	public T Reference
	{
		get
		{
			if( _reference == null ) _reference = Collection[_referenceHashID];
			return _reference;
		}
	}

	public abstract PermanentHashedSOCollection<T> Collection { get; }
}