using TMPro;
using UnityEngine;

[DefaultExecutionOrder(EXECUTION_ORDER)]
public sealed class CodeTimingTextMeshDebugExhibitor : CodeTimingDebugExhibitor
{
	[SerializeField] TextMeshProUGUI label;

	void Awake() { this.FindDescendent( ref label ); }

	protected override void SetLog( string log ) { label.text = log; }
	protected override void OnEnableChange( bool enabled ) { label.gameObject.SetActive( enabled ); }
}