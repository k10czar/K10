using UnityEngine;

public class AutoPickerAttribute : PropertyAttribute
{
    public readonly bool searchParent;
    public readonly bool searchChildren;
    public readonly bool addIfNotFound;

    public readonly string getterMethod;

    public AutoPickerAttribute(bool searchParent = true, bool searchChildren = true, bool addIfNotFound = false)
    {
        this.searchParent = searchParent;
        this.searchChildren = searchChildren;
        this.addIfNotFound = addIfNotFound;

        this.getterMethod = null;
    }

    public AutoPickerAttribute(string getterMethod)
    {
        this.getterMethod = getterMethod;
    }
}