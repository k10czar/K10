using System.Collections;
using UnityEngine;

namespace Automation
{
	[CreateAssetMenu( fileName = "OperationObject", menuName = "K10/Automation/OperationObject", order = 1 )]
    public class OperationObject : ScriptableObject, IOperation, ISummarizable
	{
		[ExtendedDrawer,SerializeReference] IOperation _operation;

        public IEnumerator ExecutionCoroutine( bool log = false )
        {
			if( _operation != null )
			{
				_operation.Log($"{"Started".Colorfy( Colors.Console.Verbs )} automation {name.Colorfy( Colors.Console.TypeName )}");
            	yield return _operation.ExecutionCoroutine( log );
			}
        }

		public override string ToString() => $"📦 {nameof(OperationObject)} {name} with {_operation.ToStringOrNull()}";
		public string Summarize() => $"{name}( {_operation.TrySummarize( ", " )} )";
    }
}