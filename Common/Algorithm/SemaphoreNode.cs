public class SemaphoreNode : Semaphore
{
    public bool IsFree => Free;

    public SemaphoreNode(SemaphoreNode parent)
    {
        ReleaseOn(parent);
    }
}