using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
	public class EditorPauseOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine() 
		{
	#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPaused = true;
	#endif //UNITY_EDITOR
			if( false ) yield return null;
		}

		public string GetSummaryColored() => $"✋ {"EditorPauseOperation".Colorfy( Colors.Console.Verbs )}";
	}
}
