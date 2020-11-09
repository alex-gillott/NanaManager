using System;
using System.IO;
using System.Windows;
using System.Diagnostics;

using NanaManagerAPI.Types;
using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI;
using System.Linq;

namespace NanaManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		App() {
			InitializeComponent();
		}

#pragma warning disable IDE1006 // Naming Styles
		[STAThread]
		[LoaderOptimization( LoaderOptimization.MultiDomain )]
		static void Main( string[] args ) {
#pragma warning restore IDE1006 // Naming Styles

			ContentFile.LoadEnvironment();
			Logging.Init();

			NanaManager.Properties.Settings.Default.ToImport = new System.Collections.Specialized.StringCollection();

			try {
				App app = new App();
				Splash wnd = new Splash(); //Load and run the application
				app.Run( wnd );

				//This part executes after the application closes
			} catch ( Exception e ) {
				Logging.Write( e, "Core", LogLevel.Crash );
				try {
					ContentFile.SetArchiveWrite();
					ContentFile.SaveData();
				} catch ( Exception er ) {
					Logging.Write( "Could not save Data", "ErrorRecovery", LogLevel.Fatal );
					Logging.Write( er, "ErrorRecovery", LogLevel.Fatal );

					if ( Debugger.IsAttached )
						throw;
				}
				Logging.SaveLogs();

				MessageBox.Show( "An error occured in the application that prevented it from running. As much data was saved as possible, and the error was logged" );

				if ( Debugger.IsAttached )
					throw;
			} finally {
				string[] logs = Directory.GetFiles( ContentFile.LogPath, "*.*", SearchOption.TopDirectoryOnly );
				if ( logs.Length > 5 ) {
					foreach ( FileInfo fi in new DirectoryInfo( ContentFile.LogPath ).GetFiles().OrderByDescending( x => x.LastWriteTime ).Skip( 5 ) )
						fi.Delete();
				}
				ContentFile.Archive?.Dispose();
			}
		}
	}
}