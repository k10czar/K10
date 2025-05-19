using UnityEngine;

public interface ITaggedState : IStateRequester, ITaggedStateObserver
{
    
}

[System.Serializable]
public class TaggedState : StateRequester, ITaggedState
{
	[SerializeField, StoreGuidFrom(typeof(StateTag))] string _tag;

    public string Tag => _tag;
}
