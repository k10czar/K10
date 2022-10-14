using UnityEngine;
using System.Collections.Generic;

public class MonoBehaviourListReferenceHolder : MonoBehaviour
{
	[SerializeField] List<MonoBehaviour> _objects = new List<MonoBehaviour>();

	public void AddRef( MonoBehaviour obj )
	{
		_objects.Add( obj );
	}
}
