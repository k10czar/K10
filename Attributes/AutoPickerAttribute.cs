using UnityEngine;

public class AutoPickerAttribute : PropertyAttribute
{
    public readonly bool searchParent;
    public readonly bool searchChildren;

    public AutoPickerAttribute(bool searchParent = true, bool searchChildren = true)
    {
        this.searchParent = searchParent;
        this.searchChildren = searchChildren;
    }
}