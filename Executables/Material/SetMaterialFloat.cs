using UnityEngine;

public class SetMaterialFloat : SetMaterialProperty
{
    [SerializeField] float value;

    protected override void SetProperty( Material m, int propId )
    {
        m.SetFloat( propId, value );
    }
}
