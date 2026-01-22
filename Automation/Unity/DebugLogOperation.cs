using System.Collections;
using UnityEngine;

namespace K10.Automation.Unity
{
	[ListingPath("Unity/Debug/Log")]
    public class DebugLogOperation : Automation.BaseOperation
	{
		[SerializeField] string _message = "Log message";
		[SerializeField] ELogType _logType = ELogType.Basic;
		[ExtendedDrawer,SerializeReference] IStringProcessor _processor;

		public override IEnumerator ExecutionCoroutine( bool log = false )
		{
			GetMessage().Log( _logType );
#pragma warning disable CS0162 // Unreachable code detected
			if( false ) yield return null;
#pragma warning restore CS0162 // Unreachable code detected
		}

		public string GetMessage() => _processor?.Execute( _message ) ?? _message;

		public override string ToString() => $"🧻 {"DebugLogOperation".Colorfy( Colors.Console.Verbs )}( {_logType} ): \"{GetMessage()}\"";
	}
}