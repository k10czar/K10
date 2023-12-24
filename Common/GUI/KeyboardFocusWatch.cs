using UnityEngine;

public class KeyboardFocusWatch
{
    private int _lastEdited = -1;

    public bool CheckForLooseFocusEvent()
    {
        var controlId = GUIUtility.GetControlID(FocusType.Keyboard) + 1;
        var currentControl = GUIUtility.keyboardControl;

        var changedLastID = _lastEdited != currentControl;
        var isTheLastID = _lastEdited == controlId;
        var isTheCurrentID = currentControl == controlId;
        if( changedLastID && isTheLastID )
        {
            _lastEdited = -1;
            return true;
        }
        if( changedLastID && _lastEdited == -1 && isTheCurrentID ) 
        {
            _lastEdited = currentControl;
        }
        return false;
    }
}
