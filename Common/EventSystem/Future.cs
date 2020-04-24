

public interface IFutureBase 
{ 
	bool IsComplete { get; } 
}

public interface IFutureObserver : IFutureBase
{
	IEventRegister OnCompletion { get; }
}

public interface IFutureObserver<Result> : IFutureBase
{
	IEventRegister<Result> OnCompletion { get; }
}

public interface IFuture : IFutureObserver
{
	void ForceComplete();
}

public interface IFuture<Result> : IFutureObserver<Result>
{
	void ForceComplete( Result t );
}

public class Future : IFuture
{
	bool _isComplete = false;

	EventSlot _completion = new EventSlot();

	public IEventRegister OnCompletion { get { return _completion; } }

	public void ForceComplete()
	{
		if( _isComplete )
			return;

		_isComplete = true;
		_completion.Trigger();
	}

	public bool IsComplete { get { return _isComplete; } }
}

public class Future<Result> : IFuture<Result>
{
	bool _isComplete = false;

	EventSlot<Result> _completion = new EventSlot<Result>();

	public IEventRegister<Result> OnCompletion { get { return _completion; } }

	public void ForceComplete( Result result )
	{
		if( _isComplete ) return;

		_isComplete = true;
		_completion.Trigger( result );
	}

	public bool IsComplete { get { return _isComplete; } }
}