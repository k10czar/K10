using UnityEngine;

public class OnlyPropertyAttribute : PropertyAttribute
{
    public readonly string Path = string.Empty;

    public OnlyPropertyAttribute( string path )
    {
        Path = path;
    }
}
