using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScalePorting : MonoBehaviour
{
	[SerializeField] bool _forcePcSize;
	[SerializeField] float _forcedPcSize = .6666666f;
	[SerializeField] bool _forceMobileSize;

	#if UNITY_ANDROID || UNITY_IOS || UNITY_WP_8 || UNITY_WP_8_1
	[SerializeField] float _forcedMobileSize = 1;
	#endif

	#if UNITY_STANDALONE
	const float SIZE = .6666666f;
	#elif UNITY_ANDROID || UNITY_IOS || UNITY_WP_8 || UNITY_WP_8_1
	const float SIZE = 1;
	#else
	const float SIZE = 1;
	#endif

	void Start() { UpdateScale(); }

	#if UNITY_EDITOR
	void Update() { UpdateScale(); }
	#endif

	float Size
	{
		get
		{
			#if UNITY_STANDALONE
//			if( _forceMobileSize ) return _forcedMobileSize;
			return _forcePcSize ? _forcedPcSize : SIZE;
			#elif UNITY_ANDROID || UNITY_IOS || UNITY_WP_8 || UNITY_WP_8_1
//			if( _forcePcSize ) return _forcedPcSize;
			return _forceMobileSize ? _forcedMobileSize : SIZE;
			#else
//			if( _forcePcSize ) return _forcedPcSize;
			return _forceMobileSize ? _forcedMobileSize : SIZE;
			#endif
		}
	}

	void UpdateScale()
	{
		var scale = transform.localScale;
		var desired = Vector3.one * Size;
		if( Mathf.Approximately( Vector3.Distance( scale, desired ), 0 ) ) return;
		transform.localScale = desired;
		K10.Utils.Unity.Algorithm.UpdateChildrenOrganizers( gameObject );
	}
}