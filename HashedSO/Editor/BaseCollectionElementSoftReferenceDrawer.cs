using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BaseCollectionElementSoftReference), true)]
public class BaseCollectionElementSoftReferenceDrawer : PropertyDrawer
{
    const int DEEP_LINES = 1;
    const int EXECUTION_LINES = 1;

    static Color RED_COLOR = Color.Lerp( Color.red, Color.white, .5f );
    static Color GREEN_COLOR = Color.Lerp( Color.green, Color.white, .9f );

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

        EditorGUI.EndFoldoutHeaderGroup();

        if( deep )
        {
            if( Application.isPlaying ) 
            {
                var executionArea = area.RequestBottom( slh );

                var state = (EAssetReferenceState)refState.enumValueIndex;
                EditorGUI.BeginDisabledGroup( true );
                EditorGUI.LabelField( executionArea.VerticalSlice( 0, 3 ), state.ToString() );
                EditorGUI.EndDisabledGroup();


                EditorGUI.BeginDisabledGroup( state == EAssetReferenceState.Empty );
                if( GUI.Button( executionArea.VerticalSlice( 1, 3 ), "Dispose" ) ) ((BaseCollectionElementSoftReference)property.GetInstance())?.DisposeAsset();
                EditorGUI.EndDisabledGroup();
                
                EditorGUI.BeginDisabledGroup( state != EAssetReferenceState.Empty );
                if( GUI.Button( executionArea.VerticalSlice( 2, 3 ), "Get Reference" ) ) ((BaseCollectionElementSoftReference)property.GetInstance())?.GetBaseReference();
                EditorGUI.EndDisabledGroup();

                area = area.CutBottom( slh );
            }

            var nextLines = area.CutTop( slh );
            var line = nextLines;
            
            var hRef = hardRef.objectReferenceValue;
            if( hRef != null ) EditorGUI.TextField( line, GUIContent.none, $"{hardRef.objectReferenceValue.GetType().ToString()}[{id.intValue}] => {hardRef.objectReferenceValue.NameOrNull()}" );
            else EditorGUI.TextField( line, GUIContent.none, $"NULL[{id.intValue}] => NULL" );
        }
        
        GuiColorManager.Revert();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var slh = EditorGUIUtility.singleLineHeight;
        var hardRef = property.FindPropertyRelative( "_assetHardReference" );
        var deep = property.isExpanded && hardRef.objectReferenceValue != null;
        var lines = 1 + ( deep ? DEEP_LINES : 0 );
        if( Application.isPlaying && deep ) lines += EXECUTION_LINES;
        return slh * lines;
    }
}
