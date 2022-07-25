using System.Collections;
using UnityEngine;

namespace K10.Automation
{
	public abstract class Operation : ScriptableObject
	{
		public abstract IEnumerator ExecutionCoroutine();
		public override string ToString() => $"{GetType()} => {name}";
	}
}

public static class OperationExtensions
{
	public static Coroutine Execute( this K10.Automation.Operation op )
	{
		Debug.Log( $"Start automation {op.NameOrNull()}" );
		if( op == null ) return null;
		var coroutine = ExternalCoroutine.StartCoroutine( op.ExecutionCoroutine() );
		return coroutine;
	}
}
