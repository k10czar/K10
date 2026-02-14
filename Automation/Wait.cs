using System.Collections;
using UnityEngine;


namespace K10.Automation
{
    [ListingPath("Wait/Condition")]
	public class Wait : BaseOperation
	{
		[ExtendedDrawer,SerializeReference] ICondition _condition;

		public override string EmojiIcon => "🤚";

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{ 
			if( log ) Debug.Log( $"Wait until {_condition.ToStringOrNull()}" );
			while( !_condition.Check() )
			{
				yield return null;
			} 
			if( log ) Debug.Log( $"Wait is over, {_condition.ToStringOrNull()} is true" );
		}

		public override string ToString() => $"{base.ToString()} {_condition.ToStringOrNull()}";
	}
}