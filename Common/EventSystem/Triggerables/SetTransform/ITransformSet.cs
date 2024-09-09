using UnityEngine;

public interface ITransformSet
{
    void Set( Transform transform );
}

public static class TransformSetExtensions
{
    public static void Set( this System.Collections.Generic.IEnumerable<ITransformSet> tweens, Transform transform )
    {
        if( tweens == null ) return;
        foreach( var tween in tweens )
        {
            tween.Set( transform );
        }
    }
}