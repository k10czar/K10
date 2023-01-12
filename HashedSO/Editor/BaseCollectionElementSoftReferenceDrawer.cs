using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BaseCollectionElementSoftReference), true)]
public class BaseCollectionElementSoftReferenceDrawer : PropertyDrawer
{
    const int DEEP_LINES = 1;

    const int DISPOSE_WIDTH = 56;
    const int GET_REFERENCE_WIDTH = 95;
    const int STATE_WIDTH = 48;

    static Color RED_COLOR = Color.Lerp( Color.red, Color.white, .5f );

    public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
    {
        var hardRef = property.FindPropertyRelative( "_assetHardReference" );
        var deep = property.isExpanded && hardRef.objectReferenceValue != null;
        var slh = EditorGUIUtility.singleLineHeight;
        
        var refState = property.FindPropertyRelative( "_referenceState" );
        var id = property.FindPropertyRelative( "_id" );
        
        var color = Color.white;
        if( hardRef.objectReferenceValue == null ) color = RED_COLOR;
        GuiColorManager.New( color );

        var firstLine = area.RequestTop( slh );
        var labelRect = firstLine.RequestLeft( EditorGUIUtility.labelWidth );
        property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup( firstLine.RequestLeft( EditorGUIUtility.labelWidth ), property.isExpanded, label );
        EditorGUI.ObjectField( firstLine.CutLeft( EditorGUIUtility.labelWidth ), hardRef, GUIContent.none );

        var realRef = (IHashedSO)hardRef.objectReferenceValue;
        id.intValue = realRef?.HashID ?? -1;
        
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
            
            var hRef = hardRef.objectReferenceValue;
            if( hRef != null ) EditorGUI.TextField( area, GUIContent.none, $"{hardRef.objectReferenceValue.GetType().ToString()}[{id.intValue}] => {hardRef.objectReferenceValue.NameOrNull()}" );
            else EditorGUI.TextField( area, GUIContent.none, $"NULL[{id.intValue}] => NULL" );
        }
        
        GuiColorManager.Revert();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var slh = EditorGUIUtility.singleLineHeight;
        var hardRef = property.FindPropertyRelative( "_assetHardReference" );
        var deep = property.isExpanded && hardRef.objectReferenceValue != null;
        var lines = 1 + ( deep ? DEEP_LINES : 0 );
        return slh * lines;
    }
}
