using UnityEngine;

public class AllValidators<T> : IValidator<T>
{
    [SerializeReference,ExtendedDrawer] IValidator<T>[] validatiors;

    public bool Validate(T interactor)
    {
        for (int i = 0; i < validatiors.Length; i++)
        {
            var validator = validatiors[i];
            if( !validator.Validate(interactor) ) return false;
        }
        return true;
    }
}
