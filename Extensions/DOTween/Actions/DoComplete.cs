using DG.Tweening;
using UnityEngine;

public class DoComplete : ITweenAction<Transform>
{
    public Tweener Do(Transform element, in float duration, in Ease ease)
    {
        element.DOComplete();
        return null;
    }
}
