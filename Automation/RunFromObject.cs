using System.Collections;
using K10.Automation;
using UnityEngine;

public class RunFromObject : BaseOperation, ISummarizable
{
	[SerializeField,InlineProperties] OperationObject _object;

	public override string EmojiIcon => "📦";

	public override IEnumerator ExecutionCoroutine( bool log = false ) 
	{
		if( _object != null ) yield return _object.ExecutionCoroutine( log );
	}

	public override string ToString() => $"{base.ToString()} {_object.ToStringOrNull()}";
	public string Summarize() => _object.TrySummarize( ", " );
}