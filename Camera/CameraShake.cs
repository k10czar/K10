using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CameraShakeInfo
{
	[SerializeField] float _shakeTime = .5f;
	[SerializeField] float _shakeAmplitude = 1;
	[SerializeField] float _shakeFrequency = 24;
	[SerializeField] float _decayPower = 2;

	public float Time { get { return _shakeTime; } }
	public float Amplitude { get { return _shakeAmplitude; } }
	public float Frequency { get { return _shakeFrequency; } }
	public float DecayPower { get { return _decayPower; } }

	public static readonly CameraShakeInfo Empty = new CameraShakeInfo(0, 0, 0);

	public CameraShakeInfo() { }

	public CameraShakeInfo( float amplitude, float frequency, float time )
		: this( amplitude, frequency, time, 1 )
	{ }

	public CameraShakeInfo( float amplitude, float frequency, float time, float decayPower )
	{
		_shakeTime = time;
		_shakeAmplitude = amplitude;
		_shakeFrequency = frequency;
		_decayPower = decayPower;
	}
}

public class CameraShake : MonoBehaviour
{
	public const float AmplitudeMultiplier = 1.0f;
	[SerializeField] CameraShakeInfo _test;

	class CameraShakeInstance
	{
		CameraShakeInfo _shake;
		float _startTime = Time.unscaledTime;

		public CameraShakeInstance( CameraShakeInfo _shake )
		{
			this._shake = _shake;
		}

		public bool IsValid { get { return _startTime + _shake.Time > Time.unscaledTime; } }

		float AccTime { get { return Time.unscaledTime - _startTime; } }
		float Percentage { get { return Mathf.Pow( ( _shake.Time - AccTime ) / _shake.Time, _shake.DecayPower ) * Mathf.Clamp01( AccTime * 20 / _shake.Time ); } }
		public float CurrentAmplitude => _shake.Amplitude * Percentage * AmplitudeMultiplier;
		public float CurrentFrequency { get { return _shake.Frequency * Percentage; } }
	}

	float _shakeTime;
	float _shakeAmplitude;
	float _shakeFrequency;
	float _accTime;
	float _decayPower;
	float _randomStart;

	float _current;

	List<CameraShakeInstance> _shakes = new List<CameraShakeInstance>();

    void Awake()
    {
        Singleton<CameraShake>.SayHello( this );
    }

	void AddCameraShake( CameraShakeInfo info )
	{
		if( info == null ) return;
		if( _shakes.Count == 0 ) _randomStart = Random.Range( 0f, 10f );
		_shakes.Add( new CameraShakeInstance( info ) );
	}

	void Test()
	{
		AddCameraShake( _test );
	}

	public static void Add( CameraShakeInfo info )
	{
		//NetworkedCameraShake.Instance.AddCameraShake( info );
		AddLocal( info );
	}

	public static void AddLocal( CameraShakeInfo info )
	{
		AddLocal( info, Camera.main );
	}

	public static void AddLocal( CameraShakeInfo info, Camera cam )
	{
		var camShake = Singleton<CameraShake>.Instance;
        if (camShake == null) return;

        camShake.AddCameraShake( info );
	}

	void Update()
	{
		for( int i = _shakes.Count - 1; i >= 0; i-- )
			if( !_shakes[i].IsValid )
				_shakes.RemoveAt( i );

		if( _shakes.Count > 0 )
		{
			float amplitude = 0;
			float frequency = 0;
			for( int i = 0; i < _shakes.Count; i++ )
			{
				amplitude = Mathf.Max( amplitude, _shakes[i].CurrentAmplitude );
				frequency = Mathf.Max( frequency, _shakes[i].CurrentFrequency );
			}

			_current += frequency * Time.deltaTime;

			var realAmplitude = Mathf.PerlinNoise( _randomStart + _current, 0 ) * amplitude;
			float angle = Mathf.PerlinNoise( 0, _randomStart + _current ) * Mathf.PI * 2;
			transform.localPosition = ( Vector3.right * Mathf.Cos( angle ) + Vector3.up * Mathf.Sin( angle ) ) * realAmplitude;
		}
		else
		{
			transform.localPosition = Vector3.zero;
		}
	}
}
