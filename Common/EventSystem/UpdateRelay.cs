using UnityEngine;

public class UpdateRelay : MonoBehaviour
{
	EventSlot<float> _onUpdate = new();

	public IEventRegister<float> OnUpdate => _onUpdate;

	void Update()
	{
		_onUpdate.Trigger( Time.deltaTime );
	}
}
