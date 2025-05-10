
namespace K10.Conditions
{
	public class Never : ICondition
	{
		public bool Check() => false;
	}
}
