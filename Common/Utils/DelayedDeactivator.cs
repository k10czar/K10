using UnityEngine;
using System.Collections;

public class DelayedDeactivator : MonoBehaviour 
{
	public void Set( float seconds )
    {
        StartCoroutine( DelayedDeactive( seconds ) );
	}


    IEnumerator DelayedDeactive( float delay )
    {
        yield return new WaitForSeconds( delay );
        gameObject.SetActive( false );
    }
}
