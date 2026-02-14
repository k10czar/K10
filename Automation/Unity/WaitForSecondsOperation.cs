using System.Collections;
using UnityEngine;

namespace K10.Automation.Unity
{
	[ListingPath("Wait/ForSeconds")]
	public class WaitForSecondsOperation : K10.Automation.BaseOperation
	{
		[SerializeField] float _seconds = 1;

		public override IEnumerator ExecutionCoroutine( bool log = false )  
		{ 
			yield return new WaitForSecondsRealtime( _seconds );
		}
		
		public override string ToString() => $"⏰ {"WaitForSecondsOperation".Colorfy( Colors.Console.Verbs )} {_seconds.ToStringColored( Colors.Console.Numbers )}s";
	}
}
