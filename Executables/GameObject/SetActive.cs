using UnityEngine;

public class SetActive : ITriggerable<GameObject>
{
    [SerializeField] bool _active = true;

    public void Trigger(GameObject go)
    {
        go.SetActive( _active );
    }

    public string Summarize() => $"{(_active?"Activate":"Deactivate")}";
}
