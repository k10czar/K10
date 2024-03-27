using System.Collections;
using UnityEngine;

namespace Automation
{
    public class RunFromObject : IOperation
	{
		[SerializeField] OperationObject _object;

		public IEnumerator ExecutionCoroutine() 
		{
			if( _object != null ) yield return _object.ExecutionCoroutine();
		}

		public string GetSummaryColored() => $"📦 {"RunFromObject".Colorfy( Colors.Console.Fields )}";
	}
}