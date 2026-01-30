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

    int combines = 0;
    bool WrongSum => chancesSum > ( combines * 1.05 ) || chancesSum < ( combines * .95 );

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
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void Combine( RangeSummary range, int times = 1 )
    {
        Min += range.Min * times;
        Average += range.Average * times;
        Max += range.Max * times;
        chancesSum += range.chancesSum * times;
        combines += range.combines * times;
        notSet = false;
    }

    public override string ToString() => notSet ? "NOT_SET" : $"[ {Min:N0} ... {Average:N1} ... {Max:N0} ]{(WrongSum?"!WRONG!":"")}";
    public string ToStringFull() => notSet ? "NOT_SET" : $"[ {Min:N0} ... {Average:N1} ... {Max:N0} ] {chancesSum:N2} {combines:N2} {(WrongSum?"!WRONG!":"")}";
}
