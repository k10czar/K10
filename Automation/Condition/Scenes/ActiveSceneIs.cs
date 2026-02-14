using K10Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace K10.Conditions
{
	[ListingPath("Scenes/Active Scene")]
    public class ActiveSceneIs : ICondition
	{
		[SerializeField,SceneSelector] string scene;

		public bool Check() => SceneManager.GetActiveScene().name == scene;
	}
}
