using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

public class ReorderableListCache<Key>
{
    readonly Dictionary<Key, ReorderableList> _dict = new Dictionary<Key, ReorderableList>();

    public ReorderableList Request( Key key, System.Func<Key, ReorderableList> listCreator )
    {
        ReorderableList list;
        if( !_dict.TryGetValue( key, out list ) )
        {
            list = listCreator( key );
            _dict[key] = list;
        }

        return list;
    }
}

public sealed class ReorderableListCollection : ReorderableListCache<SerializedProperty>
{
}