using System.Collections;
using UnityEngine;

namespace Unity.Automation
{
	public class InstantiateOperation : K10.Automation.Operation
	{
		[SerializeField] int _instances = 1;
		[SerializeField] GameObject _prefab;
		[SerializeField] Vector3 _position;
		[SerializeField] Vector3 _rotation;
		[SerializeField] bool _dontDestroyOnLoad;
		[SerializeField] int _elementsPerFrame = 0;

		public override IEnumerator ExecutionCoroutine()
		{
			if( _prefab != null )
			{
				int instancesPreFrame = 0;
				for( int i = 0; i < _instances; i++ )
				{
					var go = GameObject.Instantiate( _prefab, _position, Quaternion.Euler( _rotation ) );
					if( _dontDestroyOnLoad ) GameObject.DontDestroyOnLoad( go );
					instancesPreFrame++;
					if( instancesPreFrame > _elementsPerFrame && _elementsPerFrame > 0 )
					{
						yield return null;
						instancesPreFrame = 0;
					}
				}
			}
			else
			{
				Debug.LogError( "Cannot instantiate null prefab" );
			}
		}
	}
}