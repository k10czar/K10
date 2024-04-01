using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
    public class DebugLogOperation : Automation.IOperation
	{
		[SerializeField] string _message = "Log message";
		[SerializeField] ELogType _logType = ELogType.Basic;
		[ExtendedDrawer,SerializeReference] IStringProcessor _processor;

		public IEnumerator ExecutionCoroutine()  
		{
			GetMessage().Log( _logType );
			if( false ) yield return null;
		}

		public string GetMessage() => _processor?.Execute( _message ) ?? _message;

		public string GetSummaryColored() => $"🧻 {"DebugLogOperation".Colorfy( Colors.Console.Verbs )}( {_logType} ): \"{GetMessage()}\"";
	}
}