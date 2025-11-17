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
	int _currMax;
	bool _isFixedAllElements;

	System.Type _elementType = typeof(ScriptableObject);
	protected virtual System.Type ElementType => _elementType;

	public SubsetDrawer() : this( typeof(ScriptableObject) ) { }

	public SubsetDrawer( System.Type elementType )
    {
        _elementType = elementType;
		Debug.Log( $"SubsetDrawer( {elementType} )" );
    }

	public ReorderableList GetList( SerializedProperty prop )
    {
		var path = prop.propertyPath;
        if( _listsCache.TryGetValue( path, out var reorderableList ) ) return reorderableList;

		reorderableList = new ReorderableList(prop.serializedObject, prop, true, false, true, true);
		_listsCache.Add( path, reorderableList );
		
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
			entry.FindPropertyRelative("_cap").intValue = _isFixedAllElements ? 1 : _currMax;
		};

		 return reorderableList;
    }

	public override void OnGUI(Rect area, SerializedProperty prop, GUIContent label)
	{
		var slh = EditorGUIUtility.singleLineHeight;
		
		var displayName = prop.displayName;
		var ruleProp = prop.FindPropertyRelative("_rule");
		var minProp = prop.FindPropertyRelative("_min");
		var maxProp = prop.FindPropertyRelative("_max");
		var entriesProp = prop.FindPropertyRelative("_entries");
		var rangeWeightsProp = prop.FindPropertyRelative("_rangeWeights");

		_sumWeight = 0;
		var sumCaps = 0;
		var hasInfiniteCap = false;
		for(int i = 0; i < entriesProp.arraySize; i++) 
		{
			var element = entriesProp.GetArrayElementAtIndex(i);
			var cap = element.FindPropertyRelative("_cap").intValue;
			if( cap >= 0 ) sumCaps += cap;
			else hasInfiniteCap = true;
			_sumWeight += element.FindPropertyRelative("_weight").floatValue;
		}

		var reorderableList = GetList( entriesProp );
		
		var rulesSize = slh + MARGIN2;
		if( ruleProp.enumValueIndex == (int)ESubsetGeneratorRule.BIASED_RANGE )
        {
			var min = prop.FindPropertyRelative("_min").intValue;
			var max = prop.FindPropertyRelative("_max").intValue;
			var delta = max + 1 - min;
			if( delta < 1 ) delta = 1;
			rulesSize += delta * ( slh + MARGIN );
        }

		GUI.Box( area.CutBottom( MARGIN ), GUIContent.none );
		area = area.CutBottom( MARGIN );
		var ruleBoxRect = area.GetLineTop( rulesSize );
		GUI.Box( ruleBoxRect, GUIContent.none );
		ruleBoxRect = ruleBoxRect.Shrink( MARGIN2 );
		var ruleRect = ruleBoxRect.GetLineTop( slh );
		var newRule = (ESubsetGeneratorRule)EditorGUI.EnumPopup( ruleRect.GetColumnLeft(130, 4), GUIContent.none, (ESubsetGeneratorRule)ruleProp.enumValueIndex );
		ruleProp.enumValueIndex = (int)newRule;
		
		_isFixedAllElements = newRule == ESubsetGeneratorRule.MAX_ROLL;

        switch (newRule)
        {
            case ESubsetGeneratorRule.MAX_ROLL: 
				var count = 0;
				for (int i = 0; i < entriesProp.arraySize; i++)
				{
					var element = entriesProp.GetArrayElementAtIndex(i);
					var cap = element.FindPropertyRelative("_cap").intValue;
					if (cap > 0) count += cap;
				}
				EditorGUI.LabelField(ruleRect,$"with {count} element{(count > 1 ? "s" : "")}", K10GuiStyles.boldStyle);
                break;

            case ESubsetGeneratorRule.FIXED_ROLLS:
				GuiLabelWidthManager.New(30);
				EditorGUI.PropertyField(ruleRect, maxProp, ROLLS_LABEL);
				GuiLabelWidthManager.Revert();
                break;

            case ESubsetGeneratorRule.UNIFORM_RANGE:
            case ESubsetGeneratorRule.BIASED_RANGE:
				EditorGUI.LabelField(ruleRect.GetColumnLeft(42), "Rolls", K10GuiStyles.boldStyle);
				GuiLabelWidthManager.New(23);
				var wrongMin = !hasInfiniteCap && (minProp.intValue > sumCaps);
				if (wrongMin) GuiColorManager.New(RED_ERROR);
				EditorGUI.PropertyField(ruleRect.VerticalSlice( 0, 2 ), minProp);
				if (wrongMin) GuiColorManager.Revert();
				GuiLabelWidthManager.New(28);
				var wrongMax = !hasInfiniteCap && (maxProp.intValue > sumCaps);
				if (wrongMax) GuiColorManager.New(RED_ERROR);
				EditorGUI.PropertyField(ruleRect.VerticalSlice( 1, 2 ), maxProp);
				if (wrongMax) GuiColorManager.Revert();
				GuiLabelWidthManager.Revert(2);

				if( newRule == ESubsetGeneratorRule.UNIFORM_RANGE ) break;

				var min = minProp.intValue;
				var max = maxProp.intValue;
				var range = max + 1 - min;

				var oldSize = rangeWeightsProp.arraySize;
				if( oldSize < range ) rangeWeightsProp.arraySize = range;

				if (range > 0)
                {
					for (int i = oldSize; i < range; i++)
					{
						var e = rangeWeightsProp.GetArrayElementAtIndex(i);
						e.floatValue = 1;
					}
					
					var sumWeight = 0f;
					for (int i = 0; i < range; i++) sumWeight += rangeWeightsProp.GetArrayElementAtIndex(i).floatValue;

					GUI.Box( ruleBoxRect, GUIContent.none );
					var biasesRect = ruleBoxRect.CutTop( MARGIN );
					
					for (int i = 0; i < range; i++)
					{
						var rolls = min + i;
						var element = rangeWeightsProp.GetArrayElementAtIndex(i);
						var rect = biasesRect.HorizontalSlice( i, range, MARGIN );
						GUI.Box( rect, GUIContent.none );

						rect = rect.Shrink( MARGIN2, 0 );

						GuiLabelWidthManager.New(72);
						var newVal = EditorGUI.FloatField( rect.GetColumnLeft( 120 ), $"[{rolls}]Weight", element.floatValue);
						GuiLabelWidthManager.Revert();
						element.floatValue = newVal;
						GUIProgressBar.Draw( rect, newVal / sumWeight );
					}
                }
				break;
        }

		area = area.CutTop( MARGIN );
		_currMax = maxProp.intValue;
		reorderableList.DoList( area );
	}

	public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
	{
		var slh = EditorGUIUtility.singleLineHeight;
		// if (!prop.isExpanded) return EditorGUIUtility.singleLineHeight;
		
		var ruleProp = prop.FindPropertyRelative("_rule");
		// var minProp = prop.FindPropertyRelative("_min");
		// var maxProp = prop.FindPropertyRelative("_max");
		var entriesProp = prop.FindPropertyRelative("_entries");
		// var rangeWeightsProp = prop.FindPropertyRelative("_rangeWeights");
		
		var reorderableList = GetList( entriesProp );

		var height = slh + MARGIN2;

		var rule = (ESubsetGeneratorRule)ruleProp.enumValueIndex;
        switch (rule)
        {
            // case ESubsetGeneratorRule.MAX_ROLL:
            //     height = slh + MARGIN2;
			// 	break;

            // case ESubsetGeneratorRule.FIXED_ROLLS:
            //     height = slh + MARGIN2;
			// 	break;

            // case ESubsetGeneratorRule.UNIFORM_RANGE:
            //     height = slh + MARGIN2;
			// 	break;

            case ESubsetGeneratorRule.BIASED_RANGE:
				var min = prop.FindPropertyRelative("_min").intValue;
				var max = prop.FindPropertyRelative("_max").intValue;
				var delta = ( max + 1 ) - min;
				if( delta < 1 ) delta = 1;
				height += delta * ( slh + MARGIN );
				break;
        }

		// var entriesHeight = 20;
		var entriesHeight = reorderableList.GetHeight();

		return height + MARGIN2 + entriesHeight;
    }
}
