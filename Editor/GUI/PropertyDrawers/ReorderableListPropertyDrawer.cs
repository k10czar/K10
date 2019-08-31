using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

public static class EditorExtention
{
	public static float Height( this ReorderableList list )
	{
		if( list == null || list.serializedProperty == null ) return 10;
		var bodyHeight = 0f;

		if( list.elementHeightCallback != null ) for( int i = 0; i < list.serializedProperty.arraySize; i++ ) bodyHeight += list.elementHeightCallback( i );
		else bodyHeight = list.serializedProperty.arraySize * list.elementHeight;

		return list.headerHeight + Mathf.Max( bodyHeight, list.elementHeight ) + list.footerHeight + 10;
	}
}

public abstract class ReorderableListPropertyDrawer : PropertyDrawer
{
	static bool ANALISE = false;
	static readonly Dictionary<SerializedProperty, Analysis> _analysis = new Dictionary<SerializedProperty, Analysis>();
	System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();


	public class Analysis
	{
		float _onGui;
		float _propHeight;
		float _time;

		public bool IsValid => Time.time < ( _time + .02f );
		public float TimeSum => _onGui + _propHeight;

		public void SetHeightTime( float ms )
		{
			_propHeight = ms;
			_time = Time.time;
		}

		public void SetOnGUI( float ms )
		{
			_onGui = ms;
			_time = Time.time;
		}

		public void Draw( Rect area, float totalTime )
		{
			EditorGUI.LabelField( area, $"( {TimeSum}ms = {_propHeight}ms + {_onGui}ms ) / {totalTime}ms" );
		}
	}

	private readonly ReorderableListCollection _listCollection = new ReorderableListCollection();

	public Analysis RequestAnalysis( SerializedProperty property )
	{
		Analysis a;
		if( _analysis.TryGetValue( property, out a ) ) return a;
		a = new Analysis();
		_analysis[property] = a;
		return a;
	}

	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		if( ANALISE )
		{
			_stopwatch.Reset();
			_stopwatch.Start();
		}
		var list = RequestList( property );
		list.DoList( area );
		if( ANALISE )
		{
			_stopwatch.Stop();
			var ms = _stopwatch.ElapsedMilliseconds;
			var a = RequestAnalysis( property );
			a.SetOnGUI( ms );

			var areaProf = area.RequestBottom( 16 );
			areaProf.x = 100;
			// a.Draw( area.RequestBottom( 16 ).HorizontalShrink( 150 ), CalculateTotalTime() );
			a.Draw( areaProf, CalculateTotalTime() );
		}
	}

	private float CalculateTotalTime()
	{
		var total = 0f;
		
		var toRemove = new List<SerializedProperty>();
		foreach( var kvp in _analysis )
		{
			if( kvp.Value.IsValid ) total += kvp.Value.TimeSum;
			else toRemove.Add( kvp.Key );
		}

		for( int i = 0; i < toRemove.Count; i++ ) _analysis.Remove( toRemove[i] );

		return total;
	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		if( ANALISE )
		{
			_stopwatch.Reset();
			_stopwatch.Start();
		}
		var list = RequestList( property );
		return list.Height();
		if( ANALISE )
		{
			_stopwatch.Stop();
			var ms = _stopwatch.ElapsedMilliseconds;
			var a = RequestAnalysis( property );
			a.SetHeightTime( ms );
		}
	}

	protected UnityEditorInternal.ReorderableList RequestList( SerializedProperty prop ) { return _listCollection.Request( prop, CreateNewList ); }

	protected virtual SerializedProperty GetArrayProperty( SerializedProperty prop ) { return prop; }

	protected virtual ReorderableList CreateNewList( SerializedProperty prop )
	{
		var p = GetArrayProperty( prop );
		var list = new ReorderableList( prop.serializedObject, p, true, true, true, true );

		list.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
		{
			var element = p.GetArrayElementAtIndex( index );
			EditorGUI.PropertyField( rect, element );
		};

		list.elementHeight = EditorGUIUtility.singleLineHeight;

		return list;
	}
}