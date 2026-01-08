using System.Collections.Generic;
using System.Drawing.Printing;
using K10.EditorGUIExtention;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


[CustomPropertyDrawer(typeof(ISubsetSelector), true)]
public class SubsetDrawer : PropertyDrawer
{
	const float MARGIN = 6;
	const float MARGIN2 = 2 * MARGIN;
	static readonly Color RED_ERROR = Color.Lerp(Color.white, Color.red, .5f);
	static readonly GUIContent ROLLS_LABEL = new GUIContent("Rolls");

    private Dictionary<string,ReorderableList> _listsCache = new();

	float _sumWeight;
	int _maxVal = 1;
	bool _isFixedAllElements;

	System.Type _elementType = typeof(ScriptableObject);
	protected virtual System.Type ElementType => _elementType;

	System.Func<SerializedProperty,Color> _elementColoringFunc = null;

	public SubsetDrawer( System.Func<SerializedProperty,Color> elementColoringFunc = null ) : this( typeof(ScriptableObject), elementColoringFunc ) { }

	public SubsetDrawer( System.Type elementType, System.Func<SerializedProperty,Color> elementColoringFunc = null )
    {
        _elementType = elementType;
		_elementColoringFunc = elementColoringFunc;
    }

	public ReorderableList GetList( SerializedProperty prop )
    {
		var path = prop.propertyPath;
        if( _listsCache.TryGetValue( path, out var reorderableList ) ) return reorderableList;

		reorderableList = new ReorderableList(prop.serializedObject, prop, true, false, true, true);
		_listsCache.Add( path, reorderableList );

		if( _elementColoringFunc != null )
        {
			void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
			{
				if (prop.arraySize<=index) return;
				if (index<0) return;
				var entry = prop.GetArrayElementAtIndex(index);
				var color = _elementColoringFunc(entry);
				EditorGUI.DrawRect(rect, color.WithValue(.5f));
			}

            reorderableList.drawElementBackgroundCallback = DrawElementBackground;
        }
		
		reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			rect = rect.RequestHeight(EditorGUIUtility.singleLineHeight);
			SerializedProperty element = prop.GetArrayElementAtIndex(index);

			var group = element.FindPropertyRelative("_element");

			var cap = element.FindPropertyRelative("_cap");
			if (_isFixedAllElements)
			{
				ScriptableObjectField.Draw(rect.CutLeft(32), group, ElementType);
				EditorGUI.PropertyField(rect.RequestLeft(32), cap, GUIContent.none);
				return;
			}

			var guiElements = 7;
			ScriptableObjectField.Draw(rect.VerticalSlice(0, guiElements, 2), group, ElementType);
			GuiLabelWidthManager.New(25);
			var guaranteed = element.FindPropertyRelative("_guaranteed");
			EditorGUI.PropertyField(rect.VerticalSlice(2, guiElements), guaranteed,new GUIContent("Min"));
			guaranteed.intValue = Mathf.Max(guaranteed.intValue, 0);

			GuiLabelWidthManager.New(27);

			var capV = cap.intValue;
			var guaV = guaranteed.intValue;

			var wrongCap = capV != 0 && capV < guaV;
			if (wrongCap)
			{
				GuiColorManager.New(RED_ERROR);
				string tooltip = $"Cap({capV}) is lower than Guaranteed Rolls({guaV})!";
				GUIContent label = new GUIContent("Cap", tooltip);
				EditorGUI.PropertyField(rect.VerticalSlice(3, guiElements), cap, label);
				GuiColorManager.Revert();
			}
			else
			{
				EditorGUI.PropertyField(rect.VerticalSlice(3, guiElements), cap);
			}
			cap.intValue = Mathf.Max(cap.intValue, 0);

			var w = element.FindPropertyRelative("_weight");
			var size = Mathf.Clamp(rect.width / 4, 35, 75);
			GuiLabelWidthManager.New(size - 33);
			EditorGUI.PropertyField(rect.VerticalSlice(4, guiElements), w);
			w.floatValue = Mathf.Max(w.floatValue, 0);

			GuiLabelWidthManager.Revert(3);

			GUIProgressBar.Draw(rect.VerticalSlice(5, guiElements, 2), w.floatValue / _sumWeight );
		};
		
		reorderableList.onAddCallback = (ReorderableList list) =>
		{
			var pos = prop.arraySize;
			prop.arraySize++;
			var entry = prop.GetArrayElementAtIndex(pos);
			entry.FindPropertyRelative("_weight").floatValue = 1;
			entry.FindPropertyRelative("_cap").intValue = _maxVal;
		};

		 return reorderableList;
    }

	public override void OnGUI(Rect area, SerializedProperty prop, GUIContent label)
	{
		var slh = EditorGUIUtility.singleLineHeight;
		
		var entriesProp = prop.FindPropertyRelative("_entries");
		var rollsProp = prop.FindPropertyRelative("_rolls");

		_sumWeight = 0;
		var sumCaps = 0;
		var sumGuaranteeds = 0;
		// var hasInfiniteCap = false;
		for(int i = 0; i < entriesProp.arraySize; i++) 
		{
			var element = entriesProp.GetArrayElementAtIndex(i);
			var cap = element.FindPropertyRelative("_cap").intValue;
			if( cap >= 0 ) sumCaps += cap;
			// else hasInfiniteCap = true;
			var min = element.FindPropertyRelative("_guaranteed").intValue;
			if( min > 0 ) sumGuaranteeds += min;
			_sumWeight += element.FindPropertyRelative("_weight").floatValue;
		}

		var reorderableList = GetList( entriesProp );

		GUI.Box( area.CutBottom( MARGIN ), GUIContent.none );
		area = area.CutBottom( MARGIN );
		
		var h = IntRngPropertyDrawer.GetHeight( rollsProp );
		var ruleBoxRect = area.GetLineTop( h + MARGIN2 );
		GUI.Box( ruleBoxRect, GUIContent.none );
		ruleBoxRect = ruleBoxRect.Shrink( MARGIN2 );
		GuiLabelWidthManager.New(36);
		IntRngPropertyDrawer.Draw( ruleBoxRect, rollsProp, ROLLS_LABEL, sumGuaranteeds, sumCaps, null, "MAX ROLL", Colors.SkyBlue, Colors.PaleGoldenrod );
		GuiLabelWidthManager.Revert();

        var range = rollsProp.FindPropertyRelative("_range");
		_maxVal = range.FindPropertyRelative( "max" ).intValue;

		area = area.CutTop( MARGIN );
		reorderableList.DoList( area );
	}

	public override float GetPropertyHeight(SerializedProperty prop, GUIContent label )
	{
		var entriesProp = prop.FindPropertyRelative("_entries");
		
		var reorderableList = GetList( entriesProp );

		var entriesHeight = reorderableList.GetHeight();

		var rollsProp = prop.FindPropertyRelative("_rolls");
		var rollsHeight = IntRngPropertyDrawer.GetHeight( rollsProp );

		return MARGIN2 + entriesHeight + rollsHeight + MARGIN2;
    }
}
