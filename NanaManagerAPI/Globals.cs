using System;
using System.IO;
using System.Collections.Generic;

using NanaManagerAPI.Cryptography;
using NanaManagerAPI.Media;

namespace NanaManagerAPI
{
	/// <summary>
	/// A delegate for encrypting and decrypting data
	/// </summary>
	/// <param name="Data">The data to perform the cryptographic function on</param>
	/// <param name="Password">The password for the function</param>
	/// <returns>The data resultant of the cryptographic function</returns>
	public delegate byte[] CryptographyFunction( byte[] Data, string Password );
	/// <summary>
	/// An event handler for when the program's status changes
	/// </summary>
	/// <param name="Status">The text to display</param>
	/// <param name="Progress">Progress into completing the task. -1 if indeterminate</param>
	/// <param name="Maximum">Target progress value. Can be null if indeterminate</param>
	public delegate void StatusChange( string Status, double Progress, double? Maximum );
	/// <summary>
	/// An event handler for loading a piece of media
	/// </summary>
	/// <param name="ID">The ID of the file</param>
	public delegate void MediaLoader( string ID, bool Editing );

	public static class Globals //TODO - SPLIT
	{
		/// <summary>
		/// A delegate for setting a boolean value
		/// </summary>
		/// <param name="Set">The value to set</param>
		public delegate void SetterDelegate( bool Set );
		/// <summary>
		/// An event for handling fullscreen
		/// </summary>
		public static event SetterDelegate Fullscreen;
		/// <summary>
		/// Sets the application into fullscreen if true
		/// </summary>
		/// <param name="Set">Whether to use Fullscreen or Windowed</param>
		public static void SetFullscreen(bool Set) {
			Fullscreen( Set );
        }


		/// <summary>
		/// A delegate for loading media
		/// </summary>
		/// <param name="Current">The media to load</param>
		/// <param name="Search">The search terms used</param>
		/// <param name="Index">Where in the list it is</param>
		public delegate void LoadMedia( string Current, int[] Search, int Index );
		/// <summary>
		/// An event for handling opening media
		/// </summary>
		public static event LoadMedia OnOpenMedia;
		/// <summary>
		/// Opens the specified media with any search queries and the index the media is found in said search
		/// </summary>
		/// <param name="Current">The media to load</param>
		/// <param name="Search">The search terms used</param>
		/// <param name="Index">Where in the list the media is</param>
		public static void OpenMedia(string Current, int[] Search, int Index) {
			OnOpenMedia( Current, Search, Index );
        }


		/// <summary>
		/// A dictionary of all loaded images
		/// </summary>
		public static readonly Dictionary<string, IMedia> Media = new Dictionary<string, IMedia>();

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
		/// Defines whether hidden tags should be shown
		/// </summary>
		public static bool ShowHiddenTags = false;

		/// <summary>
		/// The event for when the program's status changes
		/// </summary>
		public static event StatusChange ChangeStatus;

		/// <summary>
		/// Sets the status with an indeterminate amount of time remaining
		/// </summary>
		/// <param name="Status">The status message</param>
		public static void SetStatus(string Status) => ChangeStatus( Status, -1, null );
		/// <summary>
		/// Sets the status with progress monitored
		/// </summary>
		/// <param name="Status">The status message</param>
		/// <param name="Progress">How far into the operation</param>
		/// <param name="Maximum">The target value for Progress</param>
		public static void SetStatus( string Status, double Progress, double Maximum ) => ChangeStatus( Status, Progress, Maximum );

		/// <summary>
		/// The file path to the NanaManager directory
		/// </summary>
		public static readonly string RootPath = Path.Combine( HydroxaPath, "NanaManager" );
		/// <summary>
		/// The file location of the Content file
		/// </summary>
		public static readonly string ContentPath = Path.Combine( RootPath, "content.nana" );
		/// <summary>
		/// The file path to the Temp directory
		/// </summary>
		public static readonly string TempPath = Path.Combine( RootPath, "temp" );
		/// <summary>
		/// The file path to the Logs directory
		/// </summary>
		public static readonly string LogPath = Path.Combine( RootPath, "logs" );
		/// <summary>
		/// The directory where media exports go to
		/// </summary>
		public static readonly string ExportPath = Path.Combine( RootPath, "exports" );
		/// <summary>
		/// The file location of the latest.log file
		/// </summary>
		public static readonly string LatestLogPath = Path.Combine( LogPath, "latest.log" );
		/// <summary>
		/// The directory containing application data for all Hydroxa programs
		/// </summary>
		public static readonly string HydroxaPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), "Hydroxa" );

		public static ICryptographyProvider CryptographyProvider;

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
		public static void AddToCatagory(string Catagory, string Extension) {
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