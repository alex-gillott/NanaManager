using System;
using System.IO;
using System.Security;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NanaManager")]

namespace NanaManagerAPI.IO
{
	public static class ContentFile
	{
		private static readonly byte[] ZIP_SIGNATURE = new byte[] { 80, 75, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		/// <summary>
		/// A list of all active encoders. These will be run when saving and loading data
		/// </summary>
		public static readonly List<IEncoder> ActiveEncoders = new List<IEncoder>();
		internal static void LoadEnvironment() {
			if ( !Directory.Exists( Globals.HydroxaPath ) )
				Directory.CreateDirectory( Globals.HydroxaPath );
			if ( !Directory.Exists( Globals.RootPath ) )
				Directory.CreateDirectory( Globals.RootPath );
			if ( !Directory.Exists( Globals.TempPath ) )
				Directory.CreateDirectory( Globals.TempPath );
			if ( !Directory.Exists( Globals.LogPath ) )
				Directory.CreateDirectory( Globals.LogPath );
			if ( !Directory.Exists( Globals.ExportPath ) )
				Directory.CreateDirectory( Globals.ExportPath );
			if ( !File.Exists( Globals.ContentPath ) )
				File.WriteAllBytes( Globals.ContentPath, ZIP_SIGNATURE ); //Blank signature for a zip file
		}

		/// <summary>
		/// Checks if the content file can be read
		/// </summary>
		/// <returns>Returns true if the content file can be read. False if corrupt or encrypted</returns>
		public static bool CheckValidity() {
			try {
				return Ionic.Zip.ZipFile.CheckZip( Globals.ContentPath );
            } catch ( Exception e ) {
				Logging.Write( e, "Content" );
				return false;
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

			using ZipArchive archive = ZipFile.OpenRead( Globals.ContentPath );

			ZipArchiveEntry entry = archive.GetEntry( Name );
			if ( entry == null )
				throw new FileNotFoundException( "The specified file was not found within the database", Name );
			else {
				string location = Path.Combine( Globals.TempPath, Name );
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
				using ( ZipArchive archive = ZipFile.Open( Globals.ContentPath, ZipArchiveMode.Update ) ) {
					ZipArchiveEntry entry = archive.GetEntry( Name );
					using Stream entryStream = entry.Open();
					using StreamWriter writer = new StreamWriter( entryStream );
					writer.Write( Data );
				}
			else
				using ( ZipArchive archive = ZipFile.Open( Globals.ContentPath, ZipArchiveMode.Update ) ) {
					//TODO - HANDLE ERRORS
					string location = Path.Combine( Globals.TempPath, Name );
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
				using ( ZipArchive archive = ZipFile.Open( Globals.ContentPath, ZipArchiveMode.Update ) ) {
					ZipArchiveEntry entry = archive.GetEntry( Name );
					using Stream entryStream = entry.Open();
					using StreamWriter writer = new StreamWriter( entryStream );
					writer.Write( Data );
				}
			else
				using ( ZipArchive archive = ZipFile.Open( Globals.ContentPath, ZipArchiveMode.Update ) ) {
					string location = Path.Combine( Globals.TempPath, Name );
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
			using ZipArchive archive = ZipFile.OpenRead( Globals.ContentPath );
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
		/// Encrypts the content file using <see cref="EncryptionFunction"/>
		/// </summary>
		/// <param name="Password">The password to encrypt the data with</param>
		public static void Encrypt( string Password ) {
			try {
				Globals.CryptographyProvider.Initialise( Password );
				File.WriteAllBytes( Globals.ContentPath, Globals.CryptographyProvider.Encrypt( File.ReadAllBytes( Globals.ContentPath ) ) );
				Globals.CryptographyProvider.Terminate();
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
		/// Decrypts the content file using <see cref="DecryptionFunction"/>
		/// </summary>
		/// <param name="Password">The password to decrypt the data with</param>
		public static void Decrypt( string Password ) {
			try {
				Globals.CryptographyProvider.Initialise( Password );
				File.WriteAllBytes( Globals.ContentPath, Globals.CryptographyProvider.Decrypt( File.ReadAllBytes( Globals.ContentPath ) ) );
				Globals.CryptographyProvider.Terminate();
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
	}
}