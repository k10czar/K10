using System.Collections.Generic;
using UnityEditor;

public class PropReadOnlyIntCollectionAdapter : PropIntEnumerableAdapter, IReadOnlyCollection<int>
{
	public int Count => _property.arraySize;
	public PropReadOnlyIntCollectionAdapter( SerializedProperty prop ) : base( prop ) { }
}
