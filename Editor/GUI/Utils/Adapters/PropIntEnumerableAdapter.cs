using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class PropIntEnumerableAdapter : IEnumerable, IEnumerable<int>
{
	protected readonly SerializedProperty _property;

	public IEnumerator<int> GetEnumerator()
	{
		for( int i = 0; i < _property.arraySize; i++ )
		{
			yield return _property.GetArrayElementAtIndex( i ).intValue;
		}
	}

	public PropIntEnumerableAdapter( SerializedProperty prop )
	{
		_property = prop;
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
