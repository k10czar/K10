using UnityEngine;

public sealed class ToggleActivityWithKey : MonoBehaviour
{
	[SerializeField] KeyCode _key = KeyCode.F1;
	[SerializeField] GameObject _object;
	[SerializeField] MonoBehaviour _behaviour;

	void Update()
	{
		if( !Input.GetKeyDown( _key ) ) return;
		if( _object != null ) _object.SetActive( !_object.activeSelf );
		if( _behaviour != null ) _behaviour.enabled = !_behaviour.enabled;
	}
}
