public class SemaphoreNode : Semaphore
{
    public bool IsFree => Free;

    public SemaphoreNode(SemaphoreNode parent)
    {
        if (parent != null) ReleaseOn(parent);
    }
}