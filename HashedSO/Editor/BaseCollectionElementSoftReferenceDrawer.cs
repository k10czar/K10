using UnityEditor;
using UnityEngine;
using static UnityEditor.Sprites.Packer;

[CustomPropertyDrawer(typeof(BaseCollectionElementSoftReference), true)]
public class BaseCollectionElementSoftReferenceDrawer : PropertyDrawer
{
    const int DEEP_LINES = 1;

    const int DISPOSE_WIDTH = 56;
    const int GET_REFERENCE_WIDTH = 95;
    const int STATE_WIDTH = 48;

    [ConstLike] static readonly Color RED_COLOR = Color.Lerp( Color.red, Color.white, .5f );

    public virtual string DebugSuffix( SerializedProperty property ) => "";
    
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
        var editorAssetRefGuid = property.FindPropertyRelative( "_editorAssetRefGuid" );

        UpdateOldRef( editorAssetRefGuid, property.FindPropertyRelative( "_assetHardReference" ) );

        var refIsNull = string.IsNullOrEmpty( editorAssetRefGuid.stringValue );

        var deep = property.isExpanded && !refIsNull;
        var slh = EditorGUIUtility.singleLineHeight;
        
        var refState = property.FindPropertyRelative( "_referenceState" );
        var id = property.FindPropertyRelative( "_id" );

        Object realRef = null;
        var instance = ((BaseCollectionElementSoftReference)property.GetInstance());
        if(instance == null)
        {
            EditorGUI.LabelField( area, ConstsK10.NULL_STRING );
            return;
		}
		var assetType = instance.EDITOR_GetAssetType();
        var path = UnityEditor.AssetDatabase.GUIDToAssetPath( editorAssetRefGuid.stringValue );
        realRef = UnityEditor.AssetDatabase.LoadAssetAtPath( path, assetType );
        
        var color = Color.white;
        if( refIsNull ) color = RED_COLOR;
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
        }

        var specifiedRealRef = (IHashedSO)newRef;
        id.intValue = specifiedRealRef?.HashID ?? -1;
        
        area = area.CutTop( slh );

        EditorGUI.EndFoldoutHeaderGroup();

        if( deep )
        {
            if( Application.isPlaying ) 
            {
                var executionW = DISPOSE_WIDTH + GET_REFERENCE_WIDTH + STATE_WIDTH;
                var ew = Mathf.Min( ( area.width * 2 ) / 5, executionW );
                var executionArea = area.RequestRight( ew );
                area = area.CutRight( ew );

                var state = (EAssetReferenceState)refState.enumValueIndex;
                EditorGUI.BeginDisabledGroup( true );
                EditorGUI.LabelField( executionArea.VerticalSlice( DISPOSE_WIDTH + GET_REFERENCE_WIDTH, executionW, STATE_WIDTH ), state.ToString() );
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup( state == EAssetReferenceState.Empty );
                if( GUI.Button( executionArea.VerticalSlice( 0, executionW, DISPOSE_WIDTH ), "Dispose" ) ) ((BaseCollectionElementSoftReference)property.GetInstance())?.DisposeAsset();
                EditorGUI.EndDisabledGroup();
                
                EditorGUI.BeginDisabledGroup( state != EAssetReferenceState.Empty );
                if( GUI.Button( executionArea.VerticalSlice( DISPOSE_WIDTH, executionW, GET_REFERENCE_WIDTH ), "Get Reference" ) ) ((BaseCollectionElementSoftReference)property.GetInstance())?.GetBaseReference();
                EditorGUI.EndDisabledGroup();
            }
            
            if( specifiedRealRef != null ) EditorGUI.TextField( area, GUIContent.none, $"{specifiedRealRef.GetType().ToString()}{DebugSuffix(property)}[{id.intValue}] => {newRef.name}" );
            else EditorGUI.TextField( area, GUIContent.none, $"NULL{DebugSuffix(property)}[{id.intValue}] => NULL" );
        }
        
        GuiColorManager.Revert();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var editorAssetRefGuid = property.FindPropertyRelative( "_editorAssetRefGuid" );
        var refIsNull = string.IsNullOrEmpty( editorAssetRefGuid.stringValue );
        var slh = EditorGUIUtility.singleLineHeight;
        var deep = property.isExpanded && !refIsNull;
        var lines = 1 + ( deep ? DEEP_LINES : 0 );
        return slh * lines;
    }
}
