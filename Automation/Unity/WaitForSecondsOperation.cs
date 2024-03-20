using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
	public class WaitForSecondsOperation : Automation.IOperation
	{
		[SerializeField] float _seconds;

		public IEnumerator ExecutionCoroutine()  
		{ 
			yield return new WaitForSecondsRealtime( _seconds );
		}
		
		public string GetSummaryColored() => $"⏰ {"WaitForSecondsOperation".Colorfy( Colors.Console.Verbs )} {_seconds.ToStringColored( Colors.Console.Numbers )}s";
	}
}
