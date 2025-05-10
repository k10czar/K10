using UnityEngine;

public class SetMaterialHdrColor : SetMaterialProperty
{
    [SerializeField,ColorUsage(true,true)] Color color = Color.white;
    

    protected override void SetProperty( Material m, int propId )
    {
        m.SetColor( propId, color );
    }
}
