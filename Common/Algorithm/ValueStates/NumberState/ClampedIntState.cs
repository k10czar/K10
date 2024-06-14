using UnityEngine;

[System.Serializable]
public class ClampedIntState : IntState
{
    [SerializeField] private int minValue = int.MinValue;
    [SerializeField] private int maxValue = int.MaxValue;

    public override void Setter(int newValue)
    {
        newValue = Mathf.Clamp(newValue, minValue, maxValue);
        base.Setter(newValue);
    }

    public void ChangeBounds(int min, int max)
    {
        minValue = min;
        maxValue = max;

        Setter(Value);
    }

    public ClampedIntState(int initialValue = default) : base(initialValue) {}
}