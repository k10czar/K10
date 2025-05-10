using UnityEngine;

public sealed class ToggleActivityWithKey : MonoBehaviour
{
	[SerializeField] KeyCode _key = KeyCode.F1;
	[SerializeField] KeyCode[] _keyCombination;
	[SerializeField] GameObject _object;
	[SerializeField] MonoBehaviour _behaviour;

	void Update()
	{
		if (CheckKeyPress() || CheckKeyCombo())
			ToggleActivity();
	}

	bool CheckKeyPress()
	{
		return Input.GetKeyDown( _key );
	}

	bool CheckKeyCombo()
	{
		if (_keyCombination == null || _keyCombination.Length == 0)
			return false;

		bool hasAtLeastOneKeyDown = false;
		foreach (var key in _keyCombination)
		{
			if (!Input.GetKey(key))
				return false;

			hasAtLeastOneKeyDown |= Input.GetKeyDown(key);
		}

		return hasAtLeastOneKeyDown;
	}

	void ToggleActivity()
	{
		if( _object != null ) _object.SetActive( !_object.activeSelf );
		if( _behaviour != null ) _behaviour.enabled = !_behaviour.enabled;
	}
}