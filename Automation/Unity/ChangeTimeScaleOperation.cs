using UnityEngine;

namespace K10.Automation.Unity
{
	public class ChangeTimeScaleOperation : Automation.BaseOperation
	{
		[UnityEngine.SerializeField] float _timeScale = 1;

		public override System.Collections.IEnumerator ExecutionCoroutine( bool log = false )  
		{
			Time.timeScale = _timeScale; yield return null;
		}

		public override string ToString() => $"⏳ {"ChangeTimeScaleOperation".Colorfy( Colors.Console.Verbs )} to {_timeScale.ToStringColored( Colors.Console.Numbers )}";
	}
}
