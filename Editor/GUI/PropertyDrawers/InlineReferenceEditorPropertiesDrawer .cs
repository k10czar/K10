
using System.Collections.Generic;
using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( InlinePropertiesAttribute ) )]
public class InlinePropertiesDrawer : PropertyDrawer
{
	const int buttonWidth = 66;
	private const string SCRIPT_FIELD = "m_Script";
	static readonly List<string> ignoreClassFullNames = new List<string> { "TMPro.TMP_FontAsset" };

	// Draw the property inside the given rect
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		var slh = EditorGUIUtility.singleLineHeight;
		var svs = EditorGUIUtility.standardVerticalSpacing;
		var lw = EditorGUIUtility.labelWidth;

		var attrib = (InlinePropertiesAttribute)attribute;
		if( attrib.boxed )
		{
			GUI.Box( new Rect( 0, area.y, Screen.width, area.height ), "" );
			area = area.VerticalShrink( 6 );
		}

		EditorGUI.BeginProperty( area, label, property );
		var type = GetFieldType();

		if( type == null || ignoreClassFullNames.Contains( type.FullName ) )
		{
			EditorGUI.PropertyField( area, property, label );
			EditorGUI.EndProperty();
			return;
		}

		ScriptableObject propertySO = null;
		if( !property.hasMultipleDifferentValues && property.serializedObject.targetObject != null && property.serializedObject.targetObject is ScriptableObject )
		{
			propertySO = (ScriptableObject)property.serializedObject.targetObject;
		}

		var propertyRect = Rect.zero;
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
			// So yeah having a foldout look like a label is a weird hack 
			// but both code paths seem to need to be a foldout or 
			// the object field control goes weird when the codepath changes.
			// I guess because foldout is an interactable control of its own and throws off the controlID?
			foldoutRect.x += 12;
			EditorGUI.Foldout( foldoutRect, property.isExpanded, guiContent, true, EditorStyles.label );
		}

		propertyRect = area.CutLeft( lw ).RequestTop( slh );
		ScriptableObjectField.Draw( propertyRect, property, type );
		// property.objectReferenceValue = EditorGUI.ObjectField( propertyRect, GUIContent.none, property.objectReferenceValue, type, false );
		if( GUI.changed ) property.serializedObject.ApplyModifiedProperties();

		var buttonRect = new Rect( area.x + area.width - buttonWidth, area.y, buttonWidth, slh );


		if( property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null )
		{
			var data = (ScriptableObject)property.objectReferenceValue;

			if( property.isExpanded )
			{

				EditorGUI.indentLevel++;
				SerializedObject serializedObject = new SerializedObject( data );

				// Iterate over all the values and draw them
				SerializedProperty prop = serializedObject.GetIterator();
				float y = area.y + slh + svs;
				if( prop.NextVisible( true ) )
				{
					do
					{
						// Don't bother drawing the class file
						if( prop.name == SCRIPT_FIELD ) continue;
						float height = EditorGUI.GetPropertyHeight( prop, new GUIContent( prop.displayName ), prop.isExpanded );
						EditorGUI.PropertyField( new Rect( area.x, y, area.width, height ), prop, prop.isExpanded );
						y += height + svs;
					}
					while( prop.NextVisible( false ) );
				}
				if( GUI.changed )
					serializedObject.ApplyModifiedProperties();

				EditorGUI.indentLevel--;
			}
		}

		property.serializedObject.ApplyModifiedProperties();
		EditorGUI.EndProperty();
	}

	// Creates a new ScriptableObject via the default Save File panel
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

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		var slh = EditorGUIUtility.singleLineHeight;
		var svs = EditorGUIUtility.standardVerticalSpacing;
		float height = base.GetPropertyHeight( property, label );
		var attrib = (InlinePropertiesAttribute)attribute;
		if( attrib.boxed ) height += 6;

		if( property.objectReferenceValue != null && AreAnySubPropertiesVisible( property ) && property.isExpanded ) 
		{
			var e = Editor.CreateEditor( property.objectReferenceValue );
			if( e != null )
			{
				var so = e.serializedObject;
				var prop = so.GetIterator();
				if( prop.NextVisible( true ) )
				{
					do
					{
						if( prop.name == SCRIPT_FIELD ) continue;
						var h = EditorGUI.GetPropertyHeight( prop, new GUIContent( prop.displayName ), prop.isExpanded );
						height += h;
						height += svs;
					}
					while( prop.NextVisible( false ) ) ;
				}
			}
		}

		return height;
	}

	System.Type GetFieldType()
	{
		System.Type type = fieldInfo.FieldType;
		if( type.IsArray ) type = type.GetElementType();
		else if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ) ) type = type.GetGenericArguments()[0];
		return type;
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
}