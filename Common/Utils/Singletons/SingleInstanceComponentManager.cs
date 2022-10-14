


public class SingleInstanceComponentManager<T> where T : UnityEngine.Component
{
	System.Func<bool> _creationCondition;

	public SingleInstanceComponentManager( System.Func<bool> creationCondition )
	{
		_creationCondition = creationCondition;
	}

	public T TryGetInstance()
	{
		if( !_creationCondition.Invoke() ) return null;
		return ForcedInstanceRequest();
	}

	public T TryGetInstanceAndLogErrorIfFail()
	{
		var instance = TryGetInstance();
		if( instance == null ) UnityEngine.Debug.LogError( $"Cannot find instance of {typeof( T )}" );
		return instance;
	}

	public T ForcedInstanceRequest() => Guaranteed<T>.Instance;
}
