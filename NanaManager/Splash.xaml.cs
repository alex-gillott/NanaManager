using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using NanaManagerAPI.Types;
using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI;
using System.Threading;
using NanaManager.SettingsPages;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
		private delegate void handleProgUpdate(int Val);
		private event handleProgUpdate ProgUpdate;

		private Thread loadThread;

        public Splash() {
            InitializeComponent();
			ProgUpdate += onProgUpdate;
        }

		private void onProgUpdate(int val) {
			Dispatcher.Invoke( () => pgbProgress.Value = val );
        }

		private void loadInternalComponents() { //TODO - Work out tasks to make this Async properly
			ProgUpdate( 1 );
			Logging.Write( "Loading API components", "Init", LogLevel.Info );

			Logging.Write( "Registering Data Encoders", "Init", LogLevel.Info );
			ContentFile.ActiveEncoders.Add( new FileEncoders.BaseEncoder1_1() );
			ProgUpdate( 2 );

			Logging.Write( "Registering Media Constructors", "Init", LogLevel.Info );
			Registry.RegisterMediaConstructor( typeof( Image ), Image.CTOR_ID );
			Registry.RegisterMediaConstructor( typeof( Audio ), Audio.CTOR_ID );
			ProgUpdate( 3 );

			Logging.Write( "Registering Media Viewers", "Init", LogLevel.Info );
			MediaHandlers.Images imhnd = new MediaHandlers.Images();
			Registry.RegisterMediaViewer( imhnd.ID, imhnd );
			Registry.RegisterExtensions( "Image Files", Image.CTOR_ID, imhnd.ID, imhnd.GetCompatibleTypes() );
			MediaHandlers.Audio auhnd = new MediaHandlers.Audio();
			Registry.RegisterMediaViewer( auhnd.ID, auhnd );
			Registry.RegisterExtensions( "Audio Files", Audio.CTOR_ID, auhnd.ID, auhnd.GetCompatibleTypes() );
			ProgUpdate( 4 );

			Logging.Write( "Registering Cryptography Providers", "Init", LogLevel.Info );
			ContentFile.CryptographyProvider = new Cryptography.Cryptography();
			ProgUpdate( 5 );

			Logging.Write( "Initialising Plugins", "Init", LogLevel.Info );
			Dispatcher.Invoke(() => pgbProgress.IsIndeterminate = true );
			Plugins.LoadPlugins();
			Dispatcher.Invoke( () =>
			 {
				 MainWindow main = new MainWindow();
				 Application.Current.MainWindow = main;
				 Close();
				 main.Show();
			 } );
		}

        private void window_ContentRendered( object sender, EventArgs e ) {
			pgbProgress.IsIndeterminate = false;
			pgbProgress.Maximum = 5;
			pgbProgress.Value = 0;

			loadThread = new Thread( loadInternalComponents );
			loadThread.Start();
        }
    }
}
