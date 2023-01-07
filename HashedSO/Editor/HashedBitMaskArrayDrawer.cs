using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using K10.EditorGUIExtention;
using System.Collections;
using System.Linq;

[CustomPropertyDrawer( typeof( HashedElementFilterBitsAttribute ) )]
public class HashedElementFilterBitsDrawer : PropertyDrawer
{
	private const int BATCH_SIZE = 32;

	public override void OnGUI( Rect area, SerializedProperty prop, GUIContent label )
	{
		if( prop.type != nameof( Bits ) )
		{
			EditorGUI.HelpBox( area, $"This field {prop.displayName} need to be from type {nameof( Bits )}, {prop.type} is not a valid type", MessageType.Error );
			return;
		}

		var typeAttr = attribute as HashedElementFilterBitsAttribute;
		var inst = ScriptableObject.CreateInstance( typeAttr.propType ) as HashedScriptableObject;

		if( inst == null )
		{
			EditorGUI.HelpBox( area, $"The editor for {prop.displayName} need to be derived from type {nameof( HashedScriptableObject )}, {typeAttr.propType} is not a valid type", MessageType.Error );
			return;
		}

		// return;

		var bits = prop.FindPropertyRelative( "_array" );
		var adapter = new PropIntListAdapter( bits );

		var col = inst.GetCollection();
		var steps = ( ( col.Count - 1 ) / BATCH_SIZE ) + 1;

		var labelArea = area.RequestLeft( EditorGUIUtility.labelWidth );
		const int BUTTON_WIDTH = 40;
		var masksArea = area.CutLeft( EditorGUIUtility.labelWidth ).CutRight( BUTTON_WIDTH );
		var buttonArea = area.RequestRight( BUTTON_WIDTH );

		var allSet = BitsManipulator.IsAll( adapter, true );
		var allunset = BitsManipulator.IsAll( adapter, false );
		if( GUI.Button( buttonArea, allSet ? "ALL" : ( allunset ? "NONE" : BitsManipulator.CountSet( adapter ).ToString() ) ) ) BitsManipulator.SetAll( adapter, !allSet );

		label.text = label.text + "(" + BitsManipulator.ToString( adapter ) + ") { " + string.Join( ", ", adapter.ToList().ConvertAll( ( v ) => v.ToString() ) ) + " }";
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
			var val = BitsManipulator.QuerySector32( adapter, i );
			var newVal = EditorGUI.MaskField( masksArea.VerticalSlice( i, steps ), GUIContent.none, val, n );

			if( newVal != val ) 
			{
				var changes = newVal ^ val;
				for( int j = 0; j < 32; j++ )
				{
					var itMask = ( 1 << j );
					if( ( changes & itMask ) == 0 ) continue;
					var newBit = ( newVal & itMask ) != 0;
					BitsManipulator.Set( adapter, init + j, newBit );
				}
			}
		}
		EditorGuiIndentManager.Revert();
	}
}