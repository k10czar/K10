using System.Collections;
using UnityEngine;

// using K10.Automation;
namespace K10.Automation
{
[CreateAssetMenu( fileName = "AutomationLoop", menuName = "Automation/Loop", order = 1 )]
	public class Loop : Operation
	{
		[SerializeField] int _repetitions = 1;
		[SerializeField, InlineProperties] System.Collections.Generic.List<Operation> _actions;

		public override IEnumerator ExecutionCoroutine()
		{
			for( int l = 0; l < _repetitions; l++ )
			{
				Debug.Log( $"Loop[{l}] in " + this.ToStringOrNull() );
				for( int i = 0; i < _actions.Count; i++ )
				{
					var act = _actions[i];
					if( act == null ) 
					{
						Debug.LogError( "Cannot play null Operation" );
						continue;
					}
					Debug.Log( "Start operation " + act.ToStringOrNull() );
					yield return act.ExecutionCoroutine();
				}
			}
		}
	}
}