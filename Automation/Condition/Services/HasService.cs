using UnityEngine;


namespace K10.Conditions
{
	[ListingPath("Services/HasService")]
	public class HasService : ICondition
	{
		[TypeFilter(typeof(IService))]
		[SerializeField] SerializableType serviceType;

		public bool Check() => ServiceLocator.Contains( serviceType );
	}
}
