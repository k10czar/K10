using System.Collections.Generic;
using PlasticGui.Help.Conditions;
using UnityEngine;

namespace Automation
{
    public class OperationRunner : KomposedDebugableMonoBehavior
	{
		[SerializeField] bool _log = true;
		[ExtendedDrawer(true), SerializeReference] IMoment _moment;
		[ExtendedDrawer(true), SerializeReference] ICondition _condition;
		[ExtendedDrawer(true), SerializeReference] IOperation _operation;

		public void Awake()
		{
			var moment = _moment.GetEventOrInstant();
			if( _log ) Debug.Log( $"{"Starting".Colorfy( Colors.Console.Verbs )} OperationRunner and registered to run in {moment}" );
			moment.Register( TryExecuteOperationNoReturn );
		}
		
		void TryExecuteOperationNoReturn() => TryExecuteOperation();
		bool TryExecuteOperation()
		{
			var conditionCheck = _condition.SafeCheck();
			if( _log ) Debug.Log( $"{"TryExecuteOperation".Colorfy( Colors.Console.Verbs )} {(conditionCheck?"PASS".Colorfy( Colors.Console.Keyword):"FAIL".Colorfy( Colors.Console.Danger))} {_condition.ToStringOrNullColored( Colors.Console.Fields )}" );
			if( !conditionCheck ) return false;
			_operation.ExecuteOn( this, _log );
			return true;
		}
    
		protected override IEnumerable<object> GetKomposedDebugableObjects()
		{
			yield return _operation;
		}
	}
}
