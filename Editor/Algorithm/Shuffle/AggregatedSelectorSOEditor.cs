using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseAggregatedSelectorSO),true)]
public class CompoundAggregatedSelectorSOEditor<T,K> : Editor where T : ScriptableObject, IAggregatedSubsetSelector<K>
{
	CompoundAggregatedSelectorEditor<T,K> _editor;

	protected virtual Func<SerializedProperty,Color> ElementColoringFunc => null;
	protected virtual IAggregatedPredictor<T> Predictor => null;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		_editor?.OnInspectorGUI( target as IAggregatedSubsetSelector );
		serializedObject.ApplyModifiedProperties();
	}

	public virtual void OnEnable()
	{
		var obj = target as IAggregatedSubsetSelector;
		if (_editor == null) _editor = new( obj.ElementType, ElementColoringFunc, Predictor );
		_editor.Setup(serializedObject);
	}
}

[CustomEditor(typeof(BaseAggregatedSelectorSO),true)]
public class AggregatedSelectorSOEditor<T> : Editor where T : ScriptableObject
{
	AggregatedSelectorEditor<T> _editor;

	protected virtual Func<SerializedProperty,Color> ElementColoringFunc => null;
	protected virtual IAggregatedPredictor<T> Predictor => new AggregatedPredictor<T>();

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		_editor?.OnInspectorGUI( target as IAggregatedSubsetSelector );
		serializedObject.ApplyModifiedProperties();
	}

	public virtual void OnEnable()
	{
		var obj = target as IAggregatedSubsetSelector;
		if (_editor == null) _editor = new( obj.ElementType, ElementColoringFunc, Predictor );
		_editor.Setup(serializedObject);
	}
}