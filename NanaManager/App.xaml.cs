using System;
using System.IO;
using System.Windows;

using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI;
using NanaManagerAPI.Types;
using System.Diagnostics;

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

			ContentFile.LoadEnvironment();
			Logging.Init();
			Logging.Write( "Loading API components", "Init", LogLevel.Info );

			Logging.Write( "Registering Data Encoders", "Init", LogLevel.Info );
			ContentFile.ActiveEncoders.Add( new FileEncoders.BaseEncoder1_1() );

			Logging.Write( "Registering Media Constructors", "Init", LogLevel.Info );
			Registry.RegisterMediaConstructor( typeof( Image ), Image.CTOR_ID );

			Logging.Write( "Registering Media Viewers", "Init", LogLevel.Info );
			MediaHandlers.Images imhnd = new MediaHandlers.Images();
			Registry.RegisterMediaViewer( imhnd.ID, imhnd );
			Registry.RegisterExtensions( "Image Files", Image.CTOR_ID, imhnd.ID, imhnd.GetCompatibleTypes() );

			Logging.Write( "Registering Cryptography Providers", "Init", LogLevel.Info );
			ContentFile.CryptographyProvider = new Cryptography.Cryptography();

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

			//try {
				App app = new App();
				MainWindow wnd = new MainWindow( instruction ); //Load and run the application
				app.Run( wnd );

				//This part executes after the application closes

				string[] logs = Directory.GetFiles( ContentFile.LogPath );
				if ( logs.Length > 5 ) {
					for ( int i = 0; i < logs.Length - 5; i++ )
						File.Delete( logs[i] );
				}
			//} catch ( Exception e ) {
			//	Logging.Write( e, "Core", LogLevel.Fatal );
			//	Logging.SaveLogs();
			//	string[] logs = Directory.GetFiles( ContentFile.LogPath );
			//	if ( logs.Length > 5 ) {
			//		for ( int i = 0; i < logs.Length - 5; i++ )
			//			File.Delete( logs[i] );
			//	}
			//	if (Debugger.IsAttached)
			//		throw;
			//}
		}
	}
}