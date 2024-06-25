using System.Collections;
using UnityEngine;

namespace Automation
{
    public class RunFromObject : IOperation
	{
		[SerializeField,InlineProperties] OperationObject _object;

		public IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			if( _object != null ) yield return _object.ExecutionCoroutine( log );
		}

		public override string ToString() => $"📦 {"RunFromObject".Colorfy( Colors.Console.Fields )}";
	}
}