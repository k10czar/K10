using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
	public class ChangeSceneOperation : Automation.IOperation
	{
		[SerializeField, K10Attributes.SceneSelector] string _scene;

		public IEnumerator ExecutionCoroutine( bool log = false )
		{
			var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( _scene );
			while( !op.isDone ) yield return null;
		}

		public override string ToString() => $"🎥 {"ChangeSceneOperation".Colorfy( Colors.Console.Verbs )} to {_scene.Colorfy( Colors.Console.Fields )}";

		public Object[] LogOwners { get; } = { null };
	}
}