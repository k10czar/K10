using System;
using UnityEngine;

public class StoreGuidAttribute : PropertyAttribute { }

public class StoreGuidFromAttribute : PropertyAttribute
{
    public readonly Type TypeRestriction;
    public readonly string TypeRestrictionName;
    public readonly string NewPath = string.Empty;
    public readonly bool AllowSceneObjects = false;

    public StoreGuidFromAttribute(Type typeRestriction, bool allowSceneObjects = false, string newPath = "")
    {
        TypeRestriction = typeRestriction;
        AllowSceneObjects = allowSceneObjects;
        NewPath = newPath;
    }

    public StoreGuidFromAttribute(string typeName, bool allowSceneObjects = false, string newPath = "")
    {
        TypeRestrictionName = typeName;
        AllowSceneObjects = allowSceneObjects;
        NewPath = newPath;
    }

    public Type ResolveType() => TypeRestriction ?? Type.GetType(TypeRestrictionName);
}

public class StoreFileIDAttribute : PropertyAttribute { }
