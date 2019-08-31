using UnityEngine;
using System.Collections;

public interface IRandomFloatRange
{
    float Random { get; }
}

[System.Serializable]
public class RandomFloatRange : IRandomFloatRange
{
    [SerializeField]
    float _min = 0;
    [SerializeField]
    float _max = 1;

    public float Random { get { return UnityEngine.Random.Range(_min, _max); } }

    public RandomFloatRange(float min, float max) { _min = min; _max = max; }
    public RandomFloatRange() : this(0, 1) { }
}

[System.Serializable]
public class RandomIntRange
{
    [SerializeField]
    int _min = 0;
    [SerializeField]
    int _max = 1;

    public int Random { get { return UnityEngine.Random.Range(_min, _max); } }

    public RandomIntRange(int min, int max) { _min = min; _max = max; }
    public RandomIntRange() : this(0, 1) { }
}
