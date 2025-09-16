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

#pragma warning disable CS0162 // Unreachable code detected
			if( false ) yield return null;
#pragma warning restore CS0162 // Unreachable code detected
		}

		public override string ToString() => $"✋ {"EditorPauseOperation".Colorfy( Colors.Console.Verbs )}";

		public Object[] LogOwners { get; } = { null };
	}
}