using System.Text;
using NUnit.Framework;

public class SerializationIntTest
{
    [Test]
    public void TestPositiveOneBitValuesSerialization()
    {
        var bytes = new byte[4];
        
        var SB = new StringBuilder();
        for( int i = 0; i < 31; i++ )
        {
            var val = 1 << i;
            var it = 0;
            bytes.SerializeIntAsBits( false, ref val, ref it, 32 );

            var readedVal = 0;
            it = 0;
            bytes.SerializeIntAsBits( true, ref readedVal, ref it, 32 );

            SB.AppendLine( $"{val} ({System.Convert.ToString(val, 2)}), " );

            Assert.AreEqual( val, readedVal, $"Serialized value: {val} deserialized as: {readedVal} error:{val-readedVal}\n{SB}" );
        }

        UnityEngine.Debug.Log( $"TestPositiveOneBitValuesSerialization() validated serialization of:\n{SB}" );
    }

    [Test]
    public void TestPositiveFullBitsValuesSerialization()
    {
        var bytes = new byte[4];
        
        var SB = new StringBuilder();
        for( int i = 1; i < 32; i++ )
        {
            var val = ( 1 << i ) - 1;
            var it = 0;
            bytes.SerializeIntAsBits( false, ref val, ref it, 32 );

            var readedVal = 0;
            it = 0;
            bytes.SerializeIntAsBits( true, ref readedVal, ref it, 32 );
            
            SB.AppendLine( $"{val} ({System.Convert.ToString(val, 2)}), " );

            Assert.AreEqual( val, readedVal, $"Serialized value: {val} deserialized as: {readedVal} error:{val-readedVal}\n{SB}" );
        }
        
        UnityEngine.Debug.Log( $"TestPositiveFullBitsValuesSerialization() validated serialization of:\n{SB}" );
    }

    [Test]
    public void TestNegativeOneBitValuesSerialization()
    {
        var bytes = new byte[4];
        
        var SB = new StringBuilder();
        for( int i = 0; i < 31; i++ )
        {
            var val = ( 1 << i ) | int.MinValue;
            var it = 0;
            bytes.SerializeIntAsBits( false, ref val, ref it, 32 );

            var readedVal = 0;
            it = 0;
            bytes.SerializeIntAsBits( true, ref readedVal, ref it, 32 );
            
            SB.AppendLine( $"{val} ({System.Convert.ToString(val, 2)}), " );

            Assert.AreEqual( val, readedVal, $"Serialized value: {val} deserialized as: {readedVal} error:{val-readedVal}\n{SB}" );
        }

        UnityEngine.Debug.Log( $"TestNegativeOneBitValuesSerialization() validated serialization of:\n{SB}" );
    }

    [Test]
    public void TestNegativeFullBitsValuesSerialization()
    {
        var bytes = new byte[4];
        
        var SB = new StringBuilder();
        for( int i = 1; i < 32; i++ )
        {
            var val = ( ( 1 << i ) - 1 ) | int.MinValue;
            var it = 0;
            bytes.SerializeIntAsBits( false, ref val, ref it, 32 );

            var readedVal = 0;
            it = 0;
            bytes.SerializeIntAsBits( true, ref readedVal, ref it, 32 );
            
            SB.AppendLine( $"{val} ({System.Convert.ToString(val, 2)}), " );

            Assert.AreEqual( val, readedVal, $"Serialized value: {val} deserialized as: {readedVal} error:{val-readedVal}\n{SB}" );
        }

        UnityEngine.Debug.Log( $"TestNegativeFullBitsValuesSerialization() validated serialization of:\n{SB}" );
    }
}
