using System.Collections.Generic;
using UnityEditor;

public class PropReadOnlyIntListAdapter : PropReadOnlyIntCollectionAdapter, IReadOnlyList<int>
{
	public int this[int index] => _property.GetArrayElementAtIndex( index ).intValue;

	public PropReadOnlyIntListAdapter( SerializedProperty prop ) : base( prop ) { }
}
