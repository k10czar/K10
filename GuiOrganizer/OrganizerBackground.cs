using UnityEngine;

[ExecuteInEditMode]
public sealed class OrganizerBackground : MonoBehaviour
{
    [SerializeField] Organizer _organizer;
    [SerializeField] RectTransform _rt;
    [SerializeField] bool _getTotalWidth;
    [SerializeField] bool _getTotalHeight;

    #if UNITY_EDITOR
    void OnValidate(){
        this.RequestAncestry(ref _organizer);
        this.RequestSibling(ref _rt);
    }
    #endif

    void Update(){
        if(_organizer == null) return;
        _rt.sizeDelta = new Vector2( _getTotalWidth ? _organizer.TotalWidth : _rt.sizeDelta.x, _getTotalHeight ? _organizer.TotalHeight : _rt.sizeDelta.y);
    }
}
