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
public class PercentageRangedFloatState : RangedFloatState, IPercentageRangedFloatState, IPercentageRangedFloatStateObserver
{
	[SerializeField] private readonly FloatState _percentage = new FloatState( 0 );

	public IValueStateObserver<float> Percentage => _percentage;

	public PercentageRangedFloatState( float percentage, float maxValue ) : this( percentage, 0, maxValue ) { }
	public PercentageRangedFloatState( float percentage, float minValue, float maxValue ) : base( percentage * ( maxValue - minValue ) + minValue, minValue, maxValue )
	{
		RegisterPercentageEvents();
		UpdatePercentage();
	}

	void RegisterPercentageEvents()
	{
		_min.OnChange.Register( UpdatePercentage );
		_max.OnChange.Register( UpdatePercentage );
		_value.OnChange.Register( UpdatePercentage );
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
}
