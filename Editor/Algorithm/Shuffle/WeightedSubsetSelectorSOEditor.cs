using UnityEditor;
using UnityEngine;
using System;

public class WeightedSubsetSelectorSOEditor<T> : WeightedSubsetSelectorSOEditor
{
	protected override Type ElementType => typeof(T);
}

[CustomEditor(typeof(BaseWeightedSubsetSelectorSO))]
public class WeightedSubsetSelectorSOEditor : Editor
{
	WeightedSubsetSelectorEditor _editor;

	protected virtual Type ElementType => typeof(ScriptableObject);

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		_editor?.OnInspectorGUI( target as ISubsetSelector );
		serializedObject.ApplyModifiedProperties();
	}

	public virtual void OnEnable()
	{
		if (_editor == null) _editor = new( ElementType );
		_editor.Setup(serializedObject);
	}

    void OnDisable()
    {
		serializedObject.Update();
		_editor?.CleanOldRanges();
		serializedObject.ApplyModifiedProperties();
    }
}
