using UnityEngine;

public class RandomService : IRandomService
{
	[ConstLike] private static IRandomService _fallback;
	public static IRandomService Fallback => _fallback ??= new RandomService();
	public static IRandomService Current => ServiceLocator.Get<IRandomService>() ?? Fallback;

	public float NextFloat() => Random.value;
	public Vector2 NextFloat2Direction()
	{
		var rnd = Random.value;
		var rndRad = rnd * 2 * Mathf.PI;
		return new Vector2(Mathf.Cos(rndRad), Mathf.Sin(rndRad));
	}
}
