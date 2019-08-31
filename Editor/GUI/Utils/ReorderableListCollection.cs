using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

public sealed class ReorderableListCollection
{
    readonly Dictionary<string, ReorderableList> _dict = new Dictionary<string, ReorderableList>();

    public ReorderableList Request( SerializedProperty prop, System.Func<SerializedProperty, ReorderableList> listCreator )
    {
        var key = prop.propertyPath;
        ReorderableList list;

        if( !_dict.TryGetValue( key, out list ) )
        {
            list = listCreator( prop );
            _dict[key] = list;
        }

        return list;
    }
}