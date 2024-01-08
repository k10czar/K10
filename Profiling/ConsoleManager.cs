using System;
using System.Collections.Generic;
using K10;
using UnityEngine;

using static Colors.Console;

public class ConsoleManager
{
    const KeyCode CONSOLE_POP_NUMBER_PILE = KeyCode.Backspace;
    const KeyCode CONSOLE_HELPER_TOGGLE = KeyCode.BackQuote;

    bool _debugAllKeysDownDetection = false;
    bool _showOnScreenDebug;
    bool _debugLogs = true;
    bool _isDirty = true;

    string _text = "NULL";
    string _shadowText = "NULL";

    List<int> _numberPile = new List<int>();

    IConsoleInteractor _currentConsole;
    Dictionary<KeyCode,IConsoleInteractor> _registeredConsoles = new Dictionary<KeyCode, IConsoleInteractor>();

    public string DebugNumberPile() => $"NumberPile:{((_numberPile.Count>0)?string.Join("",_numberPile):"EMPTY")}";
    public string DebugNumberPileColored() => $"{"NumberPile".Colorfy(Fields)}:{((_numberPile.Count>0)?string.Join("",_numberPile):"EMPTY".Colorfy(Negation))}";

    public int PileValue
    {
        get
        {
            var val = 0;
            for( int i = 0; i < _numberPile.Count; i++ )
            {
                val = val * 10 + _numberPile[i];
            }
            return val;
        }
    }

    public void SetDirty()
    {
        _isDirty = true;
    }

    public ConsoleManager With( KeyCode keyCode, IConsoleInteractor console ) 
    {
        Register( keyCode, console );
        return this;
    }

    public void Register( KeyCode keyCode, IConsoleInteractor console )
    {
        if( _registeredConsoles.TryGetValue( keyCode, out var oldConsole ) )
        {
            if( oldConsole == console )
            {
                if( _debugLogs ) Debug.Log( $"{"Console".Colorfy(Keyword)} {"Registration".Colorfy(Verbs)} {"Conflict".Colorfy(Negation)} {console.ToStringColored(TypeName)} already registered at {keyCode.ToStringColored(Numbers)}" );
                return;
            }
            if( _debugLogs ) Debug.Log( $"{"Console".Colorfy(Keyword)} {"Registration".Colorfy(Verbs)} {"Conflict".Colorfy(Negation)} {keyCode.ToStringColored(Numbers)} already has {oldConsole.ToStringColored(TypeName)} but will be replaced with {console.ToStringColored(TypeName)}" );
        }

        _registeredConsoles[keyCode] = console;
        if( _debugLogs ) Debug.Log( $"{"Console".Colorfy(Keyword)} {console.ToStringOrNullColored(TypeName)} registred with {keyCode.ToStringColored(Numbers)}" );
        _isDirty = true;
    }

    private void ResetNumberPile()
    {
        _numberPile.Clear();
        SetDirty();
    }

    private void ToogleConsoleHelper()
    {
        if (Input.GetKeyDown(CONSOLE_HELPER_TOGGLE))
        {
            _showOnScreenDebug = !_showOnScreenDebug;
            _currentConsole = null;
            ResetNumberPile();
            if( _debugLogs ) Debug.Log($"{"Console".Colorfy(Keyword)} as detected {CONSOLE_HELPER_TOGGLE.ToStringColored(Numbers)} translating to {"Toogle Console Help".Colorfy(EventName)}: {_showOnScreenDebug.ToStringColored()}");
        }
    }

    private void CheckNumberPilePop()
    {
        if (Input.GetKeyDown(CONSOLE_POP_NUMBER_PILE))
        {
            if (_numberPile.Count > 0)
            {
                var popedNumber = _numberPile[_numberPile.Count - 1];
                _numberPile.RemoveAt(_numberPile.Count - 1);
                _isDirty = true;
                if( _debugLogs ) Debug.Log($"{"Console".Colorfy(Keyword)} as detected {CONSOLE_POP_NUMBER_PILE.ToStringColored(Numbers)} translating to {"Number Pile Pop".Colorfy(EventName)}: {DebugNumberPileColored()} poped: {popedNumber.ToStringColored(Negation)}");
            }
        }
    }

