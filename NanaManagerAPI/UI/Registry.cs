using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NanaManagerAPI.Media;

namespace NanaManagerAPI.UI
{
    /// <summary>
    /// Handles registering custom components, such as <see cref="IMediaViewer"/>s
    /// </summary>
    public static class Registry
    {
		private static readonly Type[] MediaConstructorFormat = { typeof(string), typeof(int[]), typeof(string) };

		/// <summary>
		/// A dictionary of all data types currently supported with the ID related to the viewer that adds them
		/// </summary>
		public readonly static Dictionary<string, string> SupportedDataTypes = new Dictionary<string, string>();
		/// <summary>
		/// A list of all viewers currently loaded
		/// </summary>
		public readonly static Dictionary<string, IMediaViewer> Viewers = new Dictionary<string, IMediaViewer>();
		/// <summary>
		/// A dictionary of all file extensions and locations of the constructors that build them
		/// </summary>
		public readonly static Dictionary<string, string> ExtensionConstructors = new Dictionary<string, string>();
		/// <summary>
		/// A list of all constructors used to construct media types
		/// </summary>
		public readonly static Dictionary<string, ConstructorInfo> MediaConstructors = new Dictionary<string, ConstructorInfo>();
		/// <summary>
		/// A dictionary of all file categories. Used for the Open File Dialog
		/// </summary>
		public readonly static Dictionary<string, List<string>> Categories = new Dictionary<string, List<string>>();
		/// <summary>
		/// A dictionary with all of the settings tabs. Used for the settings page
		/// </summary>
		internal readonly static Dictionary<string, SettingsTab> SettingsTabs = new Dictionary<string, SettingsTab>();

		/// <summary>
		/// Returns true if the provided extension has a constructor
		/// </summary>
		/// <param name="Extension">The extention to check</param>
		public static bool ExtensionIsRegistered( string Extension ) => ExtensionConstructors.ContainsKey( Extension );
		/// <summary>
		/// Returns true if the Media Constructor exists
		/// </summary>
		/// <param name="ID">The ID of the Media Constructor</param>
		/// <returns></returns>
		public static bool MediaConstructorExists( string ID ) => MediaConstructors.ContainsKey( ID );
		/// <summary>
		/// Returns true if the Media Viewer exists
		/// </summary>
		/// <param name="ID">The ID of the viewer</param>
		/// <returns></returns>
		public static bool MediaViewerExists( string ID ) => Viewers.ContainsKey( ID );

		/// <summary>
		/// Registers the class as a media constructor
		/// </summary>
		/// <param name="MediaType">The <see cref="Type"/> to get the constructor from</param>
		public static void RegisterMediaConstructor( Type MediaType, string ID ) {
			if ( MediaType.GetInterface( nameof( IMedia ) ) == null )
				throw new ArgumentException( $"The provided class did not inherit the {nameof( IMedia )} interface", nameof( MediaType ) );

			ConstructorInfo ctor = MediaType.GetConstructor(MediaConstructorFormat);
			if ( ctor == null )
				throw new ArgumentException( $"The provided class did not contain a constructor with the parameters of {typeof(string).Name}, {typeof(int[]).Name} and {typeof(string).Name}", nameof( MediaType ) );
			MediaConstructors.Add( ID, ctor );
			Logging.Write( $"Registered Constructor \"{ID}\"", "Registry" );
		}

		/// <summary>
		/// Registers a collection of extensions as compatible
		/// </summary>
		/// <param name="Category">The category the extensions fit under. Will be created if non-existant</param>
		/// <param name="ConstructorID">The ID of the constructor to be used for the extensions</param>
		/// <param name="ViewerID">The viewer that handles the extensions</param>
		/// <param name="Extensions">The collection of extensions to register</param>
		public static void RegisterExtensions( string Category, string ConstructorID, string ViewerID, params string[] Extensions ) {
			try {
				foreach ( string ext in Extensions )
					RegisterExtension( ext, Category, ConstructorID, ViewerID );
			} catch ( Exception ) {
				throw;
			}
        }

		/// <summary>
		/// Registers a file extension as compatible
		/// </summary>
		/// <param name="Extension">The file extension to register</param>
		/// <param name="Category">The category the extension fits under. Will be created if non-existent</param>
		/// <param name="ConstructorID">The ID of the constructor that is used for the extension</param>
		/// <param name="ViewerID">The Viewer that handles the file extension</param>
		public static void RegisterExtension( string Extension, string Category, string ConstructorID, string ViewerID ) {
			if ( SupportedDataTypes.ContainsKey( Extension ) )
				throw new ArgumentException( "The extension was already registered", nameof( Extension ) );
			else if ( string.IsNullOrWhiteSpace( Extension ) )
				throw new ArgumentNullException( "The extension provided had no valid content", nameof( Extension ) );
			else if ( !MediaConstructorExists( ConstructorID ) )
				throw new ArgumentOutOfRangeException( nameof( ConstructorID ), "The Constructor provided did not exist" );
			else if ( string.IsNullOrWhiteSpace( ViewerID ) )
				throw new ArgumentNullException( nameof( ViewerID ), "The provided Viewer was null" );
			else if ( !MediaViewerExists( ViewerID ) )
				throw new ArgumentOutOfRangeException( nameof( ViewerID ), "The Viewer provided did not exist" );

			if ( !Categories.ContainsKey( Category ) )
				Categories.Add( Category, new List<string>() { Extension } );
			else
				Categories[Category].Add(Extension);

			ExtensionConstructors.Add( Extension, ConstructorID );
			SupportedDataTypes.Add( Extension, ViewerID );
			Logging.Write( $"Registered Extension \"{Extension}\"", "Registry" );
		}

		/// <summary>
		/// Adds the settings tab to the dictionary
		/// </summary>
		/// <param name="Tab">The <see cref="SettingsTab"/> to add</param>
		public static void RegisterSettings(SettingsTab Tab) {
			SettingsTabs.Add( Tab.ID, Tab );
			Logging.Write( $"Registered Setting \"{Tab.ID}\"", "Registry" );
        }

		/// <summary>
		/// Registers a class to handle its specified file types
		/// </summary>
		/// <param name="Viewer">The class to act as the handler</param>
		/// <param name="ID">The ID of the viewer</param>
		public static void RegisterMediaViewer( string ID, IMediaViewer Viewer ) {
			if ( string.IsNullOrWhiteSpace( ID ) )
				throw new ArgumentNullException( nameof( ID ), "The ID had no valid content" );
			try {
				Viewers.Add( ID, Viewer );
				Logging.Write( $"Registered Media Viewer \"{ID}\"", "Registry" );
			} catch ( Exception e ) {
				Logging.Write( e, "Registry" );
				throw;
			}
		}

		/// <summary>
		/// A shortcut to getting the media constructor for a specified file extension
		/// </summary>
		/// <param name="FileExtension">The file extension that the constructor handles</param>
		/// <returns>The constructor that handles the specified file extension. Returns null if the file extension is not supported</returns>
		public static ConstructorInfo GetConstructor( string FileExtension ) => ExtensionConstructors.ContainsKey(FileExtension) ? MediaConstructors[ExtensionConstructors[FileExtension]] : null;
		/// <summary>
		/// A shortcut to getting the media viewer for a specified file extension
		/// </summary>
		/// <param name="FileExtension">The file extension that the media viewer handles</param>
		/// <returns>The viewer for the specified file extension. Returns null if the file extension is not supported</returns>
		public static IMediaViewer GetViewer( string FileExtension ) => SupportedDataTypes.ContainsKey( FileExtension ) ? Viewers[SupportedDataTypes[FileExtension]] : null;
	}
}
