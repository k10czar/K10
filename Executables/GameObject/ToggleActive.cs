using UnityEngine;

public class ToggleActive : ITriggerable<GameObject>
{
    public void Trigger(GameObject go)
    {
        go.SetActive( !go.activeSelf );
    }
    
    public string Summarize() => $"Toogle Active";
}
