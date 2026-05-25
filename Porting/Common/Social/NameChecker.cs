using System.Collections.Generic;

public static class NameChecker
{
	static string _playerObfuscation = null;
	static Dictionary<string,int> _obfuscationIDs = new();

	public static bool CanShowRemoteNames
	{
		get
		{
			// return false;
#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
			var gdk = ServiceLocator.Get<IGdkRuntimeService>();
			if( gdk != null && gdk.UserData != null && !gdk.UserData.Privileges.UserGeneratedContent.isEnabled ) return false;
#endif
			return true;
		}
	}

	// public static string GetDisplayName( this PhotonPlayer photon )
	// {
	// 	if( photon.IsLocal || CanShowRemoteNames ) return photon.NickName;
	// 	return ObfuscateName( photon.NickName );
	// }

	// public static string GetDisplayName( this PartyMember member )
	// {
	// 	if( member.IsMe || CanShowRemoteNames ) return member.NickName;
	// 	return ObfuscateName( member.DisplayNickName );
	// }

    public static string ObfuscateName( string name )
	{
		if( !_obfuscationIDs.TryGetValue( name, out var id ) )
		{
			id = _obfuscationIDs.Count + 1;
			_obfuscationIDs[name] = id;
		}
		if( string.IsNullOrEmpty( _playerObfuscation ) )
		{
			_playerObfuscation = "Player#{0}";

			// TODO: Can't use localization to avoid circular dependency
			// RosettaStone.CurrentInterpreter.Synchronize( ( interpreter ) => {
			// 	const string key = "Hidden_Player_Name";
			// 	if( interpreter.Contains( key ) ) _playerObfuscation = interpreter.GetText( key );
			// } );
		}
		// var platform = PlatformManager.GetPlatformFromNickname( name );
		// if (platform == EPlatform.COUNT)
		// 	UnityEngine.Debug.LogError($"Tried to obfuscate name without sending prefix with name");

		var obfuscatedName = string.Format( _playerObfuscation, id );
		// return PlatformManager.GetPlatformPrefix( platform ) + obfuscatedName;
		return obfuscatedName;
	}
}
