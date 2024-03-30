using System.Collections.Generic;
using UnityEngine;

public sealed class TMP_FpsCounter : MonoBehaviour
{
    [Range(.1f,2),SerializeField] float _sampleSecond = .5f;
    [SerializeField,HideInInspector] TMPro.TMP_Text _textMesh;
    List<double> _frame = new List<double>();

    string[] _fpsCounterStringsCache = new string[ 800 ];

    private int _lastFpsValue = -1;

#if UNITY_EDITOR
    void OnValidate()
    {
        this.FindDescendent( ref _textMesh );
    }
#endif

    string RequestString( int value )
    {
        var maxFpsLabel = _fpsCounterStringsCache.Length - 1;
        var clamped = Mathf.Clamp( value, 0, maxFpsLabel );

        if( _fpsCounterStringsCache[clamped] == null )
        {
            var str = value.ToString() + " FPS";
            if( clamped == 0 ) _fpsCounterStringsCache[ clamped ] = "<" + str;
            else if( clamped == maxFpsLabel ) _fpsCounterStringsCache[ clamped ] = ">" + str;
            else _fpsCounterStringsCache[ clamped ] = str;
        }

        return _fpsCounterStringsCache[ clamped ];
    }

    void Update()
    {
        var sampleTime = ( Time.time - _sampleSecond );
        while( _frame.Count > 1 && _frame[0] < sampleTime ) _frame.RemoveAt( 0 );

        // _timeleft -= Time.deltaTime;
        // _accum += Time.timeScale / Time.deltaTime;
        // ++_frames;

        var currTime = Time.time;

        var sample = 1.0;
        if( _frame.Count > 0 ) sample = currTime - _frame[0];

        var fps = (int)( System.Math.Round( ( _frame.Count ) / sample ) + .01 );
        if( _lastFpsValue != fps )
        {
            _textMesh.SetText( RequestString( fps ) );
            _lastFpsValue = fps;
        }

        _frame.Add( Time.time );
    }
}
