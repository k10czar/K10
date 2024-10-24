using System;
using UnityEngine;

[Serializable]
public class InlineValue<T> : IValueProvider<T>
{
	[SerializeField] T value;
    public T Value => value;
}
