using System.Collections.Generic;

using NanaManagerAPI.Media;

namespace NanaManagerAPI.UI
{
    /// <summary>
    /// Handles registering custom components, such as <see cref="IMediaViewer"/>s
    /// </summary>
    public static class Registry
    {
		/// <summary>
		/// A dictionary of all data types currently supported with the index related to the viewer that adds them
		/// </summary>
		public static Dictionary<string, int> SupportedDataTypes = new Dictionary<string, int>();
		/// <summary>
		/// A list of all viewers currently loaded
		/// </summary>
		public static List<IMediaViewer> Viewers = new List<IMediaViewer>();
		/// <summary>
		/// A dictionary of all catagories for the importer
		/// </summary>
		public static Dictionary<string, List<string>> Catagories = new Dictionary<string, List<string>>();
		/// <summary>
		/// Registers a class to handle its specified file types
		/// </summary>
		/// <param name="Viewer">The class to act as the handler</param>
		public static void RegisterMediaViewer( IMediaViewer Viewer ) {
			int location = Viewers.Count;
			Viewers.Add( Viewer );
			foreach ( string format in Viewer.GetCompatibleTypes() )
				if ( SupportedDataTypes.ContainsKey( format ) )
					SupportedDataTypes[format] = location;
				else
					SupportedDataTypes.Add( format, location );
		}
		/// <summary>
		/// Adds an extension to a catagory
		/// </summary>
		/// <param name="Catagory">The catagory to add to</param>
		/// <param name="Extension">The extension to add</param>
		public static void AddToCatagory( string Catagory, string Extension ) {
			if ( Catagories.ContainsKey( Catagory ) )
				Catagories[Catagory].Add( Extension );
			else
				Catagories.Add( Catagory, new List<string>() { Extension } );
		}
		/// <summary>
		/// Adds the specified extensions to a filetype catagory
		/// </summary>
		/// <param name="Name">The name of the catagory to add to</param>
		/// <param name="Extensions">The extensions to add</param>
		public static void AddToCatagory( string Name, string[] Extensions ) {
			if ( !Catagories.ContainsKey( Name ) )
				Catagories.Add( Name, new List<string>() );
			foreach ( string ext in Extensions )
				Catagories[Name].Add( ext );
		}
	}
}
