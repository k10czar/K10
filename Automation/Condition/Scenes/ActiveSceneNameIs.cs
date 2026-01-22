using UnityEngine;
using UnityEngine.SceneManagement;


namespace K10.Conditions
{
    [ListingPath("Scenes/Active Scene Name")]
    public class ActiveSceneNameIs : ICondition
	{
		[SerializeField] string sceneName;

		public bool Check() => SceneManager.GetActiveScene().name == sceneName;
	}
}
