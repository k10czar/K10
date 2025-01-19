using System.Collections.Generic;
using UnityEngine;

public class SetMaterialColors : SetMaterialProperty
{
    [SerializeField] List<Color> colors;

    protected override void SetProperty( Material m, int propId )
    {
        m.SetColorArray( propId, colors );
    }
}
