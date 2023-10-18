using System;

public class CreationPathAttribute : Attribute
{
	public string Path { get; }

	public CreationPathAttribute( string path )
	{
		Path = path;
	}
}
