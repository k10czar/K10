using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( HashedScriptableObject ), true )]
public class HashedScriptableObjectEditor : Editor
{
	SerializedProperty _hashIdProp;
	SerializedProperty _guidProp;

	protected virtual void OnEnable()
	{
		_hashIdProp = serializedObject.FindProperty( "_hashId" );
		_guidProp = serializedObject.FindProperty( "_guid" );
	}

	protected void DrawValidationElementGUI()
	{
		K10.EditorGUIExtention.SeparationLine.Horizontal();

		bool valid = true;
		for( int i = 0; i < targets.Length; i++ )
		{
			var t = targets[i] as HashedScriptableObject;
			var col = t.GetCollection();
			valid &= col.Contains( t );
		}

		GUILayout.BeginHorizontal();

		K10.EditorGUIExtention.IconButton.Layout( 25, valid ? "greenLight" : "redLight" );

		GUILayout.BeginVertical();
		GUILayout.Space( 2 );
		GUILayout.BeginHorizontal();
		var hashs = string.Join( ", ", targets.ToList().ConvertAll( ( t ) => ( t as HashedScriptableObject ).HashID.ToString() ).ToArray() );
		GUILayout.Label( "HashID: " + hashs, K10GuiStyles.boldStyle );
		if( GUILayout.Button( "Select Collection", GUILayout.Width( 120f ) ) )
		{
			var t = target as HashedScriptableObject;
			Selection.activeObject = t.GetCollection() as UnityEngine.Object;
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		K10.EditorGUIExtention.SeparationLine.Horizontal();
	}

    public static void DrawIconTexture(SerializedObject obj, Sprite icon)
    {
        if (obj == null || icon == null) return;

        var rect = GUILayoutUtility.GetRect(64f, 64f);
        EditorGUI.DrawTextureTransparent(rect, icon.texture, ScaleMode.ScaleToFit);
    }

    public static void DrawIconTexture(SerializedObject obj, SerializedProperty iconProp)
    {
        if (iconProp != null && iconProp.objectReferenceValue != null)
        {
            var rect = GUILayoutUtility.GetRect(64f, 64f);
            EditorGUI.DrawTextureTransparent(rect, ((Sprite)iconProp.objectReferenceValue).texture, ScaleMode.ScaleToFit);
        }
    }

	public override void OnInspectorGUI()
	{
		DrawValidationElementGUI();
		DrawDefaultInspector();
	}
}