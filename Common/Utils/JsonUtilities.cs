using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine;

public static class JsonUtilities
{
	public static string DEBUG_FOLDER => ( FileAdapter.debugPersistentDataPath + "/JsonDebug/" );
	
	public static string GenerateSavePath(string fileName, string fileExtension)
     => $"{DEBUG_FOLDER}{fileName}{fileExtension}";

	public static string GenerateLogFileName(string suffix1 = "", string suffix2 = "", string environment = "")
	{
		var fileName = DateTime.Now.ToString("yyyyMMddTHHmmssfffffff");

		if (!string.IsNullOrEmpty(suffix1)) fileName = fileName + "_" + suffix1;
		if (!string.IsNullOrEmpty(suffix2)) fileName = fileName + "_" + suffix2;
		if (!string.IsNullOrEmpty(environment)) fileName = environment + "_" + fileName;

		return fileName;
	}


	public static string GenerateSavePath(string fileName, string fileExtension)
		=> $"{DEBUG_FOLDER}{fileName}{fileExtension}";

	public static string GenerateLogFileName(string suffix1 = "", string suffix2 = "", string environment = "")
	{
		var fileName = DateTime.Now.ToString("yyyyMMddTHHmmssfffffff");

		if (!string.IsNullOrEmpty(suffix1)) fileName = fileName + "_" + suffix1;
		if (!string.IsNullOrEmpty(suffix2)) fileName = fileName + "_" + suffix2;
		if (!string.IsNullOrEmpty(environment)) fileName = environment + "_" + fileName;

		return fileName;
	}

	public static string LogToJsonFile(this string rawJson, string suffix1 = "", string suffix2 = "", string environment = "")
	{
		var formattedJson = rawJson.FormatAsJson();

		var fileName = GenerateLogFileName(suffix1, suffix2, environment);
		var savePath = GenerateSavePath(fileName, ".json");

		FileAdapter.SaveHasUTF8(savePath, formattedJson);
#if UNITY_EDITOR
		Debug.Log(fileName + ": " + formattedJson);
#endif //UNITY_EDITOR
		return fileName;
	}

	private static readonly StringBuilder sb = new StringBuilder();
	private static readonly HashSet<char> ignoredChars = new HashSet<char> { '\t', '\n', '\r', ' ' };
	private static readonly HashSet<char> opens = new HashSet<char> { '{', '[' };
	private static readonly HashSet<char> newLineRequest = new HashSet<char> { ',' };
	private static readonly HashSet<char> spaceRequest = new HashSet<char> { ':' };
	private static readonly HashSet<char> closes = new HashSet<char> { '}', ']' };
	private static readonly HashSet<char> validChars = new HashSet<char> { '{', '[', ',', ':', '}', ']' };
	private static byte[] codes = null;
	private const int CODES_LENGTH = 128;

	private const byte ignoreCode = 1 << 0;
	private const byte opensCode = 1 << 1;
	private const byte newLineRequestCode = 1 << 2;
	private const byte spaceRequestCode = 1 << 3;
	private const byte closesCode = 1 << 4;
	private const byte insideCode = 1 << 5;

	private const int MAX_INLINE_CHARS = 40;
	public static string FormatAsJson( this string rawJson, string tabulation = "\t" )
	{
		if( codes == null )
		{
			codes = new byte[CODES_LENGTH];
			for( int i = 0; i < CODES_LENGTH; i++ ) codes[i] = 0;

			foreach( var c in ignoredChars ) codes[(int)c] |= ignoreCode;
			foreach( var c in opens ) codes[(int)c] |= opensCode;
			foreach( var c in newLineRequest ) codes[(int)c] |= newLineRequestCode;
			foreach( var c in spaceRequest ) codes[(int)c] |= spaceRequestCode;
			foreach( var c in closes ) codes[(int)c] |= closesCode;
			codes[(int)'\"'] |= insideCode;
		}


		sb.Clear();
		int indent = 0;
		bool inline = false;
		bool insideStr = false;

		var rawJsonLength = rawJson.Length;
		for( int i = 0; i < rawJsonLength; i++ )
		{
			var c = rawJson[i];

			var code = 0;
			var ascii = (int)c;
			if( ascii < CODES_LENGTH ) code = codes[ ascii ];

			if( ( code & insideCode ) != 0 ) insideStr = !insideStr;
			// if( c == '\"' ) insideStr = !insideStr;

			if( insideStr )
			{
				sb.Append( c );
				continue;
			}

			if( ( code & ignoreCode ) != 0 ) continue;
			// if( ignoredChars.Contains( c ) ) continue;

			var open = ( code & opensCode ) != 0;
			var close = ( code & closesCode ) != 0;
			var nlr = ( code & newLineRequestCode ) != 0;
			var sr = ( code & spaceRequestCode ) != 0;

			// if( validChars.Contains( c ) )
			// {
			// 	open = opens.Contains( c );
			// 	close = closes.Contains( c );
			// 	nlr = newLineRequest.Contains( c );
			// 	sr = spaceRequest.Contains( c );
			// }

			if( close )
			{
				if( !inline ) NewIdentation( sb, --indent, tabulation );
				else sb.Append( ' ' );
				inline = false;
			}
			sb.Append( c );
			if( open )
			{
				inline = false;
				int j = i + 1;
				int used = 0;
				while( used < MAX_INLINE_CHARS && j < rawJsonLength )
				{
					var nc = rawJson[j];
					var nextCode = 0;
					var nextAscii = (int)nc;
					if( nextAscii < CODES_LENGTH ) nextCode = codes[ nextAscii ];
					
					var nextClose = ( nextCode & closesCode ) != 0;
					if( nextClose )
					{
						inline = true;
						break;
					}
					var nextOpen = ( nextCode & opensCode ) != 0;
					if( nextOpen ) break;
					var nextIgnored = ( nextCode & ignoreCode ) != 0;
					if( !nextIgnored ) used++;
					j++;
				}
				if( !inline ) NewIdentation( sb, ++indent, tabulation );
				else sb.Append( ' ' );
			}
			if( nlr )
			{
				if( inline ) sb.Append( ' ' );
				else NewIdentation( sb, indent, tabulation );
			}
			if( sr ) sb.Append( ' ' );
		}
		var str = sb.ToString();
		sb.Clear();
		return str;
	}

	private static void NewIdentation( StringBuilder sb, int identLevel, string tabulation = "\t" )
	{
		sb.Append( '\n' );
		for( int j = 0; j < identLevel; j++ ) sb.Append( tabulation );
	}

	public static string RemoveStringInsideBrackets(string mainString, string startingBracketSubString)
	{
		int startIndex = mainString.LastIndexOf(startingBracketSubString) + startingBracketSubString.Length-1;
		if (startIndex < 0)
		{
			return mainString;
		}

		int bracketCount = 0;
		int finalIndex = -1;
		for(int i = startIndex; i < mainString.Length; i++){
			if(mainString[i] == '['){
				bracketCount++;
			}
			else if(mainString[i] == ']'){
				bracketCount--;
				if(bracketCount <= 0){
					finalIndex = i;
					break;
				}
			}
		}

		if (finalIndex == -1)
		{
			return mainString;
		}

		string resultString = mainString.Remove(startIndex+1, finalIndex - startIndex-1);
		return resultString;
	}
}
