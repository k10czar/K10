using DG.Tweening;
using UnityEngine;

public class DoLocalMoveX : ITweenAction<Transform>
{
    [SerializeField] float value;

    public Tweener Do(Transform element, in float duration, in Ease ease)
    {
        return element.DOLocalMoveX( value, duration ).SetEase( ease );
    }
}