using System;
using System.Collections;
using K10;
using UnityEngine;

public class DelayedActionCapsule : IEventTrigger, IVoidable
{
	private readonly float seconds;
	private readonly ActionCapsule capsule;

	public DelayedActionCapsule(float seconds, Action act)
	{
		this.seconds = seconds;
		capsule = new ActionCapsule(act);
	}

	private bool preVoided;
	public bool IsValid => !preVoided && capsule.IsValid;

	[HideInCallstack]
	public void Trigger()
	{
		if (!IsValid) return;

		preVoided = true;
		ExternalCoroutine.Play(DelayedTrigger());
	}

	private IEnumerator DelayedTrigger()
	{
		yield return new WaitForSeconds(seconds);

		capsule.Trigger();
		capsule.Void();
	}

	public void Void() => capsule.Void();
}