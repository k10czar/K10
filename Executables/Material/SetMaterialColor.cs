using UnityEngine;

public class SetMaterialColor : SetMaterialProperty
{
    [SerializeField] Color color = Color.white;

    protected override void SetProperty( Material m, int propId )
    {
        m.SetColor( propId, color );
    }
}
