using System;
using System.IO;
using System.Windows;
using System.Security;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using NanaManagerAPI.IO.Cryptography;
using NanaManagerAPI.Properties;
using System.Drawing.Imaging;

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
		/// The file location of the latest.log file
		/// </summary>
		public static readonly string LatestLogPath = Path.Combine( LogPath, "latest.log" );

		private static readonly byte[] ZIP_SIGNATURE = new byte[] { 80, 75, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		/// <summary>
		/// A list of all active encoders. These will be run when saving and loading data
		/// </summary>
		public static readonly List<IEncoder> ActiveEncoders = new List<IEncoder>();
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

            using MemoryStream ms = new MemoryStream();
            Resources.Music_Icon.Save( ms, ImageFormat.Png );
			ms.Position = 0;
            UI.UI.AudioSymbol = new System.Windows.Media.Imaging.BitmapImage();
            UI.UI.AudioSymbol.BeginInit();
            UI.UI.AudioSymbol.StreamSource = ms;
            UI.UI.AudioSymbol.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            UI.UI.AudioSymbol.EndInit();
            UI.UI.AudioSymbol.Freeze();
        }

		/// <summary>
		/// Checks if the content file can be read
		/// </summary>
		/// <returns>Returns true if the content file can be read. False if corrupt or encrypted</returns>
		public static bool CheckValidity() {
			try {
				return Ionic.Zip.ZipFile.CheckZip( ContentPath );
			} catch ( Exception e ) {
				Logging.Write( e, "Content", LogLevel.Error );
				throw e;
			}
		}
		/// <summary>
		/// Reads a file from the content file 
		/// </summary>
		/// <param name="Name">The name of the file to read</param>
		/// <returns>A byte array representation of the data</returns>
		public static byte[] ReadFile( string Name ) {
			if ( string.IsNullOrWhiteSpace( Name ) )
				throw new ArgumentException( "File name cannot be null or whitespace", nameof( Name ) );

			using ZipArchive archive = ZipFile.OpenRead( ContentPath );

			ZipArchiveEntry entry = archive.GetEntry( Name );
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

			if ( Exists( Name ) )
				using ( ZipArchive archive = ZipFile.Open( ContentPath, ZipArchiveMode.Update ) ) {
					ZipArchiveEntry entry = archive.GetEntry( Name );
					using Stream entryStream = entry.Open();
					using StreamWriter writer = new StreamWriter( entryStream );
					writer.Write( Data );
				}
			else
				using ( ZipArchive archive = ZipFile.Open( ContentPath, ZipArchiveMode.Update ) ) {
					//TODO - HANDLE ERRORS
					string location = Path.Combine( TempPath, Name );
					File.WriteAllText( location, Data );
					archive.CreateEntryFromFile( location, Name );
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

			if ( Exists( Name ) )
				using ( ZipArchive archive = ZipFile.Open( ContentPath, ZipArchiveMode.Update ) ) {
					ZipArchiveEntry entry = archive.GetEntry( Name );
					using Stream entryStream = entry.Open();
					using StreamWriter writer = new StreamWriter( entryStream );
					writer.Write( Data );
				}
			else
				using ( ZipArchive archive = ZipFile.Open( ContentPath, ZipArchiveMode.Update ) ) {
					string location = Path.Combine( TempPath, Name );
					File.WriteAllBytes( location, Data );
					archive.CreateEntryFromFile( location, Name );
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
			using ZipArchive archive = ZipFile.OpenRead( ContentPath );
			ZipArchiveEntry entry = archive.GetEntry( Name );
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
				CryptographyProvider.Initialise( Password );
				File.WriteAllBytes( ContentPath, CryptographyProvider.Encrypt( File.ReadAllBytes( ContentPath ) ) );
				CryptographyProvider.Terminate();
			} catch ( IOException e ) {
				//TODO - HANDLE I/O ERROR
				throw e;
			} catch ( UnauthorizedAccessException e ) {
				//TODO - HANDLE ERROR
				//File that is readonly
				//File that is hidden
				//Do not have permission
				throw e;
			} catch ( SecurityException e ) {
				//TODO - HANDLE ACCESS ERROR
				throw e;
			}
		}
		/// <summary>
		/// Decrypts the content file using <see cref="CryptographyProvider"/>
		/// </summary>
		/// <param name="Password">The password to decrypt the data with</param>
		public static void Decrypt( string Password ) {
			try {
				CryptographyProvider.Initialise( Password );
				File.WriteAllBytes( ContentPath, CryptographyProvider.Decrypt( File.ReadAllBytes( ContentPath ) ) );
				CryptographyProvider.Terminate();
			} catch ( ArgumentNullException e ) {
				//Path is null or byte array was empty
				Logging.Write( $"Attempted to write an empty array to the file\nStack Trace:\n\t{e.StackTrace}", "ContentDecryption", LogLevel.Fatal );
				throw e;
			} catch ( DirectoryNotFoundException e ) {
				Logging.Write( "The Content Path was not found. Attempting to generate new files.", "ContentDecryption", LogLevel.Error );
				LoadEnvironment();
				if ( !File.Exists( ContentPath ) )
					throw e;
			} catch ( PathTooLongException e ) {
				//Path was too long
				Logging.Write( $"Content Path was too long: \"{ContentPath}\"", "ContentDecrpytion", LogLevel.Fatal );
				throw e;
			} catch ( SecurityException e ) {
				//TODO - Does not have required permissions

				throw e;
			} catch ( IOException e ) {
				Logging.Write( $"Could not decrypt file ({e.Message})\nStack Trace:\n\t{e.StackTrace}", "ContentDecryption", LogLevel.Fatal );
				throw e;
			} catch ( UnauthorizedAccessException e ) {
				//TODO - HANDLE ERROR
				if ( Directory.Exists( ContentPath ) )
					Logging.Write( $"Content Path was a directory: \"{ContentPath}\"", "ContentDecryption", LogLevel.Fatal );
				else {
					FileInfo fi = new FileInfo( ContentPath );

					if ( fi.IsReadOnly )
						Logging.Write( $"Content Path was read only: \"{ContentPath}\"", "ContentDecryption", LogLevel.Fatal );
					//File that is readonly
					//File that is hidden
					//Do not have permission
				}
				throw e;
			}
		}
	}
}