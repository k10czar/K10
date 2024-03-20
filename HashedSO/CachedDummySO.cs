using UnityEngine;

public static class CachedDummySO<T> where T : UnityEngine.ScriptableObject
{
	private static T _dummyInstance = null;
	public static T Instance => _dummyInstance ?? ( _dummyInstance = ScriptableObject.CreateInstance<T>() );
}
