
using System.Collections.Generic;
using UnityEditor;

[CanEditMultipleObjects]
public class EditorWithReactiveVariables<T> : Editor where T : class
{
	private readonly List<ISimpleLayoutDrawerElement> _drawers = new List<ISimpleLayoutDrawerElement>();

	void OnEnable()
	{
		_drawers.Clear();

		if( targets.Length == 1 )
		{
			_drawers.Add( new ReactivePropertiesDrawer<T>( target, "Reactive Variables" ) );
		}
		else
		{
			for( int i = 0; i < targets.Length; i++ )
			{
				var t = targets[i];
				_drawers.Add( new ReactivePropertiesDrawer<T>( t, $"{t.ToStringOrNull()} - Reactive Variables" ) );
			}
		}
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		for( int i = 0; i < _drawers.Count; i++ )
		{
			var drawer = _drawers[i];
			drawer.Layout();
		}

		EditorUtility.SetDirty( target );
	}
}