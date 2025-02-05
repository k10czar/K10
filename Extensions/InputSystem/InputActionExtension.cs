using System;
using System.Collections.Generic;
using K10.DebugSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputActionExtension 
{
    public static Action RegisterTrigger( this InputAction action, Action actionHandler )
    {
        K10Log<InputLogCategory>.LogVerbose( $"{action.name}.RegisterTrigger()" );
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) => 
        { 
            actionHandler();
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger()" );
        };
        action.performed += capsule;
        return () => action.performed -= capsule;
    }

    public static Action RegisterTrigger( this InputAction action, ITriggerable actionHandler )
    {
        K10Log<InputLogCategory>.LogVerbose( $"{action.name}.RegisterTrigger()" );
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) => 
        { 
            actionHandler.Trigger();
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger()" );
        };
        action.performed += capsule;
        return () => action.performed -= capsule;
    }

    public static Action RegisterBool( this InputAction action, Action<bool> actionHandler )
    {
        K10Log<InputLogCategory>.LogVerbose( $"{action.name}.RegisterBool()" );
        Action<InputAction.CallbackContext> trueCapsule = ( InputAction.CallbackContext context ) => 
        { 
            actionHandler( true );
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger( true )" );
        };
        Action<InputAction.CallbackContext> falseCapsule = ( InputAction.CallbackContext context ) => 
        { 
            actionHandler( false );
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger( false )" );
        };
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
        K10Log<InputLogCategory>.LogVerbose( $"{action.name}.RegisterBool()" );
        Action<InputAction.CallbackContext> trueCapsule = ( InputAction.CallbackContext context ) => 
        { 
            actionHandler.Trigger( true );
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger( true )" );
        };
        Action<InputAction.CallbackContext> falseCapsule = ( InputAction.CallbackContext context ) => 
        { 
            actionHandler.Trigger( false );
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger( false )" );
        };
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
        K10Log<InputLogCategory>.LogVerbose( $"{action.name}.RegisterValue()" );
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) => 
        { 
            actionHandler( context.ReadValue<T>() ); 
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger( {context.ReadValue<T>()} )" );
        };
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
        K10Log<InputLogCategory>.LogVerbose( $"{action.name}.RegisterValue()" );
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) => 
        { 
            actionHandler.Trigger( context.ReadValue<T>() );
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger( {context.ReadValue<T>()} )" );
        };
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