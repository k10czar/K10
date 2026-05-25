
namespace K10.Conditions
{
	[ListingPath(nameof(Never))]
	public class Never : ICondition
	{
		public bool Check() => false;
	}
}
