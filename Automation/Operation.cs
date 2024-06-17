using System.Collections;
using IronFeather.IronDebug;
using UnityEngine;

namespace Automation
{
	public interface IOperation
	{
		IEnumerator ExecutionCoroutine( bool log = false );
	}

	public static class OperationExtensions
	{
		public static Coroutine ExecuteOn( this IOperation op, MonoBehaviour behaviour = null, bool log = true )
		{
			if( op == null )
			{
				Log(null, $"{"CANNOT".Colorfy( Colors.Console.Warning )} start {"NULL".Colorfy( Colors.Console.Danger )} operation", log);
				return null;
			}

			op.Log($"{"Started".Colorfy(Colors.Console.Verbs)} {op}", log);
			if( behaviour != null ) return behaviour.StartCoroutine( op.ExecutionCoroutine( log ) );
			return ExternalCoroutine.StartCoroutine( op.ExecutionCoroutine( log ) );
		}

        public static IEnumerator ExecutionCoroutine(this IOperation op, bool log )
        {
	        op.Log($"🤖 Automation {"Executing".Colorfy( Colors.Console.Verbs )} {op}", log);
            return op.ExecutionCoroutine( log );
        }

        public static string GetSummary( this IOperation op ) => op.TypeNameOrNull();

        public static void Log(this IOperation _, string message, bool log = true)
        {
	        if (log) K10Log.Log(GameSystem.Automation, message);
        }
	}
}