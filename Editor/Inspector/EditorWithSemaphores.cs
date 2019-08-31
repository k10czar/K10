
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
public class EditorWithSemaphores<T> : UnityEditor.Editor where T : Component
{
	public override void OnInspectorGUI()
	{
		K10EditorGUIUtils.DrawReactiveProperties<T>( target );
		DrawDefaultInspector();
		EditorUtility.SetDirty( target );
	}
}
