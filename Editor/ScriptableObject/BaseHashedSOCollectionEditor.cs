using UnityEngine;
using UnityEditor;
using K10.EditorGUIExtention;

[CustomEditor( typeof( BaseHashedSOCollection ), true )]
public class BaseHashedSOCollectionEditor : Editor
{
	private string _title;

	void OnEnable()
	{
		_title = target.NameOrNull();
		for( int i = _title.Length - 1; i > 0; i-- )
		{
			var c = _title[i];
			if( !char.IsUpper( c ) ) continue;
			_title = _title.Insert( i, " " );
		}
	}

	public override void OnInspectorGUI()
	{
		var collection = (BaseHashedSOCollection)target;
		int size = collection.Count;

		var count = 0;
		for( int i = 0; i < size; i++ )
		{
			var entry = collection.GetElementBase( i );
			if( entry == null ) continue;
			count++;
		}

		SeparationLine.Horizontal();
		EditorGUILayout.LabelField( $"{_title} ({count})", K10GuiStyles.bigBoldCenterStyle, GUILayout.Height( 28 ) );
		SeparationLine.Horizontal();

		EditorGUILayout.BeginVertical();
		for( int i = 0; i < size; i++ )
		{
			var entry = collection.GetElementBase( i ) as IHashedSO;
			if( entry == null ) continue;
			EditorGUILayout.BeginHorizontal();
			var hasConflict = ( entry.HashID < 0 || entry.HashID != i );
			if( hasConflict ) GuiColorManager.New( Color.red );
			EditorGUILayout.LabelField( "[" + i.ToString() + "]", GUILayout.Width( 30f ) );

			if( hasConflict ) GUILayout.Button( "!!CONFLICT!!" );

			EditorGUI.BeginDisabledGroup( true );
			EditorGUILayout.ObjectField( entry as Object, collection.GetElementType(), false );
			EditorGUI.EndDisabledGroup();

			if( hasConflict ) GuiColorManager.Revert();

			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
	}
}