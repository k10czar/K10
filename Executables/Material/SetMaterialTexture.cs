using UnityEngine;

public class SetMaterialTexture : SetMaterialProperty
{
    [SerializeField] Texture value;

    protected override void SetProperty( Material m, int propId )
    {
        m.SetTexture( propId, value );
    }
}
