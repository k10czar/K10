using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class RangeSummary
{
    public double Min = double.MaxValue;
    public double Average = 0;
    public double Max = double.MinValue;
    double chancesSum = 0;
    bool notSet = true;
    bool isSingleValue = true;

    double combines = 0;
    bool WrongSum => chancesSum > ( combines * 1.05 ) || chancesSum < ( combines * .95 );
    bool CalcIsSingleValue() => MathAdapter.Approximately( (float)Min, (float)Average ) && MathAdapter.Approximately( (float)Average, (float)Max );

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void Clear()
    {
        Min = double.MaxValue;
        Average = 0;
        Max = double.MinValue;
        chancesSum = 0;
        combines = 1;
        notSet = true;
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void StartAccumulator()
    {
        Min = double.MaxValue;
        Average = 0;
        Max = double.MinValue;
        chancesSum = 0;
        combines = 1;
        notSet = true;
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void SetZero()
    {
        Min = 0;
        Average = 0;
        Max = 0;
        chancesSum = 0;
        combines = 0;
        notSet = false;
        isSingleValue = true;
    }

    public void SetOnlyOne(double value) => SetOnlyOne(value, value, value);
    public void SetOnlyOne( double min, double avg, double max )
    {
        Min = min;
        Average = avg;
        Max = max;
        chancesSum = 1;
        combines = 1;
        isSingleValue = true;
        notSet = false;
    }

    public void Normalize()
    {
        if( MathAdapter.Approximately( (float)chancesSum, 0 ) ) return;
        // Debug.Log($"Normalize({Average}) => {Average / chancesSum} {chancesSum} {combines}");
        Average /= chancesSum;
        chancesSum = combines;
        isSingleValue = CalcIsSingleValue();
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void RegisterValue( double value, double chance )
    {
        // var before = this.ToString();
        Min = Math.Min( value, Min );
        Average += value * chance;
        Max = Math.Max( value, Max );
        chancesSum += chance;
        notSet = false;
        isSingleValue = CalcIsSingleValue();
        // Debug.Log( $"{before}.RegisterValue( {value}, {chance*100:N2}% ) = {this}" );
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void RegisterValue( double min, double avg, double max, double chance )
    {
        // var before = this.ToString();
        Min = Math.Min( min, Min );
        Average += avg * chance;
        Max = Math.Max( max, Max );
        chancesSum += chance;
        notSet = false;
        isSingleValue = CalcIsSingleValue();
        // Debug.Log( $"{before}.RegisterValue( {value}, {chance*100:N2}% ) = {this}" );
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void RegisterValue( RangeSummary value, double chance )
    {
        Min = Math.Min( value.Min, Min );
        Average += value.Average * chance;
        Max = Math.Max( value.Max, Max );
        chancesSum += chance;
        notSet = false;
        isSingleValue = CalcIsSingleValue();
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void Combine( RangeSummary range, double times = 1 )
    {
        Min += range.Min * times;
        Average += range.Average * times;
        Max += range.Max * times;
        chancesSum += range.chancesSum * times;
        combines += range.combines * times;
        notSet = false;
        isSingleValue = CalcIsSingleValue();
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void CombinePercentage( RangeSummary range, double percentage )
    {
        Min += range.Min * percentage;
        Average += range.Average * percentage;
        Max += range.Max * percentage;
        chancesSum += range.chancesSum * percentage;
        combines += range.combines * percentage;
        notSet = false;
        isSingleValue = CalcIsSingleValue();
    }

    public override string ToString() => notSet ? "NOT_SET" : ( isSingleValue ? $"{Min:N0}" : $"[ {Min:N0} ... {Average:N1} ... {Max:N0} ]{(WrongSum?$"!WRONG!{chancesSum}!={combines}":"")}" );
    public string ToStringFull() => notSet ? "NOT_SET" : ( isSingleValue ? $"{Min:N0}" : $"[ {Min:N0} ... {Average:N1} ... {Max:N0} ] {chancesSum:N2} {combines:N2} {(WrongSum?"!WRONG!":"")}" );

    public void Combine(double min, double avg, double max, double mod = 1)
    {
        Min += min * mod;
        Average += avg * mod;
        Max += max * mod;
        chancesSum += avg;
        combines += avg;
        notSet = false;
        isSingleValue = CalcIsSingleValue();
    }

    public void Combine((double min, double avg, double max) range, double mod = 1) => Combine( range.min, range.avg, range.max, mod );

    internal (double, double, double) ToTupple() => ( Min, Average, Max );
}
