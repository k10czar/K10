using UnityEngine;
using System.Collections.Generic;

public class UnityObjectListReferenceHolder : MonoBehaviour
{
	[SerializeField] List<Object> _objects = new List<Object>();

	public void AddRef( Object obj )
	{
		_objects.Add( obj );
	}
}
