#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using UnityEngine;

public class GdkPlatformSettingsData : ScriptableObject, IGdkRuntimeData
{
    [SerializeField] string titleId = "FFFFFFFF";
    [SerializeField] string scid = "00000000-0000-0000-0000-0000FFFFFFFF";
    [SerializeField] string saveScid = "00000000-0000-0000-0000-0000FFFFFFFF";
    [SerializeField] string sandbox = "";

    public string Sandbox => sandbox; 
    public string Scid => scid; 
    public string SaveScid => saveScid; 
    public string TitleId => titleId;
    public uint TitleIdNumeric => uint.Parse(titleId, System.Globalization.NumberStyles.HexNumber);
}
#endif