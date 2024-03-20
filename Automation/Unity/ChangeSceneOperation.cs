using System.Collections;
using UnityEngine;

namespace Automation.Unity
{
	public class ChangeSceneOperation : Automation.IOperation
	{
		[SerializeField, K10Attributes.SceneSelector] string _scene;

		public IEnumerator ExecutionCoroutine() 
		{
			var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( _scene );
			while( !op.isDone ) yield return null;
		}

		public string GetSummaryColored() => $"🎥 {"ChangeSceneOperation".Colorfy( Colors.Console.Verbs )} to {_scene.Colorfy( Colors.Console.Fields )}";
	}
}