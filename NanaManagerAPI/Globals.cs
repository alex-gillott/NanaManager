using System;
using System.IO;
using System.Collections.Generic;

using NanaManagerAPI.IO.Cryptography;
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

		public static ICryptographyProvider CryptographyProvider;
	}
}