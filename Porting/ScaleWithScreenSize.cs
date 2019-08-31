using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleWithScreenSize : MonoBehaviour
{
	[SerializeField] Vector2 _scaleBase = Vector2.one;
	[SerializeField] Vector2 _margin = Vector2.one * .1f;
	[SerializeField] bool _onUpdate;
	[SerializeField] bool _onStart = true;


	void Start() { if( _onStart ) UpdateScale(); }
		
	void Update()
	{
		if( !_onUpdate )
			return;
		
		UpdateScale();
	}

	void UpdateScale()
	{
		float h = Screen.height;
		float w = Screen.width;

		transform.localScale = ( Vector3.right * ( ( ( w / h ) * _scaleBase.x ) + _margin.x ) ) + ( Vector3.up * ( _scaleBase.y + _margin.y ) ) + Vector3.forward;
	}
}
