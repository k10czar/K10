using System;
using System.Collections.Generic;
using K10.DebugSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputActionExtension
{
    public static Action RegisterTrigger( this InputAction action, Action actionHandler, InputDevice device = null ) => RegisterTrigger( action, new ActionCapsule( actionHandler ), device );
    public static Action RegisterTrigger( this InputAction action, ITriggerable actionHandler, InputDevice device = null )
    {
        K10Log<InputDebug>.LogVerbose( $"{action.name}.RegisterTrigger()" );
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) =>
        {
            // Debug.Log( $"{action.name} {context.control.device.name??"NULL"} {device?.name??"NULL"} {context.control.device != device}" );
            if( device != null && context.control.device != device ) return;
            actionHandler.Trigger();
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger()" );
        };
        action.performed += capsule;
        return () => action.performed -= capsule;
    }

    public static Action RegisterBool(this InputAction action, Action<bool> actionHandler, InputDevice device = null) => RegisterBool(action, new ActionCapsule<bool>(actionHandler), device);
    public static Action RegisterBool( this InputAction action, ITriggerable<bool> actionHandler, InputDevice device = null )
    {
        K10Log<InputDebug>.LogVerbose( $"{action.name}.RegisterBool()" );
        Action<InputAction.CallbackContext> trueCapsule = ( InputAction.CallbackContext context ) =>
        {
            if( device != null && context.control.device != device ) return;
            actionHandler.Trigger( true );
            // K10Log<InputLogCategory>.LogVerbose( $"{action.name}.Trigger( true )" );
        };
        Action<InputAction.CallbackContext> falseCapsule = ( InputAction.CallbackContext context ) =>
        {
            if( device != null && context.control.device != device ) return;
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

    public static Action RegisterValue<T>(this InputAction action, Action<T> actionHandler, Func<InputAction.CallbackContext,bool> filterFunc = null, InputDevice device = null ) where T : struct => RegisterValue<T>( action, new ActionCapsule<T>(actionHandler), filterFunc, device );
    public static Action RegisterValue<T>( this InputAction action, ITriggerable<T> actionHandler, Func<InputAction.CallbackContext,bool> filterFunc = null, InputDevice device = null ) where T: struct
    {
        K10Log<InputDebug>.LogVerbose( $"{action.name}.RegisterValue()" );
        Action<InputAction.CallbackContext> capsule = ( InputAction.CallbackContext context ) =>
        {
            var value = default( T );
            if( device != null && context.control.device != device ) return;
            if( filterFunc == null || filterFunc ( context ) ) value = context.ReadValue<T>();
            actionHandler.Trigger( value );
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