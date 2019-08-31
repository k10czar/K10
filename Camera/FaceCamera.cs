using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FaceCamera : MonoBehaviour
{
	public bool _cameraRotation;
	static Camera _camera;

	void LateUpdate()
	{
		if( _camera == null ) _camera = Camera.main;
		var rotation = _cameraRotation ? _camera.transform.rotation * Vector3.up : Vector3.up;
		transform.rotation = Quaternion.LookRotation( _camera.transform.position - transform.position, Vector3.up );
	}
}