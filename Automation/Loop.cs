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
				for( int i = 0; i < _actions.Count; i++ )
				{
					yield return _actions[i].ExecutionCoroutine();
				}
			}
		}
	}
}