    public void Update()
    {
        if (_debugAllKeysDownDetection) DebugAllKeysDown();
        ToogleConsoleHelper();
        foreach( var binding in _registeredConsoles )
        {
            if( !Input.GetKeyDown( binding.Key ) ) continue;
            if( _currentConsole == binding.Value )
            {
                if( _debugLogs ) Debug.Log($"{"Console".Colorfy(Keyword)} as detected {binding.Key.ToStringColored(Numbers)} so will close already open console {_currentConsole.ToStringColored(Numbers)}");
                _currentConsole = null;
            }
            else
            {
                _currentConsole = binding.Value;
                if( _debugLogs ) Debug.Log($"{"Console".Colorfy(Keyword)} as detected {binding.Key.ToStringColored(Numbers)} so will open console {_currentConsole.ToStringColored(Numbers)}");
            }
            ResetNumberPile();
            break;
        }

        if( _currentConsole == null ) return;

        CheckNumberPilePop();
        var actionsCount = _currentConsole.ActionsCount;
        var maxNum = Mathf.Min( actionsCount, 10 );
        var numberPileValue = PileValue * 10;

        var maxDigitsCount = actionsCount.GetDigitsCount();

        for (int i = 0; i < maxNum; i++)
        {
            var keyCode = KeyCode.Keypad0 + i;
            var keyCodeAlternate = KeyCode.Alpha0 + i;
            var keyCodePressed = Input.GetKeyDown(keyCode);
            var keyCodeAlternatePressed = Input.GetKeyDown(keyCodeAlternate);
            if (!keyCodePressed && !keyCodeAlternatePressed) continue;

            var pretendedValue = numberPileValue + i;
            var ambigous = pretendedValue * 10 < actionsCount && maxDigitsCount > ( _numberPile.Count + 1 );

            if( ambigous )
            {
                _numberPile.Add( i );
                _isDirty = true;
                if( _debugLogs ) Debug.Log($"{"Console".Colorfy(Keyword)} as detected {(keyCodePressed ? keyCode : keyCodeAlternate).ToStringColored(Numbers)} so added {i.ToStringColored(Numbers)} to {DebugNumberPileColored()}");
                continue;
            }

            if( pretendedValue >= actionsCount )
            {
                if( _debugLogs ) Debug.Log($"{"Console".Colorfy(Keyword)} as detected {(keyCodePressed ? keyCode : keyCodeAlternate).ToStringColored(Numbers)} translating to code {pretendedValue.ToStringColored(Numbers)} current state of {DebugNumberPileColored()} but is not valid action code");
                _numberPile.Clear();
                _isDirty = true;
                break;
            }

            if( _debugLogs ) Debug.Log($"{"Console".Colorfy(Keyword)} as detected {(keyCodePressed ? keyCode : keyCodeAlternate).ToStringColored(Numbers)} and with {DebugNumberPileColored()} was translated to code {pretendedValue.ToStringColored(Numbers)} with action: {_currentConsole.GetActionDebugNameColored(pretendedValue)}");
            ResetNumberPile();
            _currentConsole.DoAction( pretendedValue );
        }

        if( _currentConsole.HasBack )
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if( _debugLogs ) Debug.Log($"{"Console".Colorfy(Keyword)} as detected {KeyCode.Escape.ToStringColored(Numbers)} translating to action: {"Back".ToStringOrNullColored(EventName)}");
                _currentConsole.Back();
                ResetNumberPile();
            }
        }
    }

    private static void DebugAllKeysDown()
    {
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                Debug.Log($"{"Console".Colorfy(Keyword)} as detected {"KeyDown".Colorfy(EventName)}: {keyCode.ToStringColored(Numbers)}");
            }
        }
    }

    public void OnGUI()
    {
        if( _currentConsole == null && !_showOnScreenDebug ) return;
        
        var shadowOffset = new Vector2( 1.5f, 1.5f );
        var margin = new Vector2( 10, 10 );
        var area = new Rect( 0, 0, Screen.width - 2 * margin.x, Screen.height / 2 ).Move( margin );

        if( _isDirty || ( _currentConsole?.IsDirty ?? false ) )
        {
            _isDirty = false;
            if( _currentConsole != null ) _text = $"{DebugNumberPileColored()}\n{_currentConsole.Text}";
            else _text = $"{string.Join( ",\n    ", _registeredConsoles )}";
            _shadowText = System.Text.RegularExpressions.Regex.Replace(_text, "<.*?>", "");
        }

        GuiColorManager.New( Color.black );
        GUI.Label( area.Move( shadowOffset ), _shadowText );
        GuiColorManager.Revert();
        GUI.Label( area, _text );
    }
}
