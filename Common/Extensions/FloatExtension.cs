public static class FloatExtension
{
    public static int CompareToAbs( this float value, float otherValue )
    {
        if( otherValue < 0 ) otherValue = -otherValue;
        if( value < 0 ) value = -value;
        return value.CompareTo( otherValue );
    }
	
	public static string ToPercentageString( this float percentage )
	{
		if( percentage > .05 ) return $" {100*percentage:N0}%";
		else if( percentage > .005 ) return $" {100*percentage:N1}%";
		else if( percentage > .0005 ) return $" {100*percentage:N2}%";
		else if( percentage > .00005 ) return $" {100*percentage:N3}%";
		else if( percentage > .000005 ) return $" {100*percentage:N4}%";
		else if( percentage > .0000005 ) return $" {100*percentage:N5}%";
		else if( percentage > .00000005 ) return $" {100*percentage:N6}%";
		else return $" {100*percentage:N7}%";
	}
}
