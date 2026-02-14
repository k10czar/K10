using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;
using System.Linq;
using System;

public class CompoundAggregatedSelectorEditor<T,K> : AggregatedSelectorEditor<T> where T : ScriptableObject, IAggregatedSubsetSelector<K>
{
    public CompoundAggregatedSelectorEditor( Type type = null, Func<SerializedProperty,Color> ElementColoringFunc = null, IAggregatedPredictor<T> predictor = null )
			:base( type, ElementColoringFunc, predictor )
    {
    }

	protected override void DrawPreditions()
	{
		base.DrawPreditions();
		if( _predictions.aggregatedPredictor is CompoundAggregatedPredictor<T,K> compound ) compound.DrawElementsRanges();
	}
}

public class AggregatedSelectorEditor<T> where T : ScriptableObject
{
	SerializedProperty _entriesProp;

	KReorderableList _list;

	bool _isDirty = true;

	SubsetDrawer _drawer;

	protected Type _elementType;

	protected PredictionsAggregator<T> _predictions;

	ButtonWithIcon _rollButton = new( "Debug Roll", "icons/PlayDices.png" );

    public readonly static IValueCapsule<bool> showPreditionsBool = new LazyEditorPersistentValue<bool>( "ShowPreditions" );
	static ToggleButtonFromPropExpansionLazy _showPreditionsToggleButton = new( "Chances Prediction", "icons/dices.png", null );
	
	public Func<SerializedProperty,Color> _elementColoringFunc = null;

    public bool HasDataFilled
    {
        get
        {
			if( _entriesProp == null ) return false;
			var count = _entriesProp.arraySize;
			if( count == 0 ) return false;
			for( int i = 0; i < count; i++ )
            {
                var entry = _entriesProp.GetArrayElementAtIndex(i);
				var setEntriesProp = entry.FindPropertyRelative("_entries");
				if( setEntriesProp == null ) continue;
				var setCount = setEntriesProp.arraySize;
				if( setCount == 0 ) continue;
				return true;
            }
			return false;
        }
    }

    public AggregatedSelectorEditor( Type type = null, Func<SerializedProperty,Color> ElementColoringFunc = null, IAggregatedPredictor<T> predictor = null )
    {
		_elementColoringFunc = ElementColoringFunc;
		SetType( type );
		_predictions = new( predictor );
    }
	
	public void SetType( Type type )
    {
        _elementType = type;
		_drawer = new SubsetDrawer( type, _elementColoringFunc );
    }
	
	public void Setup( SerializedObject serializedObject, string name = null, Type type = null, Texture2D icon = null )
    {
		_entriesProp = serializedObject.FindProperty("_entries");
		_predictions.SetProp( _entriesProp );
		SetupList( serializedObject, _entriesProp, name, icon );
		if( type != null ) SetType( type );
		_isDirty = true;
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

	public void SetDirty()
	{
		_isDirty = true;
	}

	protected virtual void DrawPreditions()
	{
		if( _predictions.aggregatedPredictor is BaseAggregatedPredictor<T> ap ) ap.DrawTableLayout();
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

		if( _isDirty && showPreditionsBool.Get )
		{
			_predictions.Calculate();
			_isDirty = false;
		}

		EditorGUILayout.BeginVertical( GUI.skin.box );
		if( _showPreditionsToggleButton.Layout( showPreditionsBool ) ) DrawPreditions();
		EditorGUILayout.EndVertical();
    }

	private object DebugName(object enemy) => enemy.DebugNameOrNull();
}
