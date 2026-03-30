using UnityEngine;

[System.Serializable]
public struct FloatRange
{
    [SerializeField] public float min;
    [SerializeField] public float max;

    public FloatRange(float min, float max) : this()
    {
        this.min = min;
        this.max = max;
    }

    public float Delta => max - min;
}
