using UnityEngine;

[System.Serializable]
public class ClampedFloatState : FloatState
{
    [field: SerializeField] public float MinValue { get; private set; }
    [field: SerializeField] public float MaxValue { get; private set; }

    public float Percent => MaxValue - MinValue != 0 ? (Value - MinValue) / (MaxValue - MinValue) : 0;

    public override void Setter(float newValue)
    {
        newValue = Mathf.Clamp(newValue, MinValue, MaxValue);
        base.Setter(newValue);
    }

    public void ChangeBounds(float min, float max)
    {
        MinValue = min;
        MaxValue = max;

        Setter(Value);
    }

    public ClampedFloatState(int initialValue = 0) : base(initialValue) {}
}