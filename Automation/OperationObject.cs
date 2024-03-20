using System.Collections;
using UnityEngine;

namespace Automation
{
	[CreateAssetMenu( fileName = "OperationObject", menuName = "Automation/OperationObject", order = 1 )]
    public class OperationObject : ScriptableObject, IOperation
	{
		[ExtendedDrawer,SerializeReference] IOperation _operation;

        public IEnumerator ExecutionCoroutine() 
        {
			if( _operation != null )
			{
				Debug.Log( $"{"Started".Colorfy( Colors.Console.Verbs )} automation {name.Colorfy( Colors.Console.TypeName )}" );
            	yield return _operation.ExecutionCoroutine();
			}
        }

		public string GetSummaryColored() => $"📦 {"OperationObject".Colorfy( Colors.Console.Fields )} {name} with {_operation.ToStringColored(Colors.Console.Numbers)}";
    }
}
