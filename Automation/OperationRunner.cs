using System.Collections.Generic;
using K10.DebugSystem;
//using PlasticGui.Help.Conditions;
using UnityEngine;

namespace Automation
{
    public class OperationRunner : KomposedDebugableMonoBehavior
	{
#if UNITY_EDITOR
		const bool IS_EDITOR = true;
#else
		const bool IS_EDITOR = false;
#endif //UNITY_EDITOR
		[SerializeField] bool _log = true;
		[SerializeField] bool _onlyEditor = false;
		[ExtendedDrawer(true), SerializeReference] IMoment _moment;
		[ExtendedDrawer(true), SerializeReference] ICondition _condition;
		[ExtendedDrawer(true), SerializeReference] IOperation _operation;

		public void Awake()
		{
			if( _onlyEditor && !IS_EDITOR ) return;
			var moment = _moment.GetEventOrInstant();
			Log( $"{"Starting".Colorfy( Colors.Console.Verbs )} OperationRunner and registered to run in {moment}", _log);
			moment.Register( TryExecuteOperationNoReturn );
		}

		void TryExecuteOperationNoReturn() => TryExecuteOperation();
		bool TryExecuteOperation()
		{
			var conditionCheck = _condition.SafeCheck();
			Log( $"{"TryExecuteOperation".Colorfy( Colors.Console.Verbs )} {(conditionCheck?"PASS".Colorfy( Colors.Console.Keyword):"FAIL".Colorfy( Colors.Console.Danger))} {_condition.ToStringOrNullColored( Colors.Console.Fields )}", _log);
			if( !conditionCheck ) return false;
			_operation.ExecuteOn( this, _log );
			return true;
		}

		protected override IEnumerable<object> GetKomposedDebugableObjects()
		{
			yield return _operation;
		}

		private static void Log(string message, bool log = true)
		{
			if (log) K10Log<AutomationLogCategory>.Log( message );
		}
	}
}