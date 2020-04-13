using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Bezier
{
	IEnumerable<Vector3> _points;
	List<Vector3> _tempPts = new List<Vector3>();
	
	public Bezier() { _points = new List<Vector3>(); }
	public Bezier( IEnumerable<Vector3> infoFeeder ) { SetList( infoFeeder ); }

    public float TotalLength(int sampleSize)
    {
        float d = 0f;
        var en = _points.GetEnumerator();
        var ss = (float)sampleSize;
        var lastPoint = GetPoint(0);
        for (int i = 1; i <= sampleSize; i++)
        {
            var point = GetPoint((i) / ss);
            d += Vector3.Distance(lastPoint, point);
            lastPoint = point;
        }
        return d;
        
    }
    public void SetList( IEnumerable<Vector3> positions )
	{
		_points = positions;
	}

	public Vector3 GetPoint( float percentage )
	{
		percentage = Mathf.Clamp01( percentage );

		_tempPts.Clear();
		_tempPts.AddRange( _points );
		
		int it = _tempPts.Count - 1;
		while( it > 0 )
		{
			for( int i = 0; i < it; i++ )
			{
				_tempPts[ i ] = ( ( _tempPts[ i ] * ( 1f - percentage ) ) + ( _tempPts[ i + 1 ] * percentage ) );
			}
			it--;
		}
		
		return ( _tempPts.Count == 0 ) ? Vector3.zero : _tempPts[ 0 ];
	}

	public void FillLine( LineRenderer line, float start, float end, int interactions )
	{
		var dif = end - start;
		line.positionCount = interactions;
		for( int i = 0; i < interactions; i++ )
		{
			line.SetPosition( i, GetPoint( start + ( dif * i / ( interactions - 1 ) ) ) );
		}
	}
	public void OnDrawGizmos( bool showControlLine, int interactions )
	{
		if( showControlLine )
		{
			_tempPts.Clear();
			_tempPts.AddRange( _points );
			for( int i = 1; i < _tempPts.Count; i++ )
				Debug.DrawLine( _tempPts[i - 1], _tempPts[i], Color.red );
		}

		float step = 1f / interactions;
		var pt = GetPoint( 0 );
		for( int i = 0; i <= interactions; i++ )
		{
			float pc = i * step;
			var pt2 = GetPoint( pc + step );
			Debug.DrawLine( pt, pt2, Color.yellow );

			var dist = ( pt2 - pt );
			var middle = pt + ( dist * .5f );

			var dir = dist.normalized;
			var rdir = ( Quaternion.Euler( 0, 0, 90 ) * dir ) * .05f;

			Debug.DrawLine( middle + rdir, middle - rdir, Color.yellow );

			pt = pt2;
		}
	}

}

public class TransformBezier : MonoBehaviour
{
    [SerializeField] List<Transform> _points = new List<Transform>();
    List<Vector3> _tempPts = new List<Vector3>();

    public float TotalLength
    {
        get
        {
            float d = 0f;
            for(int i = 1; i < _points.Count; i++)
            {
                d += Vector3.Distance(_points[i - 1].position, _points[i].position);
            }
            return d;
        }
    }

    public void SetPoints(List<Transform> t)
    {
        _tempPts.Clear();
        _points = t;
    }

    List<Vector3> Points
    {
        get
        {
            List<Vector3> pts = new List<Vector3>();
            foreach( var t in _points ) { if( t!= null ) pts.Add( t.position ); }
            return pts;
        }
    }
    
    
    public Vector3 GetPoint( float percentage )
    {
        percentage = Mathf.Clamp01( percentage );

        for( int i = 0; i < _points.Count; i++ )
        {
            var t = _points[ i ];
            if( _tempPts.Count > i ) _tempPts[ i ] = t.position;
            else _tempPts.Add( t.position );
        }

        int it = _tempPts.Count - 1;
        while( it > 0 )
        {
            for( int i = 0; i < it; i++ )
            {
                _tempPts[ i ] = ( ( _tempPts[ i ] * ( 1f - percentage ) ) + ( _tempPts[ i + 1 ] * percentage ) );
            }
            it--;
        }

        return ( _tempPts.Count == 0 ) ? Vector3.zero : _tempPts[ 0 ];
    }
    
    
    void OnDrawGizmos()
    {
        int interactions = 30;
        
        float step = 1f / interactions;
        
        var pts = Points;
        for( int i = 1; i < _points.Count; i++ )
        {
            Debug.DrawLine( _points[ i - 1 ].position, _points[ i ].position, Color.red );
        }
        
        for( int i = 0; i <= interactions; i++ )
        {
            float pc = i * step;
            Debug.DrawLine( GetPoint( pc ), GetPoint( pc + step ), Color.yellow );
        }
    }
}
