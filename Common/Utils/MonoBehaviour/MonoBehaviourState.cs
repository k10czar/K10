using UnityEngine;


public class MonoBehaviourState : MonoBehaviour
{
	BoolState _isActive = new BoolState();
	BoolState _isAlive = new BoolState( true );

	public IBoolStateObserver IsActive => _isActive;
	public IBoolStateObserver IsAlive => _isAlive;

	void OnEnable() => _isActive.SetTrue();
	void OnDisable() => _isActive.SetFalse();
	void OnDestroy() => _isAlive.SetFalse();
}