using System.Collections.Generic;
using UnityEngine;

public sealed class TMP_FpsCounter : MonoBehaviour
{
    [Range(.1f,2),SerializeField] float _sampleSecond = .5f;
    [SerializeField,HideInInspector] TMPro.TMP_Text _textMesh;
    List<float> _frame = new List<float>();
    private float _accum = 0;
    // FPS accumulated over the interval
    private int _frames = 0;
    // Frames drawn over the interval
    private float _timeleft;
    // Left time for current interval

    //Avoid GC Collection;
    string[] _fpsCounterStrings = new string[ 250 ];

    private string _updateFPS;

#if UNITY_EDITOR
    void OnValidate()
    {
        this.FindDescendent( ref _textMesh );
    }
#endif

    string RequestString( int value )
    {
        var clamped = Mathf.Clamp( value, 0, _fpsCounterStrings.Length - 1 );

        if( _fpsCounterStrings[clamped] == null )
        {
            var str = value.ToString() + " FPS";
            if( clamped == 0 ) _fpsCounterStrings[ clamped ] = "<" + str;
            else if( clamped == _fpsCounterStrings.Length - 1 ) _fpsCounterStrings[ clamped ] = ">" + str;
            else _fpsCounterStrings[ clamped ] = str;
        }

        return _fpsCounterStrings[ clamped ];
    }

    void Update()
    {
        var sampleTime = ( Time.time - _sampleSecond );
        while( _frame.Count > 0 && _frame[0] < sampleTime ) _frame.RemoveAt( 0 );

        _timeleft -= Time.deltaTime;
        _accum += Time.timeScale / Time.deltaTime;
        ++_frames;

        var currTime = Time.time;

        var sample = 1f;
        if( _frame.Count > 0 ) sample = currTime - _frame[0];

        _textMesh.text = RequestString( Mathf.RoundToInt( ( _frame.Count + 1 ) / sample ) );

        _frame.Add( Time.time );
    }
}
