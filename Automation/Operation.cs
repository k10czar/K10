using System.Collections;
using K10;
using K10.DebugSystem;
using UnityEngine;

public class AutomationLogCategory : IK10LogCategory
{
    public string Name => "🤖Automation";
    public Color Color => Colors.DodgerBlue;
}

namespace K10.Automation
{
	public interface IOperation : ILoggable<AutomationLogCategory>
	{
		bool CanExecute { get; }
		IEnumerator ExecutionCoroutine( bool log = false );
	}

    public abstract class BaseOperation : IOperation, IEmojiIcon
    {
		[SerializeField] bool _isActive = true;
        public bool CanExecute => _isActive;
        public virtual string EmojiIcon => "🤖";

        public abstract IEnumerator ExecutionCoroutine(bool log = false);

		public override string ToString() => $"{EmojiIcon} {(_isActive?"":"INACTIVE")}{GetType()}";
    }

    public static class OperationExtensions
	{
		static AutomationRunner _runner;

		static AutomationRunner Runner
		{
			get
			{
				if (_runner == null)
				{
					var go = new GameObject( "Automation Runner" );
					go.hideFlags = HideFlags.DontSave;
					Object.DontDestroyOnLoad( go );
					_runner = go.AddComponent<AutomationRunner>();
				}
				return _runner;
			}
		}

		public static IEnumerator TryExecute( this IOperation op, bool log = false )
		{
			if( op != null && op.CanExecute ) return op.ExecutionCoroutine( log );
			return null;
		}

		public static Coroutine ExecuteOn( this IOperation op, MonoBehaviour behaviour = null, bool log = true )
		{
			if( op == null )
			{
				Log(null, $"{"CANNOT".Colorfy( Colors.Console.Warning )} start {ConstsK10.NULL_STRING.Colorfy( Colors.Console.Danger )} operation", log);
				return null;
			}

			op.Log($"{"Started".Colorfy(Colors.Console.Verbs)} {op}", log);
			
			if( op.CanExecute )
			{
				if( behaviour != null ) return behaviour.StartCoroutine( op.TryExecute( log ) );
				return Runner.StartCoroutine( op.TryExecute( log ) );
			}

			return null;
		}

        public static IEnumerator ExecutionCoroutine(this IOperation op, bool log )
        {
	        op.Log($"🤖 Automation {"Executing".Colorfy( Colors.Console.Verbs )} {op}", log);
            return op.TryExecute( log );
        }

        public static string GetSummary( this IOperation op ) => op.TypeNameOrNull();

        public static void Log(this IOperation _, string message, bool log = true)
        {
	        if (log) K10Log<AutomationLogCategory>.Log( message );
        }
	}
}