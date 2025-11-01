using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using System;

public class WeightedSubsetSelectorEditor
{
	const float COLOR_LERP_FACTOR = .075f;
	static readonly Color BLUE = Color.Lerp(Color.white, Color.blue, COLOR_LERP_FACTOR);
	static readonly Color GREEN = Color.Lerp(Color.white, Color.green, COLOR_LERP_FACTOR / 2);
	static readonly Color RED_ERROR = Color.Lerp(Color.white, Color.red, .5f);

	SerializedProperty _ruleProp;
	SerializedProperty _minProp;
	SerializedProperty _maxProp;
	SerializedProperty _entriesProp;
	SerializedProperty _rangeWeightsProp;

	KReorderableList _list;

	string _displayName = string.Empty;

	private PersistentValue<bool> _hidePredictions;
	private PersistentValue<bool> _showDetails;
	private PersistentValue<bool> _showPermutations;
	private PersistentValue<bool> _show;

	float _sumWeight, _sumCaps;
	bool _hasInfiniteCap = false;

	bool _isDirty = true;

	bool IsFixedAllElements => _ruleProp.enumValueIndex == (int)ESubsetGeneratorRule.MAX_ROLL;
	bool IsFixedRolls => _ruleProp.enumValueIndex == (int)ESubsetGeneratorRule.FIXED_ROLLS;
	bool IsBiased => _ruleProp.enumValueIndex == (int)ESubsetGeneratorRule.BIASED_RANGE;

	public GUIStyle _rollButton;

	Type _elementType;

	public Func<SerializedProperty,Color> ElementColoring;

	public WeightedSubsetSelectorEditor( Type type = null )
    {
		_hidePredictions = PersistentValue<bool>.At("Temp/SubsetSelector/tablePrediction.tgg");
		_showDetails = PersistentValue<bool>.At("Temp/SubsetSelector/tablePredictionDetails.tgg");
		_showPermutations = PersistentValue<bool>.At("Temp/SubsetSelector/tablePredictionPermutations.tgg");
		SetType( type );
    }

	public WeightedSubsetSelectorEditor( SerializedObject obj, Type type ) : this( type )
    {
		Setup(obj);
    }

	public WeightedSubsetSelectorEditor( SerializedProperty prop, Type type ) : this( type )
    {
		Setup(prop);
    }
	
	public void SetType( Type type )
    {
        _elementType = type;
    }
	
	public void Setup( SerializedObject serializedObject, Type type = null, Texture2D icon = null )
    {
		_displayName = serializedObject.DebugNameOrNull();
		_ruleProp = serializedObject.FindProperty("_rule");
		_minProp = serializedObject.FindProperty("_min");
		_maxProp = serializedObject.FindProperty("_max");
		_entriesProp = serializedObject.FindProperty("_entries");
		_rangeWeightsProp = serializedObject.FindProperty("_rangeWeights");
		SetupList( serializedObject, _entriesProp, icon );
		if( type != null ) SetType( type );
		_show = PersistentValue<bool>.At($"Temp/SubsetSelector/{_displayName}.tgg");
    }
	
	public void Setup( SerializedProperty prop, Type type = null, Texture2D icon = null )
	{
		_displayName = prop.displayName;
		_ruleProp = prop.FindPropertyRelative("_rule");
		_minProp = prop.FindPropertyRelative("_min");
		_maxProp = prop.FindPropertyRelative("_max");
		_entriesProp = prop.FindPropertyRelative("_entries");
		_rangeWeightsProp = prop.FindPropertyRelative("_rangeWeights");
		SetupList( prop.serializedObject, _entriesProp, icon );
		if( type != null ) SetType( type );
		_show = PersistentValue<bool>.At($"Temp/SubsetSelector/{_displayName}.tgg");
    }
	
	void SetupList( SerializedObject serializedObject, SerializedProperty prop, Texture2D icon = null )
    {
		_list = new KReorderableList( serializedObject, prop, "Entries", icon );
		_list.List.drawElementCallback = DrawSquadElement;
		_list.List.onAddCallback = AddSquadElement;
		_list.List.drawElementBackgroundCallback = DrawElementBackground;
    }
	

