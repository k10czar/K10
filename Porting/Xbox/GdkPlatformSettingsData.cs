#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using UnityEngine;

public class GdkPlatformSettingsData : ScriptableObject, IGdkRuntimeData
{
    [SerializeField] string titleId = "62ab3c24";
    [SerializeField] string scid = "00000000-0000-0000-0000-000062ab3c24";
    [SerializeField] string sandbox = "";

    public string Sandbox => sandbox; 
    public string Scid => scid; 
    public string TitleId => titleId;
}
#endif