using UnityEngine;

[System.Serializable]
public struct IntRange
{
    [SerializeField] public int min;
    [SerializeField] public int max;

    public int Delta => max - min;
}
