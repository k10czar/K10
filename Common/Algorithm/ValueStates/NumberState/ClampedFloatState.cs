using UnityEngine;

[System.Serializable]
public class ClampedFloatState : FloatState
{
    [SerializeField] private float minValue = float.MinValue;
    [SerializeField] private float maxValue = float.MaxValue;

    public override void Setter(float newValue)
    {
        newValue = Mathf.Clamp(newValue, minValue, maxValue);
        base.Setter(newValue);
    }

    public void ChangeBounds(float min, float max)
    {
        minValue = min;
        maxValue = max;

        Setter(Value);
    }

    public ClampedFloatState(int initialValue = default) : base(initialValue) {}
}