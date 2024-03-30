using UnityEngine;
using UnityEditor;
#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

[CustomPropertyDrawer(typeof(BaseAssetHybridReference), true)]
public class BaseAssetHybridReferenceDrawer : PropertyDrawer
{
    const string RESOURCES_PATH = "/Resources/";

    const int DEEP_LINES = 1;
    const int EXECUTION_LINES = 1;


    static Color ORANGE_COLOR = Color.Lerp( new Color( 1, .6f, 0 ), Color.white, .5f );
    static Color RED_COLOR = Color.Lerp( Color.red, Color.white, .5f );
    static Color YELLOW_COLOR = Color.Lerp( Color.yellow, Color.white, .5f );
    static Color GREEN_COLOR = Color.Lerp( Color.green, Color.white, .8f );

    void ResetData( SerializedProperty editorAssetRefGuid, SerializedProperty refType, SerializedProperty assetDirectRef, SerializedProperty guid, SerializedProperty resourcesPath )
    {
            refType.enumValueIndex = (int)EAssetReferenceType.DirectReference;
            assetDirectRef.objectReferenceValue = null;
            guid.stringValue = string.Empty;
            resourcesPath.stringValue = string.Empty;
            editorAssetRefGuid.stringValue = string.Empty;
    }

    void AssertReferenceType( Object obj, string path, SerializedProperty editorAssetRefGuid, SerializedProperty refType, SerializedProperty assetDirectRef, SerializedProperty guid, SerializedProperty resourcesPath )
    {
        if( string.IsNullOrEmpty( editorAssetRefGuid.stringValue ) )
        {
            ResetData( editorAssetRefGuid, refType, assetDirectRef, guid, resourcesPath );
            return;
        }

        var resourcesIndex = path.IndexOf( RESOURCES_PATH, System.StringComparison.OrdinalIgnoreCase );
        guid.stringValue = string.Empty;
        
        if( resourcesIndex != -1 )
        {
            var lastDot = path.Length - 1;
            for( ; lastDot >= 0 && path[lastDot] != '.'; lastDot-- ) { }
            var startId = resourcesIndex + RESOURCES_PATH.Length;
            var resourcePath = path.Substring( startId, lastDot - startId );
            resourcesPath.stringValue = resourcePath;
            
            refType.enumValueIndex = (int)EAssetReferenceType.Resources;
            
            assetDirectRef.objectReferenceValue = null;
        }
#if USE_ADDRESSABLES
        else if( obj.IsAssetAddressable() )
        {
            refType.enumValueIndex = (int)EAssetReferenceType.Addressables;
            assetDirectRef.objectReferenceValue = null;
            resourcesPath.stringValue = string.Empty;
            guid.stringValue = editorAssetRefGuid.stringValue;
        }
#endif
        else
        {
            refType.enumValueIndex = (int)EAssetReferenceType.DirectReference;
            assetDirectRef.objectReferenceValue = obj;
            resourcesPath.stringValue = string.Empty;
        }
    }

    public void UpdateOldRef( SerializedProperty editorAssetRefGuid, SerializedProperty hardRef )
    {
        if( hardRef.objectReferenceValue == null ) return;
        var path = AssetDatabase.GetAssetPath( hardRef.objectReferenceValue );
        editorAssetRefGuid.stringValue = AssetDatabase.AssetPathToGUID( path );
        Debug.Log( $"UpdateOldRef( {hardRef.objectReferenceValue.NameOrNull()}, {path}, {editorAssetRefGuid.stringValue} )" );
        hardRef.objectReferenceValue = null;
    }

