using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class SizeOverDistance : MonoBehaviour
{
	[SerializeField]float _multiplier = 1;
	[SerializeField]float _base = 1;
	[SerializeField]float _distance = 1;
	[SerializeField]float _currentValue = 1;

    Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    void LateUpdate()
	{
		
		_distance = Vector3.Distance(_mainCamera.transform.position, transform.position );
		_currentValue = _multiplier * Mathf.Log( _distance, _base );
        if( _currentValue < 0 ) _currentValue = 0;
		transform.localScale = Vector3.one * _currentValue;
	}
}
