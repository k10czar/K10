using System.Collections;
using K10.Common;
using UnityEngine;

namespace Automation
{
    public class RunFromObject : IOperation, ISummarizable
	{
		[SerializeField,InlineProperties] OperationObject _object;

		public IEnumerator ExecutionCoroutine( bool log = false )
		{
			if( _object != null ) yield return _object.ExecutionCoroutine( log );
		}

		public override string ToString() => $"📦 {nameof(RunFromObject)} {_object.ToStringOrNull()}";
		public string Summarize() => _object.TrySummarize( ", " );

		public Object[] LogOwners { get; } = { null };
	}
}