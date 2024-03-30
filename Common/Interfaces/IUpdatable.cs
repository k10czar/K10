using System.Collections;

public interface IUpdatable
{
    void Update( float deltaTime );
}

public interface ILateUpdatable : IUpdatable
{
    void LateUpdate( float deltaTime );
}