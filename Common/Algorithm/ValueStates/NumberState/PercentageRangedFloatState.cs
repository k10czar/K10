using UnityEngine;
using static UnityEngine.Mathf;

public interface IPercentageRangedFloatState : IRangedFloatState
{
	IValueStateObserver<float> Percentage { get; }
}

public interface IPercentageRangedFloatStateObserver : IRangedFloatStateObserver
{
	IValueStateObserver<float> Percentage { get; }
}

[System.Serializable]
public class PercentageRangedFloatState : RangedFloatState, IPercentageRangedFloatState, IPercentageRangedFloatStateObserver, ISerializationCallbackReceiver
{
	public const string PERCENTAGE_PROPERTY_NAME = nameof(Percentage);

	[System.NonSerialized] private FloatState _percentage = new FloatState( 0 );
	[System.NonSerialized] private bool _percentageInited;

	public IValueStateObserver<float> Percentage => _percentage;

	public PercentageRangedFloatState( float percentage, float maxValue ) : this( percentage, 0, maxValue ) { }
	public PercentageRangedFloatState( float percentage, float minValue, float maxValue ) : base( percentage * ( maxValue - minValue ) + minValue, minValue, maxValue )
	{
		InitPercentage();
	}

	void InitPercentage()
	{
		if( _percentageInited ) return;
		_percentageInited = true;
		if( _percentage == null ) _percentage = new FloatState( 0 );
		_min.OnChange.Register( UpdatePercentage );
		_max.OnChange.Register( UpdatePercentage );
		_value.OnChange.Register( UpdatePercentage );
		UpdatePercentage();
	}

	void UpdatePercentage()
	{
		var min = _min.Value;
		var max = _max.Value;
		var delta = max - min;

		if( Approximately( delta, 0 ) )
		{
			_percentage.Value = 0;
			return;
		}

		_percentage.Value = ( _value.Value - min ) / delta;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize() { Init(); InitPercentage(); }
}
