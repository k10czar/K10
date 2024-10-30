using UnityEngine;

public interface IShaderProperty
{
    int PropertyID { get; }
}

public class ShaderPropertyByName : IShaderProperty
{
    [SerializeReference,ExtendedDrawer] IValueProvider<string> propertyNameProvider;

    int propertyID = -1;
    bool searchedProp = false;

    public int PropertyID
    {
        get
        {
            if( !searchedProp )
            {
                searchedProp = true;
                if( propertyNameProvider != null ) propertyID = Shader.PropertyToID( propertyNameProvider.Value );
            }
            return propertyID;
        }
    }
}
