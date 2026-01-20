using System.Collections;
using UnityEngine;

namespace K10.Automation.Unity
{
    public class DebugBreakOperation : Automation.BaseOperation
	{
		public override IEnumerator ExecutionCoroutine( bool log = false )
		{
			Debug.Break();

#pragma warning disable CS0162 // Unreachable code detected
			if( false ) yield return null;
#pragma warning restore CS0162 // Unreachable code detected
		}

		public override string ToString() => $"💥 {"DebugBreakOperation".Colorfy( Colors.Console.Verbs )}";
	}
}