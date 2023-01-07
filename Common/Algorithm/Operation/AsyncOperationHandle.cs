


public interface IAsyncOperationObserver
{
    IBoolStateObserver IsStarted { get; }
    IBoolStateObserver IsDone { get; }
}

public static class AsyncOperationObserverExtension
{
    public static void ExecuteWhenOperationEnd( this IAsyncOperationObserver exec, System.Action act )
    {
        if( exec == null || exec.IsDone.Value ) act();
        else exec.IsDone.OnTrueState.Register( new CallOnce( act ) );
    }
}

public class AsyncOperationHandle : IAsyncOperationObserver
{
    BoolState _isStarted = new BoolState();
    BoolState _isDone = new BoolState();

    public IBoolStateObserver IsStarted => _isStarted;
    public IBoolStateObserver IsDone => _isDone;

    public AsyncOperationHandle Start()
    {
        _isStarted.SetTrue();
        return this;
    }

    public AsyncOperationHandle Finish()
    {
        _isDone.SetFalse();
        return this;
    }
}

public class AlreadyDoneOperationHandle : IAsyncOperationObserver
{
    public IBoolStateObserver IsStarted => TrueState.Instance;
    public IBoolStateObserver IsDone => TrueState.Instance;

    private AlreadyDoneOperationHandle() {}

    public static AlreadyDoneOperationHandle _instance;
    public static AlreadyDoneOperationHandle Instance => _instance ?? ( _instance = new AlreadyDoneOperationHandle() );
}