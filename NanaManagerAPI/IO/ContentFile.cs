using System;
using System.IO;
using System.Windows;
using System.Security;
using System.IO.Compression;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using NanaManagerAPI.IO.Cryptography;
using NanaManagerAPI.Properties;
using System.Diagnostics;

[assembly: InternalsVisibleTo("NanaManager")]

namespace NanaManagerAPI.IO
{
	/// <summary>
	/// Information and handling for the user's content
	/// </summary>
	public static class ContentFile
	{
		private const int ERROR_DRIVE_MISSING = 0x001;
		private const int ERROR_INVALID_PATH = 0x002;
		private const int ERROR_GENERIC_IO = 0x003;

		/// <summary>
		/// A delegate for encrypting and decrypting data
		/// </summary>
		/// <param name="Data">The data to perform the cryptographic function on</param>
		/// <param name="Password">The password for the function</param>
		/// <returns>The data resultant of the cryptographic function</returns>
		public delegate byte[] CryptographyFunction( byte[] Data, string Password );
		public static ICryptographyProvider CryptographyProvider;

		/// <summary>
		/// The directory containing application data for all Hydroxa programs
		/// </summary>
		public static readonly string HydroxaPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), "Hydroxa" );
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
		/// The directory where plugins are found
		/// </summary>
		public static readonly string PluginPath = Path.Combine( RootPath, "plugins" );
		/// <summary>
		/// The file location of the latest.log file
		/// </summary>
		public static readonly string LatestLogPath = Path.Combine( LogPath, "latest.log" );

		/// <summary>
		/// Represents whether the Content File is open or not
		/// </summary>
		public static bool IsOpen { private set; get; }
		/// <summary>
		/// The ContentFile's Archive
		/// </summary>
		public static ZipArchive Archive;

		private static readonly byte[] ZIP_SIGNATURE = new byte[] { 80, 75, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		/// <summary>
		/// A list of all active encoders. These will be run when saving and loading data
		/// </summary>
		public static readonly List<IEncoder> ActiveEncoders = new List<IEncoder>();
		/// <summary>
		/// A helper method for ensuring the right file environment is loaded. 
		/// Can also be used for loading things before the application starts without cluttering App.xaml.cs
		/// </summary>
		internal static void LoadEnvironment() {
			try {
				if ( !Directory.Exists( HydroxaPath ) )
					Directory.CreateDirectory( HydroxaPath );
				if ( !Directory.Exists( RootPath ) )
					Directory.CreateDirectory( RootPath );
				if ( !Directory.Exists( TempPath ) )
					Directory.CreateDirectory( TempPath );
				if ( !Directory.Exists( LogPath ) )
					Directory.CreateDirectory( LogPath );
				if ( !Directory.Exists( ExportPath ) )
					Directory.CreateDirectory( ExportPath );
				if ( !Directory.Exists( PluginPath ) )
					Directory.CreateDirectory( PluginPath );
				if ( !File.Exists( ContentPath ) )
					File.WriteAllBytes( ContentPath, ZIP_SIGNATURE ); //Blank signature for a zip file
			} catch ( NotSupportedException e ) {
				MessageBox.Show( $"The expected drive was not found (Check the connection)\nMessage: {e.Message}", "Nana Manager Pre-load Error", MessageBoxButton.OK, MessageBoxImage.Error );
				Environment.Exit( ERROR_DRIVE_MISSING );
			} catch ( DirectoryNotFoundException e ) {
				MessageBox.Show( e.Message, "Nana Manager Pre-load Error", MessageBoxButton.OK, MessageBoxImage.Error );
				Environment.Exit( ERROR_INVALID_PATH );
			} catch ( IOException e ) {
				MessageBox.Show( e.Message, "Nana Manager Pre-load Error", MessageBoxButton.OK, MessageBoxImage.Error );
				Environment.Exit( ERROR_GENERIC_IO );
			}

			//Constructing the audio symbol
			UI.UI.AudioSymbol = Resources.Music_Icon.ToBitmapImage( System.Windows.Media.Imaging.BitmapCacheOption.OnLoad );
		}

		/// <summary>
		/// Sets <see cref="Archive"/> into Write Mode. (Single Use)
		/// </summary>
		/// <returns>The Archive in Write Mode</returns>
		public static ZipArchive SetArchiveWrite() {
			Archive?.Dispose(); //Write Mode is Single Use, so it must be redefined each time
			Archive = ZipFile.Open( ContentPath, ZipArchiveMode.Update );
			return Archive;
		}

		/// <summary>
		/// Sets <see cref="Archive"/> into Read Mode (Multi Use)
		/// </summary>
		/// <returns>The Archive in Read Mode</returns>
		public static ZipArchive SetArchiveRead() {
			if ( Archive == null || Archive.Mode != ZipArchiveMode.Read ) {
				Archive?.Dispose(); //Read Mode is Multi Use, so it can be kept if already defined
				Archive = ZipFile.OpenRead( ContentPath );
			}
			return Archive;
		}

		/// <summary>
		/// Checks if the content file can be read
		/// </summary>
		/// <returns>Returns true if the content file can be read. False if corrupt or encrypted</returns>
		public static bool CheckValidity() {
			try {
				if (Archive == null) {
					using var zipFile = ZipFile.OpenRead( ContentPath );
					var test = zipFile.Entries;
					return true;
				}
				var entries = Archive.Entries;
				return true;
			} catch ( InvalidDataException ) {
				return false;
			}
		}
		
		/// <summary>
		/// Reads a specified file from the content file 
		/// </summary>
		/// <param name="Name">The name of the file to read</param>
		/// <returns>A byte array representation of the data</returns>
		public static byte[] ReadFile( string Name ) {
			if ( string.IsNullOrWhiteSpace( Name ) )
				throw new ArgumentException( "File name cannot be null or whitespace", nameof( Name ) );

			SetArchiveRead();
			ZipArchiveEntry entry = Archive.GetEntry( Name );
			if ( entry == null )
				throw new FileNotFoundException( "The specified file was not found within the database", Name );
			else {
				string location = Path.Combine( TempPath, Name );
				entry.ExtractToFile( location ); //TODO - HANDLE ERRORS
				byte[] data = File.ReadAllBytes( location );
				File.Delete( location );
				return data;
			}
		}
		
		/// <summary>
		/// Writes to a file within the content file, if it exists. Creates one otherwise
		/// </summary>
		/// <param name="Name">The name of the file</param>
		/// <param name="Data">The data to write</param>
		public static void WriteFile( string Name, string Data ) {
			if ( string.IsNullOrWhiteSpace( Name ) )
				throw new ArgumentException( "File name cannot be null or whitespace", nameof( Name ) );

			if ( Exists( Name ) ) { //Write if exists
				SetArchiveWrite();
				ZipArchiveEntry entry = Archive.GetEntry( Name );
				using Stream entryStream = entry.Open();
				using StreamWriter writer = new StreamWriter( entryStream );
				writer.Write( Data );
			}
			else { //Create if doesn't exist
				//TODO - HANDLE ERRORS
				SetArchiveWrite();
				string location = Path.Combine( TempPath, Name );
				File.WriteAllText( location, Data );
				Archive.CreateEntryFromFile( location, Name );
				File.Delete( location );
			}
		}
		/// <summary>
		/// Writes to a file within the content file, if it exists. Creates one otherwise
		/// </summary>
		/// <param name="Name">The name of the file</param>
		/// <param name="Data">The data to write</param>
		public static void WriteFile( string Name, byte[] Data ) {
			if ( string.IsNullOrWhiteSpace( Name ) )
				throw new ArgumentException( "File name cannot be null or whitespace", nameof( Name ) );

			if ( Exists( Name ) ) { //Write if Exists
				ZipArchiveEntry entry = Archive.GetEntry( Name );
				using Stream entryStream = entry.Open();
				using StreamWriter writer = new StreamWriter( entryStream );
				writer.Write( Data );
			}
			else { //Create if doesn't exist
				string location = Path.Combine( TempPath, Name );
				File.WriteAllBytes( location, Data );
				Archive.CreateEntryFromFile( location, Name );
				File.Delete( location );
			}
		}

		/// <summary>
		/// Checks whether a file exists in the database
		/// </summary>
		/// <param name="Name">The name of the file to check</param>
		/// <returns>True if the file exists</returns>
		public static bool Exists( string Name ) {
			if ( string.IsNullOrWhiteSpace( Name ) )
				throw new ArgumentException( "File name cannot be null or whitespace", nameof( Name ) );
			SetArchiveRead();
			ZipArchiveEntry entry = Archive?.GetEntry( Name );
			return entry != null;
		}

		/// <summary>
		/// Invokes all encoders to save their relevant data
		/// </summary>
		public static void SaveData() {
			foreach ( IEncoder encoder in ActiveEncoders )
				encoder.SaveData();
		}
		/// <summary>
		/// Invokes all encoders to load their relevant data
		/// </summary>
		public static void LoadData() {
			foreach ( IEncoder encoder in ActiveEncoders )
				encoder.LoadData();
		}

		/// <summary>
		/// Encrypts the content file using <see cref="CryptographyProvider"/>
		/// </summary>
		/// <param name="Password">The password to encrypt the data with</param>
		public static void Encrypt( string Password ) {
			try {
				Logging.Write( "Encrypting the content file..", "ContentFile", LogLevel.Info );
				Archive?.Dispose();
				CryptographyProvider.Initialise( Password );
				File.WriteAllBytes( ContentPath, CryptographyProvider.Encrypt( File.ReadAllBytes( ContentPath ) ) );
				CryptographyProvider.Terminate();
				IsOpen = false;
			} catch ( PathTooLongException ) {
				Logging.Write( $"File Path was too long \"{ContentPath}\"", "ContentFile", LogLevel.Error );
				if ( Debugger.IsAttached )
					throw;
			} catch ( DirectoryNotFoundException ) {
				Logging.Write( $"Could not find the content file \"{ContentPath}\"", "ContentFile", LogLevel.Error );
				if ( Debugger.IsAttached )
					throw;
			} catch (ArgumentNullException) {
				FileInfo fi = new FileInfo( ContentPath );
				if ( fi.Length == 0 )
					Logging.Write( "Could not Encrypt: File was empty! This should never be the case!", "ContentFile", LogLevel.Error );
				else
					Logging.Write( "Could not Encrypt: ContentPath was null! This should never be the case!", "ContentFile", LogLevel.Error );
				if ( Debugger.IsAttached )
					throw;
			} catch ( IOException e ) {
				//TODO - HANDLE I/O ERROR
				
				Logging.Write( e, "ContentFile", LogLevel.Error );
				if ( Debugger.IsAttached )
					throw;
			} catch ( UnauthorizedAccessException e ) {
				FileInfo fi = new FileInfo( ContentPath );
				if ( fi.IsReadOnly ) //File that is readonly
					Logging.Write( "Could not Encrypt: File is Read Only", "ContentFile", LogLevel.Error );
				else if ( (fi.Attributes & FileAttributes.Hidden) > 0 ) //File that is hidden
					Logging.Write( "Coult not Encrypt: File is Hidden", "ContentFile", LogLevel.Error );
				else {
					try { //Do not have permission
						System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl( RootPath );
						Logging.Write( e, "ContentFile", LogLevel.Error ); //Shouldn't get here if expected, so just dump full error 
					} catch ( UnauthorizedAccessException ) {
						Logging.Write( "Could not Encrypt: Insufficient permissions - Need Write Permissions", "ContentFile", LogLevel.Error );
					}
				}

				if ( Debugger.IsAttached )
					throw;
			} catch ( SecurityException e ) {
				Logging.Write( $"Could not Encrypt: User has insufficient permissions (Missing permission {e.Demanded}", "ContentFile", LogLevel.Error );
				
				if ( Debugger.IsAttached )
					throw;
			}
		}
		/// <summary>
		/// Decrypts the content file using <see cref="CryptographyProvider"/>
		/// </summary>
		/// <param name="Password">The password to decrypt the data with</param>
		public static void Decrypt( string Password ) {
			try {
				if ( !CheckValidity() ) {
					CryptographyProvider.Initialise( Password );
					File.WriteAllBytes( ContentPath, CryptographyProvider.Decrypt( File.ReadAllBytes( ContentPath ) ) );
					CryptographyProvider.Terminate();
				}
				IsOpen = true;
			} catch ( ArgumentNullException ) {
				FileInfo fi = new FileInfo( ContentPath );
				if ( fi.Length == 0 )
					Logging.Write( "Could not Decrypt: File was empty! This should never be the case!", "ContentFile", LogLevel.Crash );
				else
					Logging.Write( "Could not Decrypt: ContentPath was null! This should never be the case!", "ContentFile", LogLevel.Crash );
				if ( Debugger.IsAttached )
					throw;
			} catch ( DirectoryNotFoundException ) {
				Logging.Write( "The Content Path was not found. Attempting to generate new files.", "ContentFile", LogLevel.Error );
				LoadEnvironment();
				if ( !File.Exists( ContentPath ) ) {
					Logging.Write( $"The Content Path was still not found: \"{ContentPath}\"", "ContentFile", LogLevel.Crash );
					throw;
				}
			} catch ( PathTooLongException ) {
				Logging.Write( $"Content Path was too long: \"{ContentPath}\"", "ContentFile", LogLevel.Crash );
				throw;
			} catch ( SecurityException e ) {
				Logging.Write( $"Could not Decrypt: User has insufficient permissions (Missing permission {e.Demanded}", "ContentFile", LogLevel.Crash );

				if ( Debugger.IsAttached )
					throw;
				throw;
			} catch ( IOException e ) {
				Logging.Write( e, "ContentFile", LogLevel.Crash );
				throw;
			} catch ( UnauthorizedAccessException e ) {
				if ( Directory.Exists( ContentPath ) )
					Logging.Write( $"Content Path was a directory: \"{ContentPath}\"", "ContentFile", LogLevel.Crash );
				else {
					FileInfo fi = new FileInfo( ContentPath );

					if ( fi.IsReadOnly ) //File that is readonly
						Logging.Write( $"Content Path was read only: \"{ContentPath}\"", "ContentFile", LogLevel.Crash );
					else if ( (fi.Attributes & FileAttributes.Hidden) > 0 ) //File that is hidden
						Logging.Write( "Coult not Decrypt: File is Hidden", "ContentFile", LogLevel.Crash );
					else {
						try { //Do not have permission
							System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl( RootPath );
							Logging.Write( e, "ContentFile", LogLevel.Error ); //Shouldn't get here if expected, so just dump full error 
						} catch ( UnauthorizedAccessException ) {
							Logging.Write( "Could not Decrypt: Insufficient permissions - Need Write Permissions", "ContentFile", LogLevel.Crash );
						}
					}
				}
				if ( Debugger.IsAttached )
					throw;
			}
		}
	}
}