    private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
	{
		if (ElementColoring == null) return;
		if (_entriesProp == null) return;
		if (_entriesProp.arraySize<=index) return;
		if (index<0) return;
		var entry = _entriesProp.GetArrayElementAtIndex(index);
		// GuiBackgroundColorManager.New(ElementColoring(entry));
		// GUI.Box(rect, DefaultTextures.WhiteTexture);
		var color = ElementColoring(entry);
		EditorGUI.DrawRect(rect, color.WithValue(.75f));
		// GuiBackgroundColorManager.Revert();
    }

    public void LabeledInspectorGUI( ISubsetSelector debugRollElement )
	{
		EditorGUILayout.BeginVertical( GUI.skin.box );
		_show.Set = EditorGUILayout.Foldout(_show.Get, _displayName);
		if( _show.Get ) OnInspectorGUI(debugRollElement);
		EditorGUILayout.EndVertical();
	}
	
	public void OnInspectorGUI( ISubsetSelector debugRollElement )
	{
        // lastLabelRect = GUILayoutUtility.GetLastRect();
		// float currentLabelWidth = lastLabelRect.width;
		
		if (_entriesProp == null) return;
		PreProcessData();
		DrawRollsRangeBox(BLUE);

		EditorGUI.BeginChangeCheck();
		_list.DoLayoutList();
		_isDirty |= EditorGUI.EndChangeCheck();

		if (IsFixedAllElements) return;

		FillCaches();

		if ( debugRollElement != null )
		{
			if( _rollButton == null ) _rollButton = new GUIStyle(EditorStyles.miniButton) { fontSize = 18, fontStyle = FontStyle.Bold, fixedHeight = 72, padding = new RectOffset(8, 8, 12, 12) };
			if( GUILayout.Button( new GUIContent( "Debug Roll", IconCache.Get( "icons/PlayDices.png" ).Texture ), _rollButton ) )
            {
				// var debugRollElement = target as ISubsetSelector;
				var result = debugRollElement.Roll<object>();
				Debug.Log($"<color=#BA55D3>Debug Roll</color> of <color=#7CFC00>{debugRollElement.DebugNameOrNull()}</color> result in roll with <color=#87CEFA>{result.Count()}</color>\n\t-{string.Join( "\n\t-",result.ToList().ConvertAll( DebugName ) )}\n");
            }
		}

		DrawPredictions(CalculateBacktracking);
	}

    public void CleanOldRanges()
    {
		if (_minProp == null) return;
		if (_maxProp == null) return;
		if (_rangeWeightsProp == null) return;
		var min = _minProp.intValue;
		var max = _maxProp.intValue;
		var range = max + 1 - min;
		_rangeWeightsProp.arraySize = range;
    }
	
	private object DebugName(object enemy) => enemy.DebugNameOrNull();

    private void AddSquadElement(ReorderableList list)
	{
		var pos = _entriesProp.arraySize;
		_entriesProp.arraySize++;
		var entry = _entriesProp.GetArrayElementAtIndex(pos);
		entry.FindPropertyRelative("_weight").floatValue = 1;
		entry.FindPropertyRelative("_cap").intValue = IsFixedAllElements ? 1 : _maxProp.intValue;
    }

    void DrawSquadElement(Rect rect, int index, bool isActive, bool isFocused)
	{
		rect = rect.RequestHeight(EditorGUIUtility.singleLineHeight);

		var element = _entriesProp.GetArrayElementAtIndex(index);

		var group = element.FindPropertyRelative("_element");

		var cap = element.FindPropertyRelative("_cap");
		if (IsFixedAllElements)
		{
			ScriptableObjectField.Draw(rect.CutLeft(32), group, _elementType);
			EditorGUI.PropertyField(rect.RequestLeft(32), cap, GUIContent.none);
			return;
		}

		var guiElements = 7;
		ScriptableObjectField.Draw(rect.VerticalSlice(0, guiElements, 2), group, _elementType);
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

		// var val = ( Mathf.Approximately( _sumWeight, 0 ) ) ? 0 : w.floatValue / _sumWeight;
		// var percentage = ( val * 100 );
		// EditorGUI.ProgressBar( rect.VerticalSlice( 5, guiElements ), val, $"{percentage:N1}%" );
	}

