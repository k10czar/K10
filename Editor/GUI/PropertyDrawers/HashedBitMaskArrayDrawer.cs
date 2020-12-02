using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using K10.EditorGUIExtention;

[CustomPropertyDrawer( typeof( HashedBitMaskArrayAttribute ) )]
public class HashedBitMaskArrayDrawer : PropertyDrawer
{
	private const int BATCH_SIZE = 32;

	public override void OnGUI( Rect area, SerializedProperty prop, GUIContent label )
	{
		var typeAttr = attribute as HashedBitMaskArrayAttribute;

		var inst = ScriptableObject.CreateInstance( typeAttr.propType ) as HashedScriptableObject;

		if( inst == null ) return;

		var col = inst.GetCollection();
		var steps = ( ( col.Count - 1 ) / BATCH_SIZE ) + 1;

		var labelArea = area.RequestLeft( EditorGUIUtility.labelWidth );
		const int BUTTON_WIDTH = 30;
		var masksArea = area.CutLeft( EditorGUIUtility.labelWidth ).CutRight( BUTTON_WIDTH );
		var buttonArea = area.RequestRight( BUTTON_WIDTH );

		label.text = label.text + ( ( prop.arraySize == 0 ) ? "(ALL)" : "(" + prop.intValue + ")" );
		GUI.Label( labelArea, label.text );

		EditorGuiIndentManager.New( 0 );
		var names = new List<string[]>();
		for( int i = 0; i < steps; i++ )
		{
			var init = i * BATCH_SIZE;
			var count = Mathf.Min( col.Count - init, BATCH_SIZE );
			var n = new string[ count ];
			names.Add( n );
			for( int j = 0; j < count; j++ )
			{
				var eId = init + j;
				var e = col.GetElementBase( eId );
				n[j] = e.ToStringOrNull();
			}
			var val = 0;
			if( i <= prop.arraySize ) val = prop.GetArrayElementAtIndex( i ).intValue;
			var newVal = EditorGUI.MaskField( masksArea.VerticalSlice( 1, steps ), GUIContent.none, val, n );

			if( newVal != val ) 
			{
				while( prop.arraySize <= i )
				{
					prop.InsertArrayElementAtIndex( prop.arraySize );
					prop.GetArrayElementAtIndex( i ).intValue = 0;
				}
				prop.GetArrayElementAtIndex( i ).intValue = newVal;
			}
		}
		EditorGuiIndentManager.Revert();

		if( GUI.Button( buttonArea, "ALL" ) ) prop.arraySize = 0;
	}
}