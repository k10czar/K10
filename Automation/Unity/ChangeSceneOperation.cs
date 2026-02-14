using System.Collections;
using UnityEngine;

namespace K10.Automation.Unity
{
	[ListingPath("Unity/Scenes/Change")]
	public class ChangeSceneOperation : Automation.BaseOperation
	{
		[SerializeField, K10Attributes.SceneSelector] string _scene;

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( _scene );
			while( !op.isDone ) yield return null;
		}

		public override string ToString() => $"🎥 {"ChangeSceneOperation".Colorfy( Colors.Console.Verbs )} to {_scene.Colorfy( Colors.Console.Fields )}";
	}
}