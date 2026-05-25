using System;
using UnityEngine;

public class EnumIntAttribute : PropertyAttribute
{
    public readonly Type EnumType;
    public EnumIntAttribute(Type enumType) { EnumType = enumType; }
}
