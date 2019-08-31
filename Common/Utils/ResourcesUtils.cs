using UnityEngine;
using System.Collections;

public static class ResourcesUtils
{
	public static GameObject Instantiate( string resourcePath )
	{
		var res = Resources.Load( resourcePath );
		return (GameObject)GameObject.Instantiate( res );
	}
}
