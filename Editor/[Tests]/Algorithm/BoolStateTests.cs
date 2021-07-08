using NUnit.Framework;

public class BoolStateTests
{
    [Test] public void BoolStateDefaultContructedShouldBeEqualDefaultBool()
    {
        var bs = new BoolState();
        Assert.AreEqual( default(bool), bs.Value, string.Format( "After default consturctor from BoolState the BoolState.Value should return value equals default(bool) = ({0})", default(bool) ) );
    }

    [Test] public void BoolStateContructedTrue()
    {
        var bs = new BoolState( true );
        Assert.AreEqual( true, bs.Value, string.Format( "After consturcted a BoolState with (True) the BoolState.Value should return (True)" ) );
    }

    [Test] public void BoolStateContructedFalse()
    {
        var bs = new BoolState( false );
        Assert.AreEqual( false, bs.Value, string.Format( "After consturcted a BoolState with (False) the BoolState.Value should return (False)" ) );
    }

    [Test] public void BoolStateValueSetTrue()
    {
        var bs = new BoolState();
        bs.Value = true;
        Assert.AreEqual( true, bs.Value, string.Format( "After set a BoolState.Value to (True) the BoolState.Value should return (True)" ) );
    }

    [Test] public void BoolStateValueSetFalse()
    {
        var bs = new BoolState();
        bs.Value = false;
        Assert.AreEqual( false, bs.Value, string.Format( "After set a BoolState.Value to (False) the BoolState.Value should return (False)" ) );
    }

    [Test] public void BoolStateValueSetFalseAfterTrue()
    {
        var bs = new BoolState();
        bs.Value = true;
        bs.Value = false;
        Assert.AreEqual( false, bs.Value, string.Format( "After set a BoolState.Value to (False) the BoolState.Value should return (False)" ) );
    }

    public static bool TestEventTriggerAfterChangeValue( BoolState bs, bool newState, IEventRegister evnt )
    {
        bool triggered = false;
        evnt.Register( () => triggered = true );
        bs.Value = newState;
        return triggered;
    }

    [Test] public void BoolStateOnChangeEventFromTrueToFalse()
    {
        var bs = new BoolState( true );
        bool triggered = TestEventTriggerAfterChangeValue( bs, false, bs.OnChange );
        Assert.IsTrue( triggered, string.Format( "BoolState should trigger OnChange event on Value change from (True) to (False)" ) );
    }

    [Test] public void BoolStateOnChangeEventParameterFromTrueToFalse()
    {
        var bs = new BoolState( true );
        bs.OnChange.Register( ( newVal ) => Assert.IsFalse( newVal, string.Format( "BoolState should trigger OnChange with correct new state" ) ) );
        bs.Value = false;
    }

    [Test] public void BoolStateOnChangeEventFromFalseToTrue()
    {
        var bs = new BoolState( false );
        bool triggered = TestEventTriggerAfterChangeValue( bs, true, bs.OnChange );
        Assert.IsTrue( triggered, string.Format( "BoolState should trigger OnChange event on Value change from (False) to (True)" ) );
    }

    [Test] public void BoolStateOnChangeEventParameterFromFalseToTrue()
    {
        var bs = new BoolState( false );
        bs.OnChange.Register( ( newVal ) => Assert.IsTrue( newVal, string.Format( "BoolState should trigger OnChange with correct new state" ) ) );
        bs.Value = true;
    }

    [Test] public void BoolStateOnTrueStateEventFromFalseToTrue()
    {
        var bs = new BoolState( false );
        bool triggered = TestEventTriggerAfterChangeValue( bs, true, bs.OnTrueState );
        Assert.IsTrue( triggered, string.Format( "BoolState should trigger OnTrueState event on Value change from (False) to (True)" ) );
    }

    [Test] public void BoolStateOnFalseStateEventFromFalseToTrue()
    {
        var bs = new BoolState( false );
        bool triggered = TestEventTriggerAfterChangeValue( bs, true, bs.OnFalseState );
        Assert.IsFalse( triggered, string.Format( "BoolState should NOT trigger OnFalseState event on Value change from (False) to (True)" ) );
    }

    [Test] public void BoolStateOnTrueStateEventFromTrueToFalse()
    {
        var bs = new BoolState( true );
        bool triggered = TestEventTriggerAfterChangeValue( bs, false, bs.OnTrueState );
        Assert.IsFalse( triggered, string.Format( "BoolState should NOT trigger OnTrueState event on Value change from (True) to (False)" ) );
    }

    [Test] public void BoolStateOnFalseStateEventFromTrueToFalse()
    {
        var bs = new BoolState( true );
        bool triggered = TestEventTriggerAfterChangeValue( bs, false, bs.OnFalseState );
        Assert.IsTrue( triggered, string.Format( "BoolState should trigger OnFalseState event on Value change from (True) to (False)" ) );
    }

    [Test] public void FalseBoolStateShouldNotTriggerOnFalseStateEventOnSetValueFalse()
    {
        var bs = new BoolState( false );
        Assert.IsFalse( TestEventTriggerAfterChangeValue( bs, false, bs.OnFalseState ), string.Format( "BoolState should NOT trigger OnFalseState event on Value change from (False) to (False)" ) );
    }

    [Test] public void FalseBoolStateShouldNotTriggerOnTrueStateEventOnSetValueFalse()
    {
        var bs = new BoolState( false );
        Assert.IsFalse( TestEventTriggerAfterChangeValue( bs, false, bs.OnTrueState ), string.Format( "BoolState should NOT trigger OnTrueState event on Value change from (False) to (False)" ) );
    }

    [Test] public void FalseBoolStateShouldNotTriggerOnChangeEventOnSetValueFalse()
    {
        var bs = new BoolState( false );
        Assert.IsFalse( TestEventTriggerAfterChangeValue( bs, false, bs.OnChange ), string.Format( "BoolState should NOT trigger OnChange event on Value change from (False) to (False)" ) );
    }

    [Test] public void TrueBoolStateShouldNotTriggerOnFalseStateEventOnSetValueTrue()
    {
        var bs = new BoolState( true );
        Assert.IsFalse( TestEventTriggerAfterChangeValue( bs, true, bs.OnFalseState ), string.Format( "BoolState should NOT trigger OnFalseState event on Value change from (True) to (True)" ) );
    }

    [Test] public void TrueBoolStateShouldNotTriggerOnTrueStateEventOnSetValueTrue()
    {
        var bs = new BoolState( true );
        Assert.IsFalse( TestEventTriggerAfterChangeValue( bs, true, bs.OnTrueState ), string.Format( "BoolState should NOT trigger OnTrueState event on Value change from (True) to (True)" ) );
    }

    [Test] public void TrueBoolStateShouldNotTriggerOnChangeEventOnSetValueTrue()
    {
        var bs = new BoolState( true );
        Assert.IsFalse( TestEventTriggerAfterChangeValue( bs, true, bs.OnChange ), string.Format( "BoolState should NOT trigger OnChange event on Value change from (True) to (True)" ) );
    }
}
