using UnityEngine;

namespace Automation.Unity
{
	public class ChangeTimeScaleOperation : Automation.IOperation
	{
		[UnityEngine.SerializeField] float _timeScale = 1;

		public System.Collections.IEnumerator ExecutionCoroutine( bool log = false )  
		{
			Time.timeScale = _timeScale; yield return null;
		}

		public override string ToString() => $"⏳ {"ChangeTimeScaleOperation".Colorfy( Colors.Console.Verbs )} to {_timeScale.ToStringColored( Colors.Console.Numbers )}";
	}
}
