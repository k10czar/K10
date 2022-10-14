using System.Collections;


namespace Unity.Automation
{
	public class EditorPauseOperation : K10.Automation.Operation
	{
		public override IEnumerator ExecutionCoroutine()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPaused = true;
#endif //UNITY_EDITOR
			if( false ) yield return null;
		}
	}
}
