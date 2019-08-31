using UnityEditor;


[CustomEditor( typeof( MonoBehaviourState ) )]
public class MonoBehaviourStateEditor : UnityEditor.Editor
{

	public override void OnInspectorGUI()
	{
		var state = target as MonoBehaviourState;
		K10EditorGUIUtils.Semaphore( state.IsAlive, "IsAlive" );
		K10EditorGUIUtils.Semaphore( state.IsActive, "IsActive" );
	}
}