using System.Collections;
using K10.Common;
using UnityEngine;

namespace Automation.Unity
{
	public class WaitForSecondsOperation : Automation.IOperation
	{
		[SerializeField] float _seconds;

		public IEnumerator ExecutionCoroutine( bool log = false )
		{
			yield return new WaitForSecondsRealtime( _seconds );
		}

		public override string ToString() => $"⏰ {"WaitForSecondsOperation".Colorfy( Colors.Console.Verbs )} {_seconds.ToStringColored( Colors.Console.Numbers )}s";

		public Object[] LogOwners { get; } = { null };
	}
}