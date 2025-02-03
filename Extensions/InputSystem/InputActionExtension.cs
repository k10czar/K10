using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputActionExtension 
{
    public static Action RegisterTrigger( this InputAction action, Action actionHandler )
    {
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) => { actionHandler(); };
        action.performed += capsule;
        return () => action.performed -= capsule;
    }

    public static Action RegisterTrigger( this InputAction action, ITriggerable actionHandler )
    {
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) => { actionHandler.Trigger(); };
        action.performed += capsule;
        return () => action.performed -= capsule;
    }

    public static Action RegisterBool( this InputAction action, Action<bool> actionHandler )
    {
        Action<InputAction.CallbackContext> trueCapsule = ( InputAction.CallbackContext context ) => { actionHandler( true ); };
        Action<InputAction.CallbackContext> falseCapsule = ( InputAction.CallbackContext context ) => { actionHandler( false ); };
        action.started += trueCapsule;
        action.canceled += falseCapsule;
        return () => 
        {
            action.started -= trueCapsule;
            action.canceled -= falseCapsule;
        };
    }

    public static Action RegisterBool( this InputAction action, ITriggerable<bool> actionHandler )
    {
        Action<InputAction.CallbackContext> trueCapsule = ( InputAction.CallbackContext context ) => { actionHandler.Trigger( true ); };
        Action<InputAction.CallbackContext> falseCapsule = ( InputAction.CallbackContext context ) => { actionHandler.Trigger( false ); };
        action.started += trueCapsule;
        action.canceled += falseCapsule;
        return () => 
        {
            action.started -= trueCapsule;
            action.canceled -= falseCapsule;
        };
    }

    public static Action RegisterValue<T>( this InputAction action, Action<T> actionHandler ) where T: struct
    {
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) => { actionHandler( context.ReadValue<T>() ); };
        action.started += capsule;
        action.performed += capsule;
        action.canceled += capsule;
        return () => 
        {
            action.started -= capsule;
            action.performed -= capsule;
            action.canceled -= capsule;
        };
    }

    public static Action RegisterValue<T>( this InputAction action, ITriggerable<T> actionHandler ) where T: struct
    {
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) => { actionHandler.Trigger( context.ReadValue<T>() ); };
        action.started += capsule;
        action.performed += capsule;
        action.canceled += capsule;
        return () => 
        {
            action.started -= capsule;
            action.performed -= capsule;
            action.canceled -= capsule;
        };
    }
} 