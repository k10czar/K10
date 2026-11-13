using UnityEngine;

public class BitMaskAttribute : PropertyAttribute {}


public class BitMaskUnmaskedAttribute : PropertyAttribute
{
	public System.Type propType;
	public BitMaskUnmaskedAttribute( System.Type aType ) { propType = aType; }
}