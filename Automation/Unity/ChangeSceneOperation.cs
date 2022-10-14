using System.Collections;
using UnityEngine;

namespace Unity.Automation
{
	public class ChangeSceneOperation : K10.Automation.Operation
	{
		[SerializeField, K10Attributes.SceneSelector] string _scene;

		public override IEnumerator ExecutionCoroutine()
		{
			var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( _scene );
			while( !op.isDone ) yield return null;
		}
	}
}