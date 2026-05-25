using K10.Automation;
using UnityEngine;

public class RunAutomation : ITriggerable, ISummarizable
{
	[SerializeField] bool _debugLogExecution;
	[ExtendedDrawer,SerializeReference] IOperation _operation;

	public void Trigger() { _operation.ExecuteOn( ExternalCoroutine.Instance, _debugLogExecution ); }

	public string Summarize() => $"Run {_operation.TrySummarize()}";
}
