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

		GUILayout.BeginHorizontal();

		bool valid = true;
		for( int i = 0; i < targets.Length; i++ )
		{
			var t = targets[i] as HashedScriptableObject;
			var col = t.GetCollection();
			valid &= col.Contains( t );
		}

		if( K10.EditorGUIExtention.IconButton.Layout( 40, valid ? "greenLight" : "redLight" ) )
		{
			for( int i = 0; i < targets.Length; i++ )
			{
				var t = targets[i] as HashedScriptableObject;
				var col = t.GetCollection();
				col.RequestMember( t );
				t.CheckIntegrity();

				EditorUtility.SetDirty( (ScriptableObject)col );
			}
		}

		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		var hashs = string.Join( ", ", targets.ToList().ConvertAll( ( t ) => ( t as HashedScriptableObject ).HashID.ToString() ).ToArray() );
		GUILayout.Label( "HashID: " + hashs, K10GuiStyles.boldStyle );
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		var guid = ( targets.Length == 1 ) ? _guidProp.stringValue : "...";
		GUILayout.Label( "GUID: " + guid );
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		GUILayout.EndHorizontal();

		K10.EditorGUIExtention.SeparationLine.Horizontal();
	}

	public override void OnInspectorGUI()
	{
		DrawValidationElementGUI();
		DrawDefaultInspector();
	}
}