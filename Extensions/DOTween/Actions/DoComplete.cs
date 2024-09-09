using DG.Tweening;
using UnityEngine;

public class DoComplete : ITweenAction<Transform>
{
    public void Do(Transform element, in float duration, in Ease ease)
    {
        element.DOComplete();
    }
}
