using System;
using UnityEngine;

public class TypeFilterAttribute : PropertyAttribute {
    Type filterType;

    public bool Filter( Type type ) => !type.IsAbstract &&
                                        !type.IsInterface &&
                                        !type.IsGenericType &&
                                        type.InheritsOrImplements(filterType);
        
    public TypeFilterAttribute(Type filterType) {
        this.filterType = filterType;
    }
}