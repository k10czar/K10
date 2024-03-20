using System.Diagnostics;
using NUnit.Framework;

public class CastingTests 
{
    private const string FIRST_STR = "INITED";
    private const string NEW_STR = "RESETED";
    private const long FIRST_LONG = 98216579;
    private const long NEW_LONG = 8798789095848;
    [ConstLike] private static readonly string NEW_LONGSTR = NEW_LONG.ToString();
    private const bool LOG_FAIL = false;


    [Test] public void TrySetStringOnString()
    {
        var str = FIRST_STR;
        NEW_STR.TrySetOn( ref str, LOG_FAIL );
        Assert.AreEqual( str, NEW_STR, $"After a \"{NEW_STR}\".TrySetOn( ref str:\"{FIRST_STR}\" ) the value should be str:\"{NEW_STR}\"" );
    }

    [Test] public void TrySetNullOnString()
    {
        var str = FIRST_STR;
        object nullObj = null;
        nullObj.TrySetOn( ref str, LOG_FAIL );
        Assert.AreEqual( str, FIRST_STR, $"After a NULL.TrySetOn( ref str:\"{FIRST_STR}\" ) the value should be str:\"{FIRST_STR}\"" );
    }

    [Test] public void TrySetLongOnLong()
    {
        var lng = FIRST_LONG;
        NEW_LONG.TrySetOn( ref lng, LOG_FAIL );
        Assert.AreEqual( lng, NEW_LONG, $"After a {NEW_LONG}.TrySetOn( ref lng:{FIRST_LONG} ) the value should be lng:{NEW_LONG}" );
    }

    [Test] public void TrySetStringOnLong()
    {
        var lng = FIRST_LONG;
        NEW_LONGSTR.TrySetOn( ref lng, LOG_FAIL );
        Assert.AreEqual( lng, NEW_LONG, $"After a \"{NEW_LONGSTR}\".TrySetOn( ref lng:{FIRST_LONG} ) the value should be lng:{NEW_LONG}" );
    }

    [Test] public void TrySetTupleOnStringLong()
    {
        var tuple = ( FIRST_STR, FIRST_LONG );
        ( NEW_STR, NEW_LONG ).TrySetOn( ref tuple, LOG_FAIL );
        Assert.AreEqual( tuple, ( NEW_STR, NEW_LONG ), $"After a ( \"{NEW_STR}\", {NEW_LONGSTR} ).TrySetOn( ref tuple:( \"{FIRST_STR}\", {FIRST_LONG} ) ) the value should be tuple:( \"{NEW_STR}\", {NEW_LONGSTR} )" );
    }

    [Test] public void TrySetTupleOnStringLongInverted()
    {
        var tuple = ( FIRST_STR, FIRST_LONG );
        ( NEW_LONG, NEW_STR ).TrySetOn( ref tuple, LOG_FAIL );
        Assert.AreEqual( tuple, ( NEW_STR, NEW_LONG ), $"After a ( {NEW_LONGSTR}, \"{NEW_STR}\" ).TrySetOn( ref tuple:( \"{FIRST_STR}\", {FIRST_LONG} ) ) the value should be tuple:( \"{NEW_STR}\", {NEW_LONGSTR} )" );
    }

    [Test] public void TrySetStringTupleOnLongLong()
    {
        var tuple = ( FIRST_LONG, FIRST_LONG );
        ( NEW_LONGSTR, NEW_LONGSTR ).TrySetOn( ref tuple, LOG_FAIL );
        Assert.AreEqual( tuple, ( NEW_LONG, NEW_LONG ), $"After a ( \"{NEW_LONGSTR}\", \"{NEW_LONGSTR}\" ).TrySetOn( ref tuple:( {FIRST_LONG}, {FIRST_LONG} ) ) the value should be tuple:( {NEW_LONGSTR}, {NEW_LONGSTR} )" );
    }

    [Test] public void TrySetLongTupleOnLongLong()
    {
        var tuple = ( FIRST_LONG, FIRST_LONG );
        ( NEW_LONG, NEW_LONG ).TrySetOn( ref tuple, LOG_FAIL );
        Assert.AreEqual( tuple, ( NEW_LONG, NEW_LONG ), $"After a ( {NEW_LONGSTR}, {NEW_LONGSTR} ).TrySetOn( ref tuple:( {FIRST_LONG}, {FIRST_LONG} ) ) the value should be tuple:( {NEW_LONGSTR}, {NEW_LONGSTR} )" );
    }
}