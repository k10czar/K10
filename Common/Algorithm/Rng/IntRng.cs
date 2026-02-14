using UnityEngine;

[System.Serializable]
public struct IntRng
{
    [SerializeField] public IntRange range;
    [SerializeField] float[] _weights;

    public bool IsBiased => _weights != null && _weights.Length > 0;

    public int WeightsCount => _weights?.Length ?? 0;

    public float GetBiasWeight(int rolls)
    {
        if( rolls > range.max ) return 0;
        var id = rolls - range.min;
        if (id < 0) return 0;
        if( !IsBiased ) return 1;
        var len = _weights.Length;
        if (len == 0) return 1;
        if( id >= len ) return _weights[len-1];
        return _weights[id];
    }

    public int Roll() => Roll( K10Random.Value );

    public int Roll( float rngValue01 )
    {
        var delta = range.Delta;
        if( delta == 0 ) return range.max;

        if( !IsBiased )
        { 
            var extrapolatedDelta = delta + 1;
            var scaledRng = rngValue01 * extrapolatedDelta;
            var roundRng = MathAdapter.RoundToInt( scaledRng );
            if( roundRng > delta ) roundRng = delta;
            return range.min + roundRng;
        }

        var sumWeights = 0f;
        for (int i = range.min; i <= range.max; i++ ) sumWeights += GetBiasWeight(i);
        var rng = rngValue01 * sumWeights;
        var rolls = range.min;
        for (; rolls < range.max; rolls++)
        {
            rng -= GetBiasWeight(rolls);
            if (rng < 0 || MathAdapter.Approximately(rng, 0)) break;
        }
        return rolls;
    }
}
