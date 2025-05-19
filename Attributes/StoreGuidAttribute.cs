using System;
using UnityEngine;

public class StoreGuidAttribute : PropertyAttribute { }

public class StoreGuidFromAttribute : PropertyAttribute
{
    public readonly Type TypeRestriction;
    public readonly string NewPath = string.Empty;
    public readonly bool AllowSceneObjects = false;

    public StoreGuidFromAttribute(Type typeRestriction, bool allowSceneObjects = false, string newPath = "")
    {
        TypeRestriction = typeRestriction;
        AllowSceneObjects = allowSceneObjects;
        NewPath = newPath;
    }
}

public class StoreFileIDAttribute : PropertyAttribute { }