	private void PreProcessData()
	{
		_sumWeight = 0;
		_sumCaps = 0;
		_hasInfiniteCap = false;

		for( int i = 0; i < _entriesProp.arraySize; i++ )
		{
			var element = _entriesProp.GetArrayElementAtIndex( i );
			_sumWeight += element.FindPropertyRelative( "_weight" ).floatValue;
			var cap = element.FindPropertyRelative( "_cap" ).intValue;
			var guaranteed = element.FindPropertyRelative( "_guaranteed" ).intValue;

			_hasInfiniteCap |= cap < 0;

			_sumCaps += Mathf.Max( guaranteed, cap );
		}
	}
	
	static GUILayoutOption ROLLS_HEIGHT = GUILayout.Height(25);

	private void DrawRollsRangeBox( Color color )
	{
		GuiColorManager.New( color );
		EditorGUILayout.BeginVertical( GUI.skin.box );
		GuiColorManager.Revert();
		EditorGUILayout.BeginHorizontal();

		string errorMsg = string.Empty;
		EditorGUILayout.BeginVertical( GUI.skin.box, GUILayout.Width(84), ROLLS_HEIGHT );
		// GUILayout.Space( 3 );
		var newRule = EditorGUILayout.EnumPopup( GUIContent.none, (ESubsetGeneratorRule)_ruleProp.enumValueIndex, GUILayout.Width(130), ROLLS_HEIGHT );
		EditorGUILayout.EndVertical();
		_ruleProp.enumValueIndex = (int)(ESubsetGeneratorRule)newRule;
		
		if( (int)(ESubsetGeneratorRule)newRule != _ruleProp.enumValueIndex )Debug.Log( $"{_ruleProp.enumValueIndex} => {newRule}" );

		if (IsFixedAllElements)
		{
			var count = 0;
			for (int i = 0; i < _entriesProp.arraySize; i++)
			{
				var element = _entriesProp.GetArrayElementAtIndex(i);
				var cap = element.FindPropertyRelative("_cap").intValue;
				if (cap > 0) count += cap;
			}
			EditorGUILayout.LabelField($"with {count} element{(count > 1 ? "s" : "")}", K10GuiStyles.boldStyle, ROLLS_HEIGHT);
		}
		else if( IsFixedRolls )
        {
			GuiLabelWidthManager.New(30);
			var wrong = !_hasInfiniteCap && (_maxProp.intValue > _sumCaps);
			if (wrong) GuiColorManager.New(RED_ERROR);
			EditorGUILayout.BeginHorizontal(GUI.skin.box);
			EditorGUILayout.PropertyField(_maxProp, new GUIContent("Rolls"));
			EditorGUILayout.EndHorizontal();
			if (wrong) GuiColorManager.Revert();
			GuiLabelWidthManager.Revert();

			wrong = !_hasInfiniteCap && (_maxProp.intValue > _sumCaps);
			if ( wrong ) errorMsg = $"The maximum number of rolls is {_sumCaps}, due the sum of entries cap limit";
        }
		else
		{
			EditorGUILayout.LabelField("Rolls", K10GuiStyles.boldStyle, GUILayout.Width(45), ROLLS_HEIGHT);

			EditorGUILayout.BeginVertical();
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUI.skin.box);

			GuiLabelWidthManager.New(23);

			var wrongMin = !_hasInfiniteCap && (_minProp.intValue > _sumCaps);

			if (wrongMin) GuiColorManager.New(RED_ERROR);
			EditorGUILayout.PropertyField(_minProp, new GUIContent("Min"), GUILayout.MinWidth(40));
			if (wrongMin) GuiColorManager.Revert();

			var wrongMax = !_hasInfiniteCap && (_maxProp.intValue > _sumCaps);

			GuiLabelWidthManager.New(28);

			if (wrongMax) GuiColorManager.New(RED_ERROR);
			EditorGUILayout.PropertyField(_maxProp, new GUIContent("Max"), GUILayout.MinWidth(45));
			if (wrongMax) GuiColorManager.Revert();

			GuiLabelWidthManager.Revert(2);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			if (wrongMin || wrongMax) errorMsg = $"The maximum number of rolls is {_sumCaps}, due the sum of entries cap limit or guaranteed rolls";
		}

		EditorGUILayout.EndHorizontal();

