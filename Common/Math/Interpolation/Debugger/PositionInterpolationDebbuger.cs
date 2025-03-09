using UnityEngine;

public class PositionInterpolationDebbuger : IValueInterpolationDebugger<Vector3>
{
    public void Debug( double time, IValueInterpolationDataAccess<Vector3> interpolationData )
    {
        var currId = interpolationData.CurrentPackageId;
        var bufferSize = interpolationData.BufferSize;
        var itPackage = interpolationData.CurrentPackageId + interpolationData.BufferSize;
        var lastData = interpolationData.GetPackageValue( currId );
        for( int i = 1; i < bufferSize; i++ )
        {
            var data = interpolationData.GetPackageValue( ( itPackage - i ) % bufferSize );
		    UnityEngine.Debug.DrawLine( data, lastData, Color.blue );
            var dir = data - lastData;
            if( dir.sqrMagnitude > Mathf.Epsilon )
            {
                var dirNorm = dir.normalized;
                var d = Vector3.Cross( dirNorm, Vector3.up ).normalized;
                var side = d * .25f;
                UnityEngine.Debug.DrawLine( data + side, data - side, Color.blue );
            }
            lastData = data;
        }
        
        lastData = interpolationData.GetPackageValue( currId );
        var lastTime = interpolationData.GetPackageTime( currId );
        if( lastTime < time ) 
        {
            DebugUtils.X( lastData, .25f, Color.yellow, 2);
            return;
        }
        DebugUtils.X( interpolationData.From( time ), .25f, Color.red, 2 );
        DebugUtils.X( lastData, .25f, Color.yellow, 2 );
    }
}
