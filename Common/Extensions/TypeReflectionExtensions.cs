using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace K10.Reflection.Extensions
{
    public static class TypeReflectionExtensions
    {
        public static string[] ReflectListMembers<T>( this Type type, Func<T, string, string> debugColor, BindingFlags flags, int batches = 1 )
        {
            if( type == null ) return new string[]{ $"Cannot list Members of NULL" };
            var allColors = type.GetFields( flags );
            List<int> batchesDirector = new List<int>();
            var count = allColors.Count();
            if( count == 0 ) return new string[]{ $"No members defined on {type.FullName}" };
            for( int i = 1; i < batches; i++ ) batchesDirector.Add( count * i / batches );
            batchesDirector.Add( count );

            var debugs = new string[batchesDirector.Count];
            var it = 0;
            var SB = new StringBuilder();
            SB.Append( $"Members defined on {type.FullName}: " );
            for( int bi = 0; bi < batchesDirector.Count; bi++ )
            {
                var stopAt = batchesDirector[bi];
                for( ; it < stopAt; it++ )
                {
                    var field = allColors[it];
                    var value = field.GetValue( null );
                    if( value is T t ) SB.Append( $"{debugColor(t,field.Name)} " );
                }
                var itContinues = ( bi + 1 ) < batchesDirector.Count;
                SB.Append( itContinues ? "..." : "." );
                debugs[bi] = SB.ToString();
                SB.Clear();
            }
            return debugs;
        }
    }
}