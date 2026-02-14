using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace K10.EditorGUIExtention
{
    public static class DrawGUI
	{
		const float BOXING_SIZE = 10;
		private const string SCRIPT_FIELD = "m_Script";
		static readonly List<string> ignoreClassFullNames = new List<string> { "TMPro.TMP_FontAsset" };

		public static void InlinePropertiesLayout( Object data )
		{
			EditorGUI.indentLevel++;
			SerializedObject serializedObject = new SerializedObject( data );

			// Iterate over all the values and draw them
			SerializedProperty prop = serializedObject.GetIterator();
			if( prop.NextVisible( true ) )
			{
				do
				{
					// Don't bother drawing the class file
					if( prop.name == SCRIPT_FIELD ) continue;
					EditorGUILayout.PropertyField( prop, prop.isExpanded );
				}
				while( prop.NextVisible( false ) );
			}
			if( GUI.changed )
				serializedObject.ApplyModifiedProperties();

			EditorGUI.indentLevel--;
		}

		public static float CalculateInlinePropertiesEditorHeight( SerializedProperty property, GUIContent label, bool boxed = false )
		{
			float height = EditorGUIUtility.singleLineHeight;
			if( boxed ) height += BOXING_SIZE;

			if( ShowExpandedProp( property ) )
				height += CalculateInlinePropertiesEditorHeight( property );

			return height;
		}


		public static float CalculateInlineEditorHeight( SerializedProperty property, GUIContent label, bool boxed = false )
		{
			float height = EditorGUIUtility.singleLineHeight;
			if( boxed ) height += BOXING_SIZE;

			if( ShowExpandedProp( property ) )
				height += EditorGUIUtility.singleLineHeight;

			return height;
		}

		public static float CalculateInlinePropertiesEditorHeight( SerializedProperty property )
		{
			var e = Editor.CreateEditor( property.objectReferenceValue );
			if( e != null ) return CalculateInlinePropertiesEditorHeight( e.serializedObject );
			return 0;
		}

		public static float CalculateInlinePropertiesEditorHeight( SerializedObject serializedObject )
		{
			float height = 0;
			var prop = serializedObject.GetIterator();
			if( prop.NextVisible( true ) )
			{
				do
				{
					if( prop.name == SCRIPT_FIELD ) continue;
					var h = EditorGUI.GetPropertyHeight( prop, new GUIContent( prop.displayName ), prop.isExpanded );
					height += h;
					height += EditorGUIUtility.standardVerticalSpacing;
				}
				while( prop.NextVisible( false ) ) ;
			}
			return height;
		}

		public static void PropertyFieldsFromObject( Rect rect, Object data )
		{
			EditorGUI.indentLevel++;
			SerializedObject serializedObject = new SerializedObject( data );

			// Iterate over all the values and draw them
			SerializedProperty prop = serializedObject.GetIterator();
			if( prop.NextVisible( true ) )
			{
				var svs = EditorGUIUtility.standardVerticalSpacing;
				do
				{
					// Don't bother drawing the class file
					if( prop.name == SCRIPT_FIELD ) continue;
					float height = EditorGUI.GetPropertyHeight( prop, new GUIContent( prop.displayName ), prop.isExpanded );
					EditorGUI.PropertyField( rect.RequestTop( height ), prop, prop.isExpanded );
					rect = rect.CutTop( height + svs );
				}
				while( prop.NextVisible( false ) );
			}
			if( GUI.changed )
				serializedObject.ApplyModifiedProperties();

			EditorGUI.indentLevel--;
		}

		public static void EditorLayoutFromObject( Object data )
		{
			if( data == null ) return;

			var editor = CustomEditorUtility.GetEditor( data );

			if( editor == null ) return;

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUI.indentLevel++;
			editor.OnInspectorGUI();
			EditorGUI.indentLevel--;
			EditorGUILayout.EndVertical();
		}

		static void StartDrawRefPropField( ref Rect area, SerializedProperty property, System.Type type, GUIContent label, bool boxed = false )
		{
			var slh = EditorGUIUtility.singleLineHeight;
			var lw = EditorGUIUtility.labelWidth;

			if( boxed )
			{
				GUI.Box( area, "" );
				area = area.VerticalShrink( BOXING_SIZE );
			}

			EditorGUI.BeginProperty( area, label, property );

			if( type == null || ignoreClassFullNames.Contains( type.FullName ) )
			{
				ScriptableObjectField.Draw( area, property, type );
				EditorGUI.EndProperty();
				return;
			}

			var guiContent = new GUIContent( property.displayName );
			var foldoutRect = new Rect( area.x, area.y, lw, slh );
			if( property.objectReferenceValue != null && AreAnySubPropertiesVisible( property ) )
			{
				var exp = property.isExpanded;
				property.isExpanded = EditorGUI.Foldout( foldoutRect, exp, guiContent, true );
				if( exp != property.isExpanded ) property.serializedObject.ApplyModifiedProperties();
			}
			else
			{
				foldoutRect.x += 12;
				EditorGUI.Foldout( foldoutRect, property.isExpanded, guiContent, true, EditorStyles.label );
			}

			var propertyRect = area.CutLeft( lw ).RequestTop( slh );
			ScriptableObjectField.Draw( propertyRect, property, type );
			if( GUI.changed ) property.serializedObject.ApplyModifiedProperties();
			
			var svs = EditorGUIUtility.standardVerticalSpacing;
			area = area.CutTop( slh + svs );
		}
		
		static void EndDrawRefPropField( SerializedProperty property )
		{
			property.serializedObject.ApplyModifiedProperties();
			EditorGUI.EndProperty();
		}

        static bool ShowExpandedProp(SerializedProperty prop) => prop.isExpanded && prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null;

        public static void PropertyWithInlineProperties( Rect area, SerializedProperty property, System.Type type, GUIContent label, bool boxed )
		{
			StartDrawRefPropField( ref area, property, type, label, boxed );

			if( ShowExpandedProp( property ) )
			{
				var data = (ScriptableObject)property.objectReferenceValue;
				PropertyFieldsFromObject( area, data );
			}

			EndDrawRefPropField( property );
		}

        public static void PropertyWithInlineEditor( Rect area, SerializedProperty property, System.Type type, GUIContent label, bool boxed )
		{
			StartDrawRefPropField( ref area, property, type, label, boxed );

			if( ShowExpandedProp( property ) )
			{

				var obj = (ScriptableObject)property.objectReferenceValue;
				if( obj != null )
				{
					EditorGUI.LabelField( area, "🦗 Inline Editor are not implemented correctly for now" );
					// EditorGUI.LabelField( area, "🦗 Buggy implementation Editor show bellow everything ⤵" );
					// var editor = CustomEditorUtility.GetEditor( obj );
				    // if( editor != null )
				    // {
					// 	// Debug.Log( $"{Event.current} area:{area}" );
					// 	// GUILayout.BeginArea(area);
				    //     EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				    //     EditorGUILayout.LabelField($"Inline editor of: {obj.name}");
				    //     editor.OnInspectorGUI();
				    //     EditorGUILayout.EndVertical();
        			// 	// GUILayout.EndArea();
				    // }
					// else
					// {
					// 	EditorGUI.LabelField( area, "Editor not found!" );
					// }
				}
			}

			EndDrawRefPropField( property );
		}

		static bool AreAnySubPropertiesVisible( SerializedProperty property )
		{
			var data = (ScriptableObject)property.objectReferenceValue;
			SerializedObject serializedObject = new SerializedObject( data );
			SerializedProperty prop = serializedObject.GetIterator();
			while( prop.NextVisible( true ) )
			{
				if( prop.name == SCRIPT_FIELD ) continue;
				return true; //if theres any visible property other than m_script
			}
			return false;
		}

		static ScriptableObject CreateAssetWithSavePrompt( System.Type type, string path )
		{
			path = EditorUtility.SaveFilePanelInProject( "Save ScriptableObject", type.Name + ".asset", "asset", "Enter a file name for the ScriptableObject.", path );
			if( path == "" ) return null;
			ScriptableObject asset = ScriptableObject.CreateInstance( type );
			AssetDatabase.CreateAsset( asset, path );
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate );
			EditorGUIUtility.PingObject( asset );
			return asset;
		}
	}
}
