using System;
using System.IO;
using System.Windows;

using NanaManagerAPI.IO;
using NanaManagerAPI;

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
		static void Main( string[] args ) {
#pragma warning restore IDE1006 // Naming Styles

			//MessageBox.Show( typeof( App ).Assembly.GetName().Name );

			ContentFile.LoadEnvironment();
			Logging.Init();
			Logging.Write( "Loading API components", "Init", LogLevel.Info );
			Logging.Write( "Registering Data Encoders", "Init" );
			ContentFile.ActiveEncoders.Add( new FileEncoders.BaseEncoder1_1() );
			Logging.Write( "Registering Media Viewers", "Init" );
			Globals.RegisterMediaViewer( new MediaHandlers.Images() );
			Logging.Write( "Registering Cryptography Providers", "Init" );
			Globals.CryptographyProvider = new Cryptography.Cryptography();

			if ( NanaManager.Properties.Settings.Default.ToImport == null ) { //Instantiate Import Collection if non-existent
				NanaManager.Properties.Settings.Default.ToImport = new System.Collections.Specialized.StringCollection();
				NanaManager.Properties.Settings.Default.Save();
			}
			if ( NanaManager.Properties.Settings.Default.ticked == null ) {
				NanaManager.Properties.Settings.Default.ticked = new System.Collections.Specialized.StringCollection();
				NanaManager.Properties.Settings.Default.Save();
			}

			int instruction = 0;
			if ( args.Length > 0 ) {
				switch ( args[0] ) {
					case "import":
						if ( File.Exists( args[1] ) || Directory.Exists( args[1] ) ) //If importing from shell, add to list
						{
							NanaManager.Properties.Settings.Default.ToImport.Add( args[1] );
							NanaManager.Properties.Settings.Default.Save();
						}
						if ( args[2] == "open" ) //If opening from shell, set the instruction as such
						{
							instruction = 0x1;
							break;
						}
						return;
				}
			}

			App app = new App();
			MainWindow wnd = new MainWindow( instruction ); //Load and run the application
			app.Run( wnd );

			string[] logs = Directory.GetFiles( Globals.LogPath );
			if ( logs.Length > 5 ) {
				for ( int i = logs.Length - 1; i > 4; i-- )
					File.Delete( logs[i] );
			}
			//This part executes after the application closes
		}
	}
}