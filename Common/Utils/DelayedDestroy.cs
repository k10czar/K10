using UnityEngine;
using System.Collections;

public class DelayedDestroy : MonoBehaviour 
{
    public float _life;

    void Start()
    {
        foreach( var ps in GetComponentsInChildren<ParticleSystem>() ) 
        {
            if( _life < ps.main.duration )
                _life = ps.main.duration;
        }

        Invoke( nameof(DD), _life );
    }

	void DD()
	{
		GameObject.Destroy( gameObject );
	}
}
