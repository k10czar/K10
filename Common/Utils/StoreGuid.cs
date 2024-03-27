
public static class StoreGuid
{
#if UNITY_EDITOR
    public static void At( UnityEditor.SerializedProperty property )
    {
        var rootObject = property.serializedObject.targetObject;
        var path = UnityEditor.AssetDatabase.GetAssetPath( rootObject );
        var guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
        if( property.stringValue != guid ) property.stringValue = guid;
    }
#endif //UNITY_EDITOR

    public static bool At( ref string guidField, UnityEngine.Object obj )
    {
#if UNITY_EDITOR
        var path = UnityEditor.AssetDatabase.GetAssetPath( obj );
        var guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
        if( guidField == guid ) return false;
        guidField = guid;
        return true;
#else //!UNITY_EDITOR
        return false;
#endif //UNITY_EDITOR
    }
}

public static class StoreFileID
{
#if UNITY_EDITOR
    public static void At( UnityEditor.SerializedProperty property )
    {
        var rootObject = property.serializedObject.targetObject;
        var id = rootObject.GetInstanceID();
        if( property.longValue != id ) property.intValue = id;
    }
#endif //UNITY_EDITOR

    public static bool At( ref ulong idField, UnityEngine.Object obj )
    {
#if UNITY_EDITOR
        var id = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow( obj ).targetObjectId;
        if( idField == id ) return false;
        idField = id;
        return true;
#else //!UNITY_EDITOR
        return false;
#endif //UNITY_EDITOR
    }
}
