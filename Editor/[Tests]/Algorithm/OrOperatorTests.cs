using NUnit.Framework;
using BoolStateOperations;

public class OrOperatorTests 
{
    bool Bit( int val, int varId ) { return BoolStateExpresionsTests.Bit( val, varId ); }

    [Test] public void Results2Vars()
    {
        for( int i = 0; i < 4; i++ )
        {
            var a = new BoolState( Bit( i, 0 ) );
            var b = new BoolState( Bit( i, 1 ) );
            var or = new Or( a, b );
            var res = ( a.Value || b.Value );

            Assert.AreEqual( res, or.Value, string.Format( "The OR operator should result ({0}) but is resulting ({1}) from current operors ( {2} || {3} )", res, or.Value, a.Value, b.Value ) );
        }
    }

    [Test] public void Results3Vars()
    {
        for( int i = 0; i < 8; i++ )
        {
            var a = new BoolState( Bit( i, 0 ) );
            var b = new BoolState( Bit( i, 1 ) );
            var c = new BoolState( Bit( i, 2 ) );
            var or = new Or( a, b, c );
            var res = ( a.Value || b.Value || c.Value );

            Assert.AreEqual( res, or.Value, string.Format( "The OR operator should result ({0}) but is resulting ({1}) from current operors ( {2} || {3} || {4} )", res, or.Value, a.Value, b.Value, c.Value ) );
        }
    }

    [Test] public void OnChangeEventParameter()
    {
        for( int i = 0; i < 4; i++ )
        {
            var bs = new BoolState( Bit( i, 0 ) );
            var or = new Or( bs );
            var initial = or.Value;
            var res = ( Bit( i, 1 ) );

            or.OnChange.Register( ( val ) => Assert.AreEqual( res, val, string.Format( "The OR operator should trigger OnChange with correct current Value ({0}) on change from ({1}) to ({2})", res, Bit( i, 0 ), Bit( i, 1 ) ) ) );
            bs.Value = Bit( i, 1 );
        }
    }

    [Test] public void ResulsOnChangeEvent()
    {
        for( int i = 0; i < 4; i++ )
        {
            var bs = new BoolState( Bit( i, 0 ) );
            var or = new Or( bs );
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
            var or = new Or( bs );
            bool triggered = BoolStateTests.TestEventTriggerAfterChangeValue( bs, Bit( i, 1 ), or.OnTrueState );
            bool shouldTrigger = ( Bit( i, 0 ) != Bit( i, 1 ) ) && Bit( i, 1 );
            Assert.AreEqual( shouldTrigger, triggered, string.Format( "BoolState should {0}trigger OnTrueState event on Value change from ({1}) to ({2})", shouldTrigger ? "" : "NOT ", Bit( i, 0 ), Bit( i, 1 ) ) );
        }
    }

    [Test] public void OnFalseStateEvent()
    {
        for( int i = 0; i < 4; i++ )
        {
            var bs = new BoolState( Bit( i, 0 ) );
            var or = new Or( bs );
            bool triggered = BoolStateTests.TestEventTriggerAfterChangeValue( bs, Bit( i, 1 ), or.OnFalseState );
            bool shouldTrigger = ( Bit( i, 0 ) != Bit( i, 1 ) ) && !Bit( i, 1 );
            Assert.AreEqual( shouldTrigger, triggered, string.Format( "BoolState should {0}trigger OnFalseState event on Value change from ({1}) to ({2})", shouldTrigger ? "" : "NOT ", Bit( i, 0 ), Bit( i, 1 ) ) );
        }
    }
}
