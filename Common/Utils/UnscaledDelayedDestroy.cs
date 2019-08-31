using UnityEngine;
using System.Collections;

public class UnscaledDelayedDestroy : MonoBehaviour 
{
    [SerializeField]float _life;
	float _accTime = 0;

	void Update()
	{
		_accTime += Time.unscaledDeltaTime;

		if( _accTime > _life )
			DD();
	}

	void DD()
	{
		GameObject.Destroy( gameObject );
	}	
	
	public static void Apply( GameObject go, float seconds )
	{
		var dd = go.GetComponent<UnscaledDelayedDestroy>();
		if( dd == null ) dd = go.AddComponent<UnscaledDelayedDestroy>();
		dd._life = seconds + dd._accTime;
	}
}
