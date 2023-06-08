using UnityEngine;

public class TMP_FpsAverageCounter : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text _textMesh;
    private long _frames = 0;
    private double _time = 0;

    private char[] _string = new char[]{ '9', '9', '9', '9', '.', '9', ' ', 'a', 'v', 'g', };
    int _lastRawDataShow = -1;
    bool _canUpdateData = true;

    void Start()
    {
        this.FindDescendent( ref _textMesh );
        UpdateLabel( 0 );
    }

    public void Restart()
    {
        _canUpdateData = true;
        Reset();
    }

    public void Reset()
    {
        _frames = 0;
        _time = 0;
    }

    public void StopData()
    {
        _canUpdateData = false;
    }

    void UpdateLabel( double value )
    {
        var dataToShow = Mathf.RoundToInt( (float)( value * 10 ) );
        if( _lastRawDataShow == dataToShow ) return;
        _lastRawDataShow = dataToShow;
        _string[0] = ( dataToShow > 9999 ) ? (char)( (int)'0' + ( dataToShow % 100000 ) / 10000 ) : ' ';
        _string[1] = ( dataToShow > 999 ) ? (char)( (int)'0' + ( dataToShow % 10000 ) / 1000 ) : ' ';
        _string[2] = ( dataToShow > 99 ) ? (char)( (int)'0' + ( dataToShow % 1000 ) / 100 ) : ' ';
        _string[3] = (char)( (int)'0' + ( dataToShow % 100 ) / 10 );
        _string[5] = (char)( (int)'0' + ( dataToShow % 10 ) );
        _textMesh.SetText( _string );
    }

    void Update()
    {
        if( !_canUpdateData ) return;
        
        _time += Time.deltaTime;
        _frames++;
        if( _time > 0 )
        {
            var avg = _frames / _time;
            UpdateLabel( avg );
        }
    }
}
