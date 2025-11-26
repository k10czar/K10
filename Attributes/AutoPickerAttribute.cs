using UnityEngine;

public class AutoPickerAttribute : PropertyAttribute
{
    public readonly bool searchParent;
    public readonly bool searchChildren;

    public readonly string getterMethod;

    public AutoPickerAttribute(bool searchParent = true, bool searchChildren = true)
    {
        this.searchParent = searchParent;
        this.searchChildren = searchChildren;

        this.getterMethod = null;
    }

    public AutoPickerAttribute(string getterMethod)
    {
        this.getterMethod = getterMethod;
    }
}