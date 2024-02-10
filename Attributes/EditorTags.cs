using System;


public class ListingPathAttribute : Attribute
{
	public string Path { get; }

	public ListingPathAttribute( string path )
	{
		Path = path;
	}
}