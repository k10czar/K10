
public static class StoreGuid
{
    public static void At( UnityEditor.SerializedProperty property )
    {
        var rootObject = property.serializedObject.targetObject;
        var path = UnityEditor.AssetDatabase.GetAssetPath( rootObject );
        var guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
        if( property.stringValue != guid ) property.stringValue = guid;
    }

    public static bool At( ref string guidField, UnityEngine.Object obj )
    {
        var path = UnityEditor.AssetDatabase.GetAssetPath( obj );
        var guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
        if( guidField == guid ) return false;
        guidField = guid;
        return true;
    }
}

public static class StoreFileID
{
    public static void At( UnityEditor.SerializedProperty property )
    {
        var rootObject = property.serializedObject.targetObject;
        var id = rootObject.GetInstanceID();
        if( property.longValue != id ) property.intValue = id;
    }

    public static bool At( ref ulong idField, UnityEngine.Object obj )
    {
        var id = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow( obj ).targetObjectId;
        if( idField == id ) return false;
        idField = id;
        return true;
    }
}