    public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
    {
        var deep = property.isExpanded;
        var slh = EditorGUIUtility.singleLineHeight;

        var editorAssetRefGuid = property.FindPropertyRelative( "_editorAssetRefGuid" );
        UpdateOldRef( editorAssetRefGuid, property.FindPropertyRelative( "_assetHardReference" ) );
        
        var instance = ((BaseAssetHybridReference)property.GetInstance());
        var assetType = instance.EDITOR_GetAssetType();
        var path = UnityEditor.AssetDatabase.GUIDToAssetPath( editorAssetRefGuid.stringValue );
        var realRef = UnityEditor.AssetDatabase.LoadAssetAtPath( path, assetType );

        var refType = property.FindPropertyRelative( "_referenceType" );
        var guid = property.FindPropertyRelative( "_guid" );
        var resourcesPath = property.FindPropertyRelative( "_resourcesPath" );
        var directRef = property.FindPropertyRelative( "_serializedDirectReference" );
        var refState = property.FindPropertyRelative( "_referenceState" );
        AssertReferenceType( realRef, path, editorAssetRefGuid, refType, directRef, guid, resourcesPath );

        Color color = Color.white;
        if( string.IsNullOrEmpty( editorAssetRefGuid.stringValue ) ) color = RED_COLOR;
        else
        {
            switch( refType.enumValueIndex )
            {
                case (int)EAssetReferenceType.DirectReference: color = ORANGE_COLOR; break;
                case (int)EAssetReferenceType.Resources: color = YELLOW_COLOR; break;
#if USE_ADDRESSABLES
                case (int)EAssetReferenceType.Addressables: color = GREEN_COLOR; break;
#endif
            }
        }
        GuiColorManager.New( color );

        var firstLine = area.RequestTop( slh );
        var labelRect = firstLine.RequestLeft( EditorGUIUtility.labelWidth );
        property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup( firstLine.RequestLeft( EditorGUIUtility.labelWidth ), property.isExpanded, label );

		var newRef = EditorGUI.ObjectField( firstLine.CutLeft( EditorGUIUtility.labelWidth ), GUIContent.none, realRef, assetType, true );
        if( realRef != newRef )
        {
            path = AssetDatabase.GetAssetPath( newRef );
            var newGuidValue = AssetDatabase.AssetPathToGUID( path );
            editorAssetRefGuid.stringValue = newGuidValue;
            guid.stringValue = newGuidValue;
        }

        EditorGUI.EndFoldoutHeaderGroup();

        if( deep )
        {
            if( Application.isPlaying ) 
            {
                var executionArea = area.RequestBottom( slh );

                var state = (EAssetReferenceState)refState.enumValueIndex;
                EditorGUI.BeginDisabledGroup( true );
                EditorGUI.LabelField( executionArea.VerticalSlice( 0, 5 ), state.ToString() );
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup( state == EAssetReferenceState.Empty );
                if( GUI.Button( executionArea.VerticalSlice( 1, 5 ), "Dispose" ) ) instance?.DisposeAsset();
                EditorGUI.EndDisabledGroup();
                
                EditorGUI.BeginDisabledGroup( state != EAssetReferenceState.Empty );
                if( GUI.Button( executionArea.VerticalSlice( 2, 5 ), "PreLoad" ) ) instance?.PreLoad();
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup( state == EAssetReferenceState.Loaded || state == EAssetReferenceState.LoadedNull );
                if( GUI.Button( executionArea.VerticalSlice( 3, 5 ), "Load" ) ) instance?.GetBaseReference();
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup( state == EAssetReferenceState.Loaded );
                if( GUI.Button( executionArea.VerticalSlice( 4, 5 ), "Both" ) ) 
                {
                    var inst = instance;
                    inst?.PreLoad();
                    inst?.GetBaseReference();
                }
                EditorGUI.EndDisabledGroup();

                area = area.CutBottom( slh );
            }

            var nextLines = area.CutTop( slh );

            var line = nextLines;
            
            
            EditorGUI.LabelField( line.RequestLeft( 100 ), ((EAssetReferenceType)refType.enumValueIndex).ToString() );
            
            EditorGUI.TextField( line.CutLeft( 100 ), GUIContent.none, ( refType.enumValueIndex == (int)EAssetReferenceType.Resources ) ? resourcesPath.stringValue : guid.stringValue );
        }

        if( path.Contains( "editor/") )
        {
            var msg = $"Cannot point to an asset on *Editor* Folder!\n!detected! editor folder on the following path:\n{path}";
            Debug.LogError( msg );
            EditorUtility.DisplayDialog( "ERROR", msg, "I will Never do that again!" );
            ResetData( editorAssetRefGuid, refType, directRef, guid, resourcesPath );
        }
        
        GuiColorManager.Revert();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var slh = EditorGUIUtility.singleLineHeight;
        var deep = property.isExpanded;
        var lines = 1 + ( deep ? DEEP_LINES : 0 );
        if( Application.isPlaying && deep ) lines += EXECUTION_LINES;
        return slh * lines;
    }
}
