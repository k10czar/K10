using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;
using System.Linq;
using System;

public class AggregatedSelectorEditor
{
	SerializedProperty _entriesProp;

	KReorderableList _list;

	string _displayName = string.Empty;

	bool _isDirty = true;

	SubsetDrawer _drawer;

	protected System.Type _elementType;

	PredictionsAggregator _predictions = new();

	ButtonWithIcon _rollButton = new( "Debug Roll", "icons/PlayDices.png" );
	
	public Func<SerializedProperty,Color> _elementColoringFunc = null;

	public AggregatedSelectorEditor( Type type = null, Func<SerializedProperty,Color> ElementColoringFunc = null )
    {
		_elementColoringFunc = ElementColoringFunc;
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
		_drawer = new SubsetDrawer( type, _elementColoringFunc );
    }
	
	public void Setup( SerializedObject serializedObject, string name = null, Type type = null, Texture2D icon = null )
    {
		_displayName = serializedObject.DebugNameOrNull();
		_entriesProp = serializedObject.FindProperty("_entries");
		_predictions.SetProp( _entriesProp );
		SetupList( serializedObject, _entriesProp, name, icon );
		if( type != null ) SetType( type );
    }
	
	public void Setup( SerializedProperty prop, Type type = null, Texture2D icon = null )
	{
		_displayName = prop.displayName;
		_entriesProp = prop.FindPropertyRelative("_entries");
		_predictions.SetProp( _entriesProp );
		SetupList( prop.serializedObject, _entriesProp, prop.displayName, icon );
		if( type != null ) SetType( type );
    }
	
	void SetupList( SerializedObject serializedObject, SerializedProperty prop, string name = null, Texture2D icon = null )
    {
		_list = new KReorderableList( serializedObject, prop, name ?? "Subsets", icon );
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
		
		var isDirty = EditorGUI.EndChangeCheck();
		if( isDirty ) _predictions.SetDirty();
		_isDirty |= isDirty;
		
		if ( elementToRoll != null && _rollButton.Layout() )
		{
			// var debugRollElement = target as ISubsetSelector;
			var result = elementToRoll.Roll<object>();
			Debug.Log($"<color=#BA55D3>Debug Roll</color> of <color=#7CFC00>{elementToRoll.DebugNameOrNull()}</color> result in roll with <color=#87CEFA>{result.Count()}</color>\n\t-{string.Join( "\n\t-",result.ToList().ConvertAll( DebugName ) )}\n");
		}

		if( _isDirty && _predictions.Prop.isExpanded )
		{
			_predictions.Calculate();
			_isDirty = false;
		}

		_predictions.DrawLayout();
    }

	public void Draw( Rect rect, IAggregatedSubsetSelector elementToRoll = null )
    {
		EditorGUI.BeginChangeCheck();
		_list.DrawOnTop( ref rect );
		var isDirty = EditorGUI.EndChangeCheck();
		if( isDirty ) _predictions.SetDirty();
		_isDirty |= isDirty;
		
		if ( elementToRoll != null && _rollButton.DrawOnTop( ref rect ) )
		{
			// var debugRollElement = target as ISubsetSelector;
			var result = elementToRoll.Roll<object>();
			Debug.Log($"<color=#BA55D3>Debug Roll</color> of <color=#7CFC00>{elementToRoll.DebugNameOrNull()}</color> result in roll with <color=#87CEFA>{result.Count()}</color>\n\t-{string.Join( "\n\t-",result.ToList().ConvertAll( DebugName ) )}\n");
		}

		if( _isDirty && _predictions.Prop.isExpanded )
		{
			_predictions.Calculate();
			_isDirty = false;
		}

		_predictions.Draw( rect );
    }

	public float GetHeight( IAggregatedSubsetSelector elementToRoll = null )
    {
		var height = _list.Height;
		if( elementToRoll != null ) height += _rollButton.GetHeight();
		height += _predictions.GetHeight();
		return height;
    }

	private object DebugName(object enemy) => enemy.DebugNameOrNull();
}