		if( !string.IsNullOrEmpty( errorMsg ) )
		{
			GuiColorManager.New( RED_ERROR );
			GUILayout.Label( errorMsg, GUI.skin.box, GUILayout.ExpandWidth( true ) );
			GuiColorManager.Revert();
		}

		if (IsBiased)
		{
			var min = _minProp.intValue;
			var max = _maxProp.intValue;
			var range = max + 1 - min;

			var oldSize = _rangeWeightsProp.arraySize;
			if( oldSize < range ) _rangeWeightsProp.arraySize = range;
			if (range > 0)
			{
				for (int i = oldSize; i < range; i++)
				{
					var e = _rangeWeightsProp.GetArrayElementAtIndex(i);
					e.floatValue = 1;
				}
				var sumWeight = 0f;
				for (int i = 0; i < range; i++) sumWeight += _rangeWeightsProp.GetArrayElementAtIndex(i).floatValue;

				EditorGUILayout.BeginVertical(GUI.skin.box);
				
				for (int i = 0; i < range; i++)
				{
					var rolls = min + i;
					var element = _rangeWeightsProp.GetArrayElementAtIndex(i);

					EditorGUILayout.BeginHorizontal(GUI.skin.box);
					GuiLabelWidthManager.New(72);
					var newVal = EditorGUILayout.FloatField($"[{rolls}]Weight", element.floatValue, GUILayout.Width(120));
					GuiLabelWidthManager.Revert();
					element.floatValue = newVal;
					GUIProgressBar.DrawLayout( newVal / sumWeight );
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
		}

		EditorGUILayout.EndVertical();
	}
	
	void FillCaches()
	{
		if (!_isDirty) return;
		var count = _entriesProp.arraySize;

		if (_minCache == null || _minCache.Length != count) _minCache = new int[count];
		if (_maxCache == null || _maxCache.Length != count) _maxCache = new int[count];
		if (_namesCache == null || _namesCache.Length != count) _namesCache = new string[count];
		if (_chancesCache == null || _chancesCache.Length != count) _chancesCache = new float[count];

		for (int i = 0; i < count; i++)
		{
			var element = _entriesProp.GetArrayElementAtIndex(i);
			_chancesCache[i] = element.FindPropertyRelative("_weight").floatValue / _sumWeight;
			_minCache[i] = element.FindPropertyRelative("_guaranteed").intValue;
			_maxCache[i] = element.FindPropertyRelative("_cap").intValue;
			var objRef = element.FindPropertyRelative("_element").objectReferenceValue;
			_namesCache[i] = objRef.DebugNameOrNull();
		}

		if (!IsBiased)
		{
			_rollChancesCache = null;
			return;
		}

		var rCount = _rangeWeightsProp.arraySize;
		if( _rollChancesCache == null || _rollChancesCache.Length != rCount ) _rollChancesCache = new float[rCount];

		for (int i = 0; i < rCount; i++)
		{
			var element = _rangeWeightsProp.GetArrayElementAtIndex(i);
			_rollChancesCache[i] = element.floatValue;
		}

		_isDirty = false;
    }

	private void DrawPredictions( System.Action<int, int[], int[], float[], int, int, double[,,]> Calculation )
	{
		var count = _entriesProp.arraySize;

		int[] min = new int[count];
		int[] max = new int[count];
		float[] chances = new float[count];

		var rMin = Mathf.Max( _minProp.intValue, 0 );
		var rMax = Mathf.Max( _maxProp.intValue, 0 );
		if (IsFixedRolls) rMin = rMax;
		var maxElements = rMax;

		int minSum = 0;

		for( int i = 0; i < count; i++ )
		{
			var element = _entriesProp.GetArrayElementAtIndex( i );
			chances[i] = element.FindPropertyRelative( "_weight" ).floatValue / _sumWeight;
			min[i] = element.FindPropertyRelative( "_guaranteed" ).intValue;
			max[i] = element.FindPropertyRelative( "_cap" ).intValue;

			var realCap = rMax;
			if( max[i] != 0 ) realCap = Mathf.Min( realCap, max[i] );
			if( realCap < min[i] ) realCap = min[i];

			maxElements = Mathf.Max( maxElements, realCap );
			minSum += min[i];
		}

		double[,,] M = new double[count, maxElements + 1, rMax + 1];
		Calculation( count, min, max, chances, rMax, maxElements, M );

		double[] acc = new double[rMax + 1];
		CalculateAcc( count, rMax, maxElements, M, acc );

		DrawPredictionsTable( count, rMin, rMax, maxElements, M, acc );
	}

	static void RebasePoolElements( float newBase, List<(int id, int max, float chance)> pool )
	{
		for( int i = 0; i < pool.Count; i++ )
		{
			var element = pool[i];
			element.chance /= newBase;
			pool[i] = element;
		}
	}

	static void TryExpand( double chance, int total, int maxTotal, int[] elements, List<int> sequence, List<(int id, int max, float chance)> pool, double[,,] M )
	{
		var newTotal = total + 1;
		if( newTotal > maxTotal ) return;

		for( int i = 0; i < pool.Count; i++ )
		{
			var e = pool[i];

			var newCount = elements[e.id] + 1;
			elements[e.id] = newCount;
			var removeFromPool = e.max != 0 && newCount >= e.max;
			if( removeFromPool )
			{
				pool.RemoveAt( i );
				RebasePoolElements( 1 - e.chance, pool );
			}

			chance *= e.chance;
			sequence.Add( e.id );
			// var countStr = string.Join( ", ", System.Array.ConvertAll<int, string>( sequence.ToArray(), ( int v ) => v.ToString() ) );
			// Debug.Log( $"{{ {countStr} }}[{sequence.Count}] => {( chance * 100 ):N1}%" );

			for( int j = 0; j < elements.Length; j++ )
			{
				var c = elements[j];
				M[j, c, newTotal] += chance;
			}

			TryExpand( chance, newTotal, maxTotal, elements, sequence, pool, M );

			sequence.RemoveAt( sequence.Count - 1 );
			chance /= e.chance;
			elements[e.id] = newCount - 1;
			if( removeFromPool )
			{
				RebasePoolElements( 1 / ( 1 - e.chance ), pool );
				pool.Insert( i, e );
			}
		}
	}

	private static void CalculateBacktracking( int count, int[] min, int[] max, float[] chances, int rMax, int maxElements, double[,,] Predictions )
	{
		var pool = new List<(int id, int max, float chance)>();
		var sequence = new List<int>();
		var elements = new int[count];
		var total = 0;

		var basePercentage = 1f;
		for( int i = 0; i < count; i++ )
		{
			var g = min[i];
			elements[i] = g;
			total += g;
			for( int j = 0; j < g; j++ ) sequence.Add( i );
			var chance = chances[i];
			var addToPool = ( ( max[i] == 0 || g < max[i] ) && chance > 0 );
			if( addToPool ) pool.Add( (i, max[i], chance) );
			else basePercentage -= chance;
		}

		RebasePoolElements( basePercentage, pool );

		for( int i = 0; i <= total && i <= rMax; i++ )
		{
			for( int j = 0; j < count; j++ )
			{
				var c = elements[j];
				Predictions[j, c, i] = 1;
			}
		}

		TryExpand( 1, total, rMax, elements, sequence, pool, Predictions );
	}

	private static void CalculateAcc( int count, int rMax, int maxElements, double[,,] M, double[] acc )
	{
		for( int i = 0; i < count; i++ )
		{
			for( int k = 1; k <= rMax; k++ )
			{
				for( int j = 0; j <= maxElements; j++ )
				{
					acc[k] += M[i, j, k];
				}
			}
		}
	}

	private void DrawPredictionsTable( int count, int rMin, int rMax, int maxElements, double[,,] Predictions, double[] acc )
	{
		string[] name = new string[count];
		for( int i = 0; i < count; i++ )
		{
			var element = _entriesProp.GetArrayElementAtIndex( i );
			var group = element.FindPropertyRelative( "_element" );
			var obj = group.objectReferenceValue;
			if (obj == null) name[i] = "NOTHING";
			else name[i] = obj.DebugNameOrNull("NOTHING");
		}

		var lh = EditorGUIUtility.singleLineHeight;
		GuiColorManager.New( GREEN );
		EditorGUILayout.BeginVertical( GUI.skin.box );
		GuiColorManager.Revert();

		if( !ToogleButton( _hidePredictions, "Chances Prediction", "icons/dices.png" ) )
		{
			int minE = int.MaxValue;
			int maxE = int.MinValue;
			for( int k = rMin; k <= rMax; k++ )
			{
				for( int i = 0; i < count; i++ )
				{
					for( int j = 0; j <= maxElements; j++ )
					{
						if( Mathf.Approximately( (float)Predictions[i, j, k], 0 ) ) continue;
						minE = Mathf.Min( j, minE );
						maxE = Mathf.Max( j, maxE );
					}
				}
			}

			var slices = ( maxE - minE ) + 2;
			if( rMin <= rMax )
			{
				var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
				rect.RequestTop( lh + 3 );
				GUILayout.Space( lh + 3 );
				GUI.Label( rect.VerticalSlice( 0, slices ), "Average", GUI.skin.box );
				for( int j = minE; j <= maxE; j++ ) GUI.Label( rect.VerticalSlice( ( j - minE ) + 1, slices ), $"{j}", GUI.skin.box );
				EditorGUILayout.EndHorizontal();
				
				var range = Mathf.Max( rMax - rMin, 0 ) + 1;
				float sumWeight = range;
				
				if (IsBiased && _rangeWeightsProp.arraySize >= range)
				{
					sumWeight = 0;
					for (int i = 0; i < range; i++)
					{
						sumWeight += _rangeWeightsProp.GetArrayElementAtIndex(i).floatValue;
					}
				}

				for( int i = 0; i < count; i++ )
				{
					rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh ) );
					rect.RequestTop( lh );
					GUILayout.Space( lh );
					GUI.Label( rect.VerticalSlice( 0, slices ), name[i] );
					for( int j = minE; j <= maxE; j++ )
					{
						double val = 0;
						for (int k = rMin; k <= rMax; k++)
						{
							var chance = Predictions[i, j, k];
							if (IsBiased) chance *= _rangeWeightsProp.GetArrayElementAtIndex(k - rMin).floatValue;
							val += chance;
						}
						val /= sumWeight;
						val = System.Math.Max( val, 0 );

						var r = rect.VerticalSlice( ( j - minE ) + 1, slices );
						GUIProgressBar.Draw( r, (float)val );
					}
					EditorGUILayout.EndHorizontal();
				}
			}

			if( rMin < rMax && ToogleButton( _showDetails, "Detailed rolls", UnityIcons.ViewToolZoomOn, UnityIcons.ViewToolZoom ) )
			{
				for( int k = rMin; k <= rMax; k++ )
				{
					var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
					rect.RequestTop( lh + 3 );
					GUILayout.Space( lh + 3 );
					GUI.Label( rect.VerticalSlice( 0, slices ), $"{k} Roll{(k>1?"s":"")}", GUI.skin.box );
					for( int j = minE; j <= maxE; j++ ) GUI.Label( rect.VerticalSlice( ( j - minE ) + 1, slices ), $"{j}", GUI.skin.box );
					EditorGUILayout.EndHorizontal();

					for( int i = 0; i < count; i++ )
					{
						// double prob = 0;
						// for( int j = minE; j <= maxE; j++ ) prob += M[i, j, k];

						rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh ) );
						rect.RequestTop( lh );
						GUILayout.Space( lh );
						// var probLabel = $"{( prob * 100 ):N1}%";
						// GUI.Label( rect.VerticalSlice( 0, slices ), probLabel + " " + name[i] );
						GUI.Label( rect.VerticalSlice( 0, slices ), name[i] );
						for( int j = minE; j <= maxE; j++ )
						{
							var val = Predictions[i, j, k];
							var r = rect.VerticalSlice( ( j - minE ) + 1, slices );
							GUIProgressBar.Draw( r, (float)val );
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}

			if( ToogleButton( _showPermutations, "Permutations", UnityIcons.d_SortingGroup_Icon, UnityIcons.SortingGroup_Icon ) )
			{
				Permutations( rMin, rMax );
				_permutations.Sort(PermutationSorting);
				EditorGUILayout.BeginVertical();
				var maxSize = 24f;
        		GUIStyle labelStyle = GUI.skin.label;
				foreach(var permutation in _permutations ) maxSize = MathAdapter.max( maxSize, labelStyle.CalcSize(new GUIContent(permutation.name)).x );
				var labelWidth = GUILayout.Width(maxSize);
				var labelHeight = GUILayout.Height( EditorGUIUtility.singleLineHeight );

				foreach(var permutation in _permutations )
                {
					EditorGUILayout.BeginHorizontal( labelHeight );
					GUILayout.Label( permutation.name, labelStyle, labelWidth, labelHeight );
					GUIProgressBar.DrawLayout( permutation.percentage, GUILayout.MinWidth( 32 ) );
					EditorGUILayout.EndHorizontal();
                }
				EditorGUILayout.EndVertical();
            }
		}

		EditorGUILayout.EndVertical();
	}

