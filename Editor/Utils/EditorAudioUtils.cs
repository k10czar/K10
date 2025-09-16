using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EditorAudioUtils
{
    static MethodInfo _previewPlayMethod;
    static object[] _params = new object[] { null, 0, false };

    static MethodInfo PreviewPlayMethod
    {
        get 
        { 
            if( _previewPlayMethod == null )
            {
                Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
                Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
                _previewPlayMethod = audioUtilClass.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip), typeof(Int32), typeof(Boolean) }, null);
            }
            return _previewPlayMethod;
        }
    }

    public static void PlayClip(string path)
	{
		var clip = Resources.Load<AudioClip>(path);
		PlayNoAlloc( clip );
	}

    public static void PlayClip(AudioClip clip)
    {
        PlayNoAlloc( clip );
    }

    public static void EditorPreview( this AudioClip clip )
    {
        PlayNoAlloc( clip );
    }

    static void PlayNoAlloc( AudioClip clip )
    {
		if( clip == null ) return;
        _params[0] = clip;
        PreviewPlayMethod.Invoke(null, _params );
    }
}
