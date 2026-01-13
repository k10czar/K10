using UnityEditor;

public interface IGetHeight
{
	float GetHeight();
}

[CustomEditor(typeof(BaseAggregatedSelectorSO),true)]
public class AggregatedSelectorSOEditor : Editor, IGetHeight
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

    public float GetHeight() => _editor?.GetHeight(target as IAggregatedSubsetSelector ) ?? EditorGUIUtility.singleLineHeight;
}
