using UnityEngine;

public class SetMaterialInteger : SetMaterialProperty
{
    [SerializeField] int value;

    protected override void SetProperty( Material m, int propId )
    {
        m.SetInteger( propId, value );
    }
}
