using Automation;
using UnityEngine;

public class RunAutomation : ITriggerable, ISummarizable
{
	[ExtendedDrawer,SerializeReference] IOperation _operation;

	public void Trigger() { _operation.ExecuteOn( ExternalCoroutine.Instance ); }

	public string Summarize() => $"Run {_operation.TrySummarize()}";
}