    private int PermutationSorting((string, float) x, (string, float) y) => y.Item2.CompareTo( x.Item2 );

    int[] _countCache;
	float[] _rollChancesCache;
	int[] _minCache;
	int[] _maxCache;
	float[] _chancesCache;
	string[] _namesCache;
	List<(string name, float percentage)> _permutations = new();

	void Permutations( int rMin, int rMax )
	{
		var rollsCount = rMax + 1 - rMin;
		var defaultChance = 1f / rollsCount;
		var rChanLen = _rollChancesCache?.Length ?? 0;
		var biased = rChanLen > 0;

		var count = _minCache.Length;
		if( _countCache == null || _countCache.Length < count ) _countCache = new int[count];

		var baseMins = 0;
		for (int i = 0; i < _minCache.Length; i++)
		{
			baseMins += _minCache[i];
			_countCache[i] = _minCache[i];
		}

		_permutationChances.Clear();

		var rwSum = 0f;
		for (int i = 0; i < rChanLen; i++) rwSum += _rollChancesCache[i];

        for( int i = 0; i < rollsCount; i++ )
		{
			var rolls = i + rMin;
			var chance = defaultChance;
			if( biased ) chance = _rollChancesCache[ Mathf.Min( i, rChanLen - 1 )] / rwSum;
			Permute(baseMins, rolls, chance );
        }

		_permutations.Clear();
		foreach( var e in _permutationChances ) _permutations.Add( ( e.Key, e.Value ) );
		_permutationChances.Clear();
    }

