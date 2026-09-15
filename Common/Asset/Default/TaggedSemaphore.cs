using UnityEngine;

public interface ITaggedSemaphore : ISemaphore, ITaggedStateObserver
{

}

[System.Serializable]
public class TaggedSemaphore : Semaphore, ITaggedSemaphore
{
	[SerializeField, StoreGuidFrom(typeof(SemaphoreTag))] string _tag;

    public string Tag => _tag;
}