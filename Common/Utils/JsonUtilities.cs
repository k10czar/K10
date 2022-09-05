using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class JsonUtilities
{
	public static string DEBUG_FOLDER => ( UnityEngine.Application.persistentDataPath + "/JsonDebug/" );

	public static string LogToJsonFile( this string rawJson, string prefix = "", string suffix = "" , string environment = "")
	{
		var formattedJson = rawJson.FormatAsJson();
		var fileName = DateTime.Now.ToFileTimeUtc().ToString();
		if( !string.IsNullOrEmpty( prefix ) ) fileName = prefix + "_" + fileName;
		if( !string.IsNullOrEmpty( suffix ) ) fileName = fileName + "_" + suffix;
		if (!string.IsNullOrEmpty(environment)) fileName = environment + "_" + fileName;
		FileAdapter.SaveHasUTF8( DEBUG_FOLDER + fileName + ".json", formattedJson );
		#if UNITY_EDITOR
		Debug.Log( fileName + ": " + rawJson.FormatAsJson( "    " ) );
		#endif //UNITY_EDITOR
		return fileName;
	}

	private static readonly StringBuilder sb = new StringBuilder();
	private static readonly HashSet<char> ignoredChars = new HashSet<char> { '\t', '\n', '\r', ' ' };
	private static readonly HashSet<char> opens = new HashSet<char> { '{', '[' };
	private static readonly HashSet<char> newLineRequest = new HashSet<char> { ',' };
	private static readonly HashSet<char> spaceRequest = new HashSet<char> { ':' };
	private static readonly HashSet<char> closes = new HashSet<char> { '}', ']' };
	private const int MAX_INLINE_CHARS = 40;
	public static string FormatAsJson( this string rawJson, string tabulation = "\t" )
	{
		sb.Clear();
		int indent = 0;
		bool inline = false;
		bool insideStr = false;
		for( int i = 0; i < rawJson.Length; i++ )
		{
			var c = rawJson[i];

			if( c == '\"' ) insideStr = !insideStr;

			if( insideStr )
			{
				sb.Append( c );
				continue;
			}

			if( ignoredChars.Contains( c ) ) continue;

			var open = opens.Contains( c );
			var close = closes.Contains( c );
			var nlr = newLineRequest.Contains( c );
			var sr = spaceRequest.Contains( c );

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
				while( used < MAX_INLINE_CHARS && j < rawJson.Length )
				{
					var nc = rawJson[j];
					if( closes.Contains( nc ) )
					{
						inline = true;
						break;
					}
					if( opens.Contains( nc ) ) break;
					if( !ignoredChars.Contains( nc ) ) used++;
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
