using NUnit.Framework;
using BoolStateOperations;


public class BoolStateExpresionsTests
{
    public static bool Bit( int val, int varId ) { return ( val & ( 1 << varId ) ) != 0; }
}
