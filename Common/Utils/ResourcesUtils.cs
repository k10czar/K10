using UnityEngine;

public static class ResourcesUtils
{
	public static GameObject Instantiate( string resourcePath )
	{
		var res = Resources.Load( resourcePath );
		return (GameObject)GameObject.Instantiate( res );
	}
}
