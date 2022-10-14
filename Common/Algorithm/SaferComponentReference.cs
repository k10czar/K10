public class SaferComponentReference<T> : CachedReference<T> where T : UnityEngine.Component
{
	public SaferComponentReference( T startData = default( T ) ) : base( startData )
	{
		OnReferenceSet.Register( OnReferenceChange );
		OnReferenceChange( startData, Validator );
	}

	private void OnReferenceChange( T t, IEventValidator validator )
	{
		if( t == null ) return;
		t.gameObject.EventRelay().OnDestroy.Register( validator.Validated( OnReferenceDie ) );
	}

	void OnReferenceDie()
	{
		ChangeReference( null );
	}
}
