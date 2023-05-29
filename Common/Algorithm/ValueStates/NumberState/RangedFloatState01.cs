using UnityEngine;
using static UnityEngine.Mathf;

[System.Serializable]
public class RangedFloatState01 : INumericValueState<float>, IValueStateObserver<float>, ISerializationCallbackReceiver, ICustomDisposableKill
{
	[SerializeField] protected FloatState _value;
	[System.NonSerialized] private bool _inited;

	public float Value => _value.Value;
	public IEventRegister<float> OnChange => _value.OnChange;

	public float Get() => _value.Get();

	public void Setter( float t )
	{
		if( t < 0 ) t = 0;
		else if( t > 1 ) t = 1;
		_value.Setter( t );
	}

	public void Increment( float increment = 1 )
	{
		if( increment < float.Epsilon && increment > FloatHelper.NegativeEpsilon ) return;
		Setter( _value.Value + increment );
	}

	public RangedFloatState01( float initialValue )
	{
		if( initialValue < 0 ) initialValue = 0;
		else if( initialValue > 1 ) initialValue = 1;
		_value = new FloatState( initialValue );

		Init();
	}

	public void Kill()
	{
		_value?.Kill();
	}

	protected void Init()
	{
		if( _inited ) return;
		_inited = true;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize() { Init(); }
}
