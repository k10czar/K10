using UnityEngine;

public class SetMaterialVector : SetMaterialProperty
{
    [SerializeField] Vector4 vector;

    protected override void SetProperty( Material m, int propId )
    {
        m.SetVector( propId, vector );
    }
}
