using UnityEngine;

public sealed class TMP_FpsCounter : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text _textMesh;
    [SerializeField] FpsCounter _fpsCounter = new( .5f );

    private int _lastFpsValue = -1;

#if UNITY_EDITOR
    void OnValidate()
    {
        this.FindDescendent( ref _textMesh );
    }
#endif

    void Update()
    {
        _fpsCounter.Update();

        var currentFps = _fpsCounter.CurrentFps;
        if( _lastFpsValue != _fpsCounter.CurrentFps )
        {
            _lastFpsValue = currentFps;
            _textMesh.SetText( _fpsCounter.RequestString( currentFps ) );
        }
    }
}
