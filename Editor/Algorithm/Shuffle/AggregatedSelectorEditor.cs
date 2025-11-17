using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using System;

public class AggregatedSelectorEditor
{
	const float COLOR_LERP_FACTOR = .075f;
	static readonly Color BLUE = Color.Lerp(Color.white, Color.blue, COLOR_LERP_FACTOR);
	static readonly Color GREEN = Color.Lerp(Color.white, Color.green, COLOR_LERP_FACTOR / 2);
	static readonly Color RED_ERROR = Color.Lerp(Color.white, Color.red, .5f);

	SerializedProperty _entriesProp;

	KReorderableList _list;

	string _displayName = string.Empty;

	private PersistentValue<bool> _hidePredictions;
	private PersistentValue<bool> _showDetails;
	private PersistentValue<bool> _showPermutations;
	private PersistentValue<bool> _show;

	bool _isDirty;

	SubsetDrawer _drawer;

	protected System.Type _elementType;
	protected virtual System.Type ElementType => _elementType;

	public Func<SerializedProperty,Color> ElementColoring;

	public AggregatedSelectorEditor( Type type = null )
    {
		_hidePredictions = PersistentValue<bool>.At("Temp/SubsetSelector/tablePrediction.tgg");
		_showDetails = PersistentValue<bool>.At("Temp/SubsetSelector/tablePredictionDetails.tgg");
		_showPermutations = PersistentValue<bool>.At("Temp/SubsetSelector/tablePredictionPermutations.tgg");
		SetType( type );
    }

	public AggregatedSelectorEditor( SerializedObject obj, Type type ) : this( type )
    {
		Setup(obj);
    }

	public AggregatedSelectorEditor( SerializedProperty prop, Type type ) : this( type )
    {
		Setup(prop);
    }
	
	public void SetType( Type type )
    {
        _elementType = type;
		_drawer = new SubsetDrawer( type );
    }
	
	public void Setup( SerializedObject serializedObject, Type type = null, Texture2D icon = null )
    {
		_displayName = serializedObject.DebugNameOrNull();
		_entriesProp = serializedObject.FindProperty("_entries");
		SetupList( serializedObject, _entriesProp, icon );
		if( type != null ) SetType( type );
		_show = PersistentValue<bool>.At($"Temp/SubsetSelector/{_displayName}.tgg");
    }
	
	public void Setup( SerializedProperty prop, Type type = null, Texture2D icon = null )
	{
		_displayName = prop.displayName;
		_entriesProp = prop.FindPropertyRelative("_entries");
		SetupList( prop.serializedObject, _entriesProp, icon );
		if( type != null ) SetType( type );
		_show = PersistentValue<bool>.At($"Temp/SubsetSelector/{_displayName}.tgg");
    }
	
	void SetupList( SerializedObject serializedObject, SerializedProperty prop, Texture2D icon = null )
    {
		_list = new KReorderableList( serializedObject, prop, "Subsets", icon );
		_list.List.drawElementCallback = DrawSquadElement;
		_list.List.elementHeightCallback = SquadElementHeight;
		// _list.List.onAddCallback = AddSquadElement;
    }

    void DrawSquadElement(Rect rect, int index, bool isActive, bool isFocused)
	{
		var element = _entriesProp.GetArrayElementAtIndex( index );
		_drawer.OnGUI( rect, element, null );
	}

    private float SquadElementHeight(int index)
    {
		var element = _entriesProp.GetArrayElementAtIndex( index );
		return _drawer.GetPropertyHeight( element, null );
    }

	public void OnInspectorGUI( IAggregatedSubsetSelector elementToRoll )
    {
		EditorGUI.BeginChangeCheck();
		_list.DoLayoutList();
		_isDirty |= EditorGUI.EndChangeCheck();
    }
}
