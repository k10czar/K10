public class SaferGameObjectReference : CachedReference<UnityEngine.GameObject>
{
	public SaferGameObjectReference( UnityEngine.GameObject startData = null ) : base( startData )
	{
		OnReferenceSet.Register( OnReferenceChange );
		OnReferenceChange( startData, Validator );
	}

	private void OnReferenceChange( UnityEngine.GameObject t, IEventValidator validator )
	{
		if( t == null ) return;
		t.EventRelay().OnDestroy.Register( validator.Validated( OnReferenceDie ) );
	}

	void OnReferenceDie()
	{
		ChangeReference( null );
	}
}
