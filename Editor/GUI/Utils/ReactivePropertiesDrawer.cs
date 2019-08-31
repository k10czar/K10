using UnityEditor;

public interface ISimpleLayoutDrawerElement
{
	void Layout();
}

public class ReactivePropertiesDrawer<T> : ISimpleLayoutDrawerElement where T : class
{
	IValueCapsule<bool> _toggle;
	object _object;
	string _label;

	public ReactivePropertiesDrawer( object target ) : this( target, typeof( T ).ToString() ) { }
	public ReactivePropertiesDrawer( object target, string label )
	{
		_label = label;
		_toggle = PersistentValue<bool>.At( $"Temp/Inspector/{label}_details.bool" );
		_object = target;
	}

	public void Layout()
	{
#if UNITY_2019_1_OR_NEWER
		_toggle.Set = EditorGUILayout.BeginFoldoutHeaderGroup( _toggle.Get, _label );
#else
		_toggle.Set = EditorGUILayout.Foldout( _toggle.Get, _label );
#endif
		if( _toggle.Get ) K10EditorGUIUtils.DrawReactiveProperties<T>( _object );

#if UNITY_2019_1_OR_NEWER
		EditorGUILayout.EndFoldoutHeaderGroup();
#endif
	}
}