using UnityEngine;

public class GdkPlatformSettingsData : ScriptableObject, IGdkRuntimeData
{
    [SerializeField] string titleId = "75B4A042";
    [SerializeField] string scid = "00000000-0000-0000-0000-000075b4a042";
    [SerializeField] string saveScid = "00000000-0000-0000-0000-000075b4a042";
    [SerializeField] string sandbox = "ZKLCLN.2";

    public string Sandbox => sandbox; 
    public string Scid => scid; 
    public string SaveScid => saveScid; 
    public string TitleId => titleId;
    public uint TitleIdNumeric => uint.Parse(titleId, System.Globalization.NumberStyles.HexNumber);
}