using System.Collections.Generic;
using UnityEngine;

public class ResourcesFolderFinder
{
	static List<string> s_resourcesFolders;

	public static IReadOnlyList<string> GetAllResourcesFolders()
	{
		if (s_resourcesFolders != null) return s_resourcesFolders;
		string assetsPath = Application.dataPath;
		s_resourcesFolders = new List<string>();

		// Search all directories named "Resources" inside Assets
		string[] directories = System.IO.Directory.GetDirectories(assetsPath, "Resources", System.IO.SearchOption.AllDirectories);

		var startRelative = Application.dataPath.Length + 1;

		foreach (string dir in directories)
		{
			// Convert full path to relative Unity path
			string relativePath = dir.Substring(startRelative).Replace("\\", "/");
			s_resourcesFolders.Add(relativePath);
		}

		return s_resourcesFolders;
	}
}
