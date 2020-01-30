using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class GuiRaycastInspect : EditorWindow
{
	Inspection _inspection;
	private static Vector2 _scroll;
	private static readonly HashSet<string> _opens = new HashSet<string>();

	class Inspection
	{
		int _objectCount;
		int _actives;
		int _realyActives;

		List<Branch> _branches = new List<Branch>();

		public Inspection( GameObject[] roots )
		{
			_objectCount = 0;
			_actives = 0;
			_realyActives = 0;
			foreach( var go in roots )
			{
				var branches = Branch.Build( go.transform );
				for( int i = 0; i < branches.Count; i++ )
				{
					var branch = branches[i];
					_objectCount += branch.Count;
					branch.CalculateActives();
					_actives += branch.ActivesCount;
					_realyActives += branch.RealyActivesCount;
				}
				_branches.AddRange( branches );
			}
		}

		public void OnGUI()
		{
			var percentage = 0;
			if( _objectCount > 0 ) percentage = ( 100 * _actives ) / _objectCount;
			var realyRercentage = 0;
			if( _objectCount > 0 ) realyRercentage = ( 100 * _realyActives ) / _objectCount;
			GUILayout.Label( $"{percentage}%({_actives}/{_objectCount}) realy:{realyRercentage}%({_realyActives}/{_objectCount})", K10GuiStyles.bigBoldCenterStyle );

			K10.EditorGUIExtention.SeparationLine.Horizontal();

			_actives = 0;
			_realyActives = 0;
			_scroll = GUILayout.BeginScrollView( _scroll );
			for( int i = 0; i < _branches.Count; i++ )
			{
				var branch = _branches[i];
				var greyout = branch.ActivesCount == 0;
				if( greyout ) GuiColorManager.New( Color.grey );
				branch.OnGUI( true );
				if( greyout ) GuiColorManager.Revert();
				branch.CalculateActives();
				_actives += branch.ActivesCount;
				_realyActives += branch.RealyActivesCount;
			}
			GUILayout.EndScrollView();

		}

		public class Branch
		{
			CanvasGroup _canvas;
			List<Graphic> _graphics = new List<Graphic>();
			List<Branch> _branches = new List<Branch>();
			Transform _transform;

			int _cacheCount;
			public int Count => _cacheCount;

			int _childActivesCount;
			int _siblingsActivesCount;
			public int ActivesCount => _siblingsActivesCount + _childActivesCount;

			int _childRealyActivesCount;
			int _siblingsRealyActivesCount;
			public int RealyActivesCount => _siblingsRealyActivesCount + _childRealyActivesCount;

			private Branch( Transform t, CanvasGroup canvas, Graphic[] graphics )
			{
				_transform = t;
				_canvas = canvas;
				_graphics.AddRange( graphics );
				_branches.AddRange( BuildChilds( t ) );
				var childsCount = _branches.Count;
				for( int i = 0; i < childsCount; i++ ) _cacheCount += _branches[i].Count;
				if( _canvas != null ) _cacheCount++;
				_cacheCount += _graphics.Count;
			}

			internal void CalculateActives()
			{
				_childActivesCount = 0;
				_childRealyActivesCount = 0;
				_siblingsActivesCount = 0;
				_siblingsRealyActivesCount = 0;
				var active = _transform.gameObject.activeInHierarchy;
				for( int i = 0; i < _branches.Count; i++ )
				{
					_branches[i].CalculateActives();
					_childActivesCount += _branches[i].ActivesCount;
					if( active ) _childRealyActivesCount += _branches[i].RealyActivesCount;
				}
				if( _canvas != null && ( _canvas.blocksRaycasts || _canvas.interactable ) ) _siblingsActivesCount++;
				for( int i = 0; i < _graphics.Count; i++ ) if( _graphics[i].raycastTarget ) _siblingsActivesCount++;
				if( !active ) return;
				if( _canvas != null && _canvas.enabled && ( _canvas.blocksRaycasts || _canvas.interactable ) ) _siblingsRealyActivesCount++;
				for( int i = 0; i < _graphics.Count; i++ ) if( _graphics[i].raycastTarget && _graphics[i].enabled ) _siblingsRealyActivesCount++;
			}

			public static List<Branch> BuildChilds( Transform t )
			{
				List<Branch> branches = new List<Branch>();
				var childsCount = t.childCount;
				for( int i = 0; i < childsCount; i++ ) branches.AddRange( Build( t.GetChild( i ) ) );
				return branches;
			}

			public static List<Branch> Build( Transform t )
			{
				List<Branch> branches = new List<Branch>();
				var c = t.GetComponent<CanvasGroup>();
				var gs = t.GetComponents<Graphic>();
				// if( c == null && gs.Length == 0 ) branches.AddRange( BuildChilds( t ) );
				// else 
				branches.Add( new Branch( t, c, gs ) );
				return branches;
			}

			public void Force( bool t )
			{
				if( _canvas != null )
				{
					if( t != _canvas.interactable )
					{
						_canvas.interactable = t;
						EditorUtility.SetDirty( _canvas );
					}

					if( t != _canvas.blocksRaycasts )
					{
						_canvas.blocksRaycasts = t;
						EditorUtility.SetDirty( _canvas );
					}
				}

				for( int i = 0; i < _graphics.Count; i++ )
				{
					var g = _graphics[i];
					if( t != g.raycastTarget )
					{
						g.raycastTarget = t;
						EditorUtility.SetDirty( g );
					}
				}

				for( int i = 0; i < _branches.Count; i++ )
				{
					_branches[i].Force( t );
				}
			}

			public void OnGUI( bool first = false )
			{
				var name = _transform.HierarchyNameOrNull();
				var toggle = _opens.Contains( name );

				var newToggle = false;

				var percentage = 0;
				if( Count > 0 ) percentage = ( 100 * ActivesCount ) / Count;
				var realyRercentage = 0;
				if( Count > 0 ) realyRercentage = ( 100 * RealyActivesCount ) / Count;

				var label = $"{_transform.name} {percentage}%({ActivesCount}/{Count}) r:{realyRercentage}%({RealyActivesCount}/{Count})";

				if( first ) newToggle = EditorGUILayout.BeginFoldoutHeaderGroup( toggle, label );
				else
				{
					EditorGUILayout.BeginHorizontal();
					newToggle = EditorGUILayout.Foldout( toggle, label );
				}

				if( newToggle != toggle )
				{
					if( newToggle ) _opens.Add( name );
					else { _opens.Remove( name ); }
				}

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space( EditorGUI.indentLevel * 15 );
				GUILayout.Label( "Force", GUILayout.Width( 40 ) );
				GUILayout.Space( 5 );
				if( GUILayout.Button( "Enable", GUILayout.Width( 60 ) ) ) Force( true );
				GUILayout.Space( 5 );
				if( GUILayout.Button( "Disable", GUILayout.Width( 60 ) ) ) Force( false );
				EditorGUILayout.EndHorizontal();

				if( !first ) EditorGUILayout.EndHorizontal();


				if( !newToggle )
				{
					if( first ) EditorGUILayout.EndFoldoutHeaderGroup();
					return;
				}

				EditorGUI.indentLevel++;

				if( _canvas != null )
				{
					var t = EditorGUILayout.ToggleLeft( "canvas.interactable", _canvas.interactable );
					if( t != _canvas.interactable )
					{
						_canvas.interactable = t;
						EditorUtility.SetDirty( _canvas );
					}
					t = EditorGUILayout.ToggleLeft( "canvas.blocksRaycasts", _canvas.blocksRaycasts );
					if( t != _canvas.blocksRaycasts )
					{
						_canvas.blocksRaycasts = t;
						EditorUtility.SetDirty( _canvas );
					}
				}

				for( int i = 0; i < _graphics.Count; i++ )
				{
					var g = _graphics[i];
					var t = EditorGUILayout.ToggleLeft( $"{g.GetType()}.raycastTarget", g.raycastTarget );
					if( t != g.raycastTarget )
					{
						g.raycastTarget = t;
						EditorUtility.SetDirty( g );
					}
				}

				for( int i = 0; i < _branches.Count; i++ )
				{
					_branches[i].OnGUI();
				}

				if( first ) EditorGUILayout.EndFoldoutHeaderGroup();
				EditorGUI.indentLevel--;
			}
		}
	}

	[MenuItem( "K10/GuiRaycastInspect" )]
	private static void Init()
	{
		GetWindow<GuiRaycastInspect>( "Gui Raycast Inspect" );
	}

	void OnSelectionChange()
	{
		_inspection = new Inspection( Selection.gameObjects );
		EditorUtility.SetDirty( this );
		this.Repaint();
	}

	void OnEnable()
	{
		_inspection = new Inspection( Selection.gameObjects );
	}

	private void OnGUI()
	{
		GUILayout.Label( "Gui Raycast Inspect", K10GuiStyles.bigBoldCenterStyle );
		K10.EditorGUIExtention.SeparationLine.Horizontal();

		if( _inspection != null ) _inspection.OnGUI();
		else GUILayout.Label( "No Inspection", K10GuiStyles.bigBoldCenterStyle );
	}
}