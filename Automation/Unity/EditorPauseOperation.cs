using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
	public class EditorPauseOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine( bool log = false ) 
		{
	#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPaused = true;
	#endif //UNITY_EDITOR
			if( false ) yield return null;
		}

		public override string ToString() => $"✋ {"EditorPauseOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
