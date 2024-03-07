using System.Collections;
using UnityEngine;

namespace Automation
{
	public interface IOperation
	{
		IEnumerator ExecutionCoroutine();
		string GetSummaryColored();
	}

	public static class OperationExtensions
	{
		public static Coroutine ExecuteOn( this IOperation op, MonoBehaviour behaviour = null, bool log = true )
		{
			if( op == null ) 
			{
				if( log ) Debug.Log( $"{"CANNOT".Colorfy( Colors.Console.Warning )} start {"NULL".Colorfy( Colors.Console.Danger )} operation" );
				return null;
			}
			if( log ) Debug.Log( $"{"Started".Colorfy( Colors.Console.Verbs )} {op.GetSummaryColored()}" );
			if( behaviour != null ) return behaviour.StartCoroutine( op.ExecutionCoroutine( log ) );
			return ExternalCoroutine.StartCoroutine( op.ExecutionCoroutine( log ) );
		}

        public static IEnumerator ExecutionCoroutine(this IOperation op, bool log )
        {
			if( log ) Debug.Log( $"🤖 Automation{"Executing".Colorfy( Colors.Console.Verbs )} {op.GetSummaryColored()}" );
            return op.ExecutionCoroutine();
        }

        public static string GetSummary( this IOperation op ) => op.TypeNameOrNull();
	}
}
