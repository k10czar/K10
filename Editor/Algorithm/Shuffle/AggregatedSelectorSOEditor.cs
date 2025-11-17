using UnityEditor;

[CustomEditor(typeof(BaseAggregatedSelectorSO),true)]
public class AggregatedSelectorSOEditor : Editor
{
	AggregatedSelectorEditor _editor;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		_editor?.OnInspectorGUI( target as IAggregatedSubsetSelector );
		serializedObject.ApplyModifiedProperties();
	}

	public virtual void OnEnable()
	{
		var obj = target as IAggregatedSubsetSelector;
		if (_editor == null) _editor = new( obj.ElementType );
		_editor.Setup(serializedObject);
	}
}
