using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
	public class WaitForEndOfFrameOperation : Automation.IOperation
	{
		public IEnumerator ExecutionCoroutine( bool log = false )
		{
			yield return new UnityEngine.WaitForEndOfFrame();
		}

		public override string ToString() => $"🖼 {"WaitForEndOfFrameOperation".Colorfy( Colors.Console.Verbs )}";

		public Object[] LogOwners { get; } = { null };
	}
}