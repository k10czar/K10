

//I dont want to mix with IDisposable features, because I cannot predict the results so I did this interface separated
public interface ICustomDisposableKill
{
	void Kill();
}