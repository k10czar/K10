using UnityEngine;

public abstract class SetMaterialProperty : ITriggerable<Material>
{
    [SerializeReference,ExtendedDrawer] IShaderProperty propertyFinder;

    public void Trigger(Material m)
    {
        if( propertyFinder == null ) return;
        SetProperty( m, propertyFinder.PropertyID );
    }

    protected abstract void SetProperty( Material m, int propId );
}