	Dictionary<string, float> _permutationChances = new();
	
	void Permute( int it, int maxPermute, float currChance )
	{
		if( it >= maxPermute )
		{
			var name = NameCombination(_countCache, _namesCache, it);
			_permutationChances.TryGetValue(name, out var chance);
			_permutationChances[name] = chance + currChance;
			return;
        }
		var sumWeights = 0f;

		var count = _maxCache.Length;
		for( int k = 0; k < count; k++ )
		{
			if (_maxCache[k] <= _countCache[k]) continue;
			var eChan = _chancesCache[k];
			sumWeights += eChan;
		}
		
		for( int k = 0; k < count; k++ )
		{
			if (_maxCache[k] <= _countCache[k]) continue;
			var eChan = _chancesCache[k];
			_countCache[k]++;
			Permute( it + 1, maxPermute, currChance * ( eChan / sumWeights ) );
			_countCache[k]--;
		}
    }

    private string NameCombination( int[] _countCache, string[] names, int maxRolls = int.MaxValue)
	{
		var sb = StringBuilderPool.RequestEmpty();

		var first = true;
		for (int i = 0; i < _countCache.Length; i++)
		{
			var count = Mathf.Min( _countCache[i], maxRolls );
			if (count > 0)
			{
				if( !first ) sb.Append( ", " );
				sb.Append($"{count} {names[i]}");
				first = false;
			}
			maxRolls -= count;
		}

		return sb.ReturnToPoolAndCast();
    }

    bool ToogleButton( IValueCapsule<bool> toggleValue, string label, string iconNameOn, string iconNameOff = null )
    {
		var toggle = toggleValue.Get;
		var content = new GUIContent( label, IconCache.Get( toggle ? iconNameOn : ( iconNameOff ?? iconNameOn ) ).Texture );
		var lh = EditorGUIUtility.singleLineHeight;
		var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
		rect.RequestTop( lh + 3 );
		GUILayout.Space( lh + 3 );
		var change = GUI.Button( rect, content, K10GuiStyles.smallBoldCenterStyle );
		if( change ) toggleValue.Set = !toggle;
		EditorGUILayout.EndHorizontal();
		return toggleValue.Get;
    }
}
