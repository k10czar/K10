using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScreenShoter : MonoBehaviour
{
    [SerializeField] bool _takePicture;
    [SerializeField] string _file = "ss";
	
	void Update()
    {
        if( _takePicture )
        {
            ScreenCapture.CaptureScreenshot( _file + ".png" );
            _takePicture = false;
        }
	}
}
