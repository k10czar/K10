using NUnit.Framework;
using BoolStateOperations;

public class NotOperatorTests
{
    bool Bit( int val, int varId ) { return BoolStateExpresionsTests.Bit( val, varId ); }

    [Test] public void Results()
    {
        var a = new BoolState();
        var not = new Not( a );

        for( int i = 0; i < 2; i++ )
        {
            a.Value = Bit( i, 0 );
            var res = !a.Value;
            Assert.AreEqual( res, not.Value, string.Format( "The NOT operator should result ({0}) but is resulting ({1}) from current operands !({2})", res, not.Value, a.Value ) );
        }
    }

    [Test] public void OnChangeEventParameter()
    {
        for( int i = 0; i < 4; i++ )
        {
            var bs = new BoolState( Bit( i, 0 ) );
            var or = new Not( bs );
            var initial = or.Value;
            var res = ( !Bit( i, 1 ) );

            or.OnChange.Register( ( val ) => Assert.AreEqual( res, val, string.Format( "The OR operator should trigger OnChange with correct current Value ({0}) on change from ({1}) to ({2})", res, Bit( i, 0 ), Bit( i, 1 ) ) ) );
            bs.Value = Bit( i, 1 );
        }
    }

    [Test] public void ResulsOnChangeEvent()
    {
        for( int i = 0; i < 4; i++ )
        {
            var bs = new BoolState( Bit( i, 0 ) );
            var or = new Not( bs );
            bool triggered = BoolStateTests.TestEventTriggerAfterChangeValue( bs, Bit( i, 1 ), or.OnChange );
            bool shouldTrigger = ( Bit( i, 0 ) != Bit( i, 1 ) );
            Assert.AreEqual( shouldTrigger, triggered, string.Format( "BoolState should {0}trigger OnChange event on Value change from ({1}) to ({2})", shouldTrigger ? "" : "NOT ", Bit( i, 0 ), Bit( i, 1 ) ) );
        }
    }

    [Test] public void OnTrueStateEvent()
    {
        for( int i = 0; i < 4; i++ )
        {
            var bs = new BoolState( Bit( i, 0 ) );
            var or = new Not( bs );
            bool triggered = BoolStateTests.TestEventTriggerAfterChangeValue( bs, Bit( i, 1 ), or.OnTrueState );
            bool shouldTrigger = ( Bit( i, 0 ) != Bit( i, 1 ) ) && !Bit( i, 1 );
            Assert.AreEqual( shouldTrigger, triggered, string.Format( "BoolState should {0}trigger OnTrueState event on Value change from ({1}) to ({2})", shouldTrigger ? "" : "NOT ", Bit( i, 0 ), Bit( i, 1 ) ) );
        }
    }

    [Test] public void OnFalseStateEvent()
    {
        for( int i = 0; i < 4; i++ )
        {
            var bs = new BoolState( Bit( i, 0 ) );
            var or = new Not( bs );
            bool triggered = BoolStateTests.TestEventTriggerAfterChangeValue( bs, Bit( i, 1 ), or.OnFalseState );
            bool shouldTrigger = ( Bit( i, 0 ) != Bit( i, 1 ) ) && Bit( i, 1 );
            Assert.AreEqual( shouldTrigger, triggered, string.Format( "BoolState should {0}trigger OnFalseState event on Value change from ({1}) to ({2})", shouldTrigger ? "" : "NOT ", Bit( i, 0 ), Bit( i, 1 ) ) );
        }
    }
}
