using System.Collections;
using UnityEngine;

namespace K10.Automation
{
	public abstract class Operation : ScriptableObject
	{
		public abstract IEnumerator ExecutionCoroutine();
	}
}

public static class OperationExtensions
{
	public static Coroutine Execute( this K10.Automation.Operation op )
	{
		if( op == null ) return null;
		var coroutine = ExternalCoroutine.StartCoroutine( op.ExecutionCoroutine() );
		return coroutine;
	}
}
