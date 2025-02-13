using UnityEngine;

public class RandonStringGuidProvider : IValueProvider<string>
{
    [SerializeField] string _format = "N";
    public string Value => System.Guid.NewGuid().ToString(_format);
}
