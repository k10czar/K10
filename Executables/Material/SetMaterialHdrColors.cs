using System.Collections.Generic;
using UnityEngine;

public class SetMaterialHdrColors : SetMaterialProperty
{
    [SerializeField,ColorUsage(true,true)] List<Color> colors;

    protected override void SetProperty( Material m, int propId )
    {
        m.SetColorArray( propId, colors );
    }
}
