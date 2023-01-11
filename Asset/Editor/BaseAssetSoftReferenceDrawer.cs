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
    static Color GREEN_COLOR = Color.Lerp( Color.green, Color.white, .7f );

    void ResetData( SerializedProperty refType, SerializedProperty hardRef, SerializedProperty assetDirectRef, SerializedProperty guid, SerializedProperty resourcesPath )
    {
            refType.enumValueIndex = (int)EAssetReferenceType.DirectReference;
            hardRef.objectReferenceValue = null;
            assetDirectRef.objectReferenceValue = null;
            guid.stringValue = string.Empty;
            resourcesPath.stringValue = string.Empty;
    }

    void AssertReferenceType( SerializedProperty refType, SerializedProperty hardRef, SerializedProperty assetDirectRef, SerializedProperty guid, SerializedProperty resourcesPath )
    {
        var go = hardRef.objectReferenceValue;
        if( go == null )
        {
            ResetData( refType, hardRef, assetDirectRef, guid, resourcesPath );
            return;
        }

        var path = AssetDatabase.GetAssetPath( hardRef.objectReferenceValue );
        guid.stringValue = AssetDatabase.AssetPathToGUID( path );
        
        var resourcesIndex = path.IndexOf( RESOURCES_PATH, System.StringComparison.OrdinalIgnoreCase );
        if( resourcesIndex != -1 )
        {
            var lastDot = path.Length - 1;
            for( ; lastDot >= 0 && path[lastDot] != '.'; lastDot-- ) { }
            var startId = resourcesIndex + RESOURCES_PATH.Length;
            var resourcePath = path.Substring( startId, lastDot - startId );
            resourcesPath.stringValue = resourcePath;
            
            if( refType.enumValueIndex != (int)EAssetReferenceType.Resources )
            {
                refType.enumValueIndex = (int)EAssetReferenceType.Resources;
            }
            
            assetDirectRef.objectReferenceValue = null;
        }
#if USE_ADDRESSABLES
        else if( hardRef.objectReferenceValue.IsAssetAddressable() )
        {
            refType.enumValueIndex = (int)EAssetReferenceType.Addressables;
            assetDirectRef.objectReferenceValue = null;
            resourcesPath.stringValue = string.Empty;
        }
#endif
        else
        {
            if( refType.enumValueIndex != (int)EAssetReferenceType.DirectReference )
            {
                refType.enumValueIndex = (int)EAssetReferenceType.DirectReference;
            }
            assetDirectRef.objectReferenceValue = hardRef.objectReferenceValue;
            resourcesPath.stringValue = string.Empty;
        }
    }

    public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
    {
        var deep = property.isExpanded;
        var slh = EditorGUIUtility.singleLineHeight;

        var refType = property.FindPropertyRelative( "_referenceType" );
        var hardRef = property.FindPropertyRelative( "_assetHardReference" );
        var guid = property.FindPropertyRelative( "_guid" );
        var resourcesPath = property.FindPropertyRelative( "_resourcesPath" );
        var directRef = property.FindPropertyRelative( "_serializedDirectReference" );
        var refState = property.FindPropertyRelative( "_referenceState" );

        AssertReferenceType( refType, hardRef, directRef, guid, resourcesPath );

        Color color = Color.white;
        if( hardRef.objectReferenceValue == null ) color = RED_COLOR;
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
        EditorGUI.ObjectField( firstLine.CutLeft( EditorGUIUtility.labelWidth ), hardRef, GUIContent.none );
        EditorGUI.EndFoldoutHeaderGroup();

        if( deep )
        {
            if( Application.isPlaying ) 
            {
                var executionArea = area.RequestBottom( slh );

                EditorGUI.BeginDisabledGroup( true );
                EditorGUI.PropertyField( executionArea.VerticalSlice( 0, 5 ), refState, GUIContent.none );
                EditorGUI.EndDisabledGroup();

                var state = (EAssetReferenceState)refState.enumValueIndex;

                EditorGUI.BeginDisabledGroup( state == EAssetReferenceState.Empty );
                if( GUI.Button( executionArea.VerticalSlice( 1, 5 ), "Dispose" ) ) ((BaseAssetHybridReference)property.GetInstance())?.DisposeAsset();
                EditorGUI.EndDisabledGroup();
                
                EditorGUI.BeginDisabledGroup( state != EAssetReferenceState.Empty );
                if( GUI.Button( executionArea.VerticalSlice( 2, 5 ), "PreLoad" ) ) ((BaseAssetHybridReference)property.GetInstance())?.PreLoad();
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup( state == EAssetReferenceState.Loaded || state == EAssetReferenceState.LoadedNull );
                if( GUI.Button( executionArea.VerticalSlice( 3, 5 ), "Load" ) ) ((BaseAssetHybridReference)property.GetInstance())?.GetReference();
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup( state == EAssetReferenceState.Loaded );
                if( GUI.Button( executionArea.VerticalSlice( 4, 5 ), "Both" ) ) 
                {
                    var inst = ((BaseAssetHybridReference)property.GetInstance());
                    inst?.PreLoad();
                    inst?.GetReference();
                }
                EditorGUI.EndDisabledGroup();

                area = area.CutBottom( slh );
            }

            var nextLines = area.CutTop( slh );

            var line = nextLines;
            
            EditorGUI.BeginDisabledGroup( true );
            EditorGUI.PropertyField( line.RequestLeft( 120 ), refType, GUIContent.none );
            EditorGUI.EndDisabledGroup();
            EditorGUI.TextField( line.CutLeft( 120 ), GUIContent.none, ( refType.enumValueIndex == (int)EAssetReferenceType.Resources ) ? resourcesPath.stringValue : guid.stringValue );
        }

        var path = AssetDatabase.GetAssetPath( hardRef.objectReferenceValue );
        if( path.Contains( "editor/", System.StringComparison.OrdinalIgnoreCase ) )
        {
            var msg = $"Cannot point to an asset on *Editor* Folder!\n!detected! editor folder on the following path:\n{path}";
            Debug.LogError( msg );
            EditorUtility.DisplayDialog( "ERROR", msg, "I will Never do that again!" );
            ResetData( refType, hardRef, directRef, guid, resourcesPath );
        }

        // base.OnGUI( area, property, label );
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
