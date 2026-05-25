
namespace K10.Conditions
{
	[ListingPath(nameof(Ever))]
	public class Ever : ICondition
	{
		public bool Check() => true;
	}
}
