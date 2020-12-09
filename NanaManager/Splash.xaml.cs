using NanaManagerAPI;
using NanaManagerAPI.IO;
using NanaManagerAPI.Types;
using NanaManagerAPI.UI;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        private delegate void handleProgUpdate( int Val );

        private event handleProgUpdate ProgUpdate; //Handling the progress bar

        private Thread loadThread;

        public Splash() {
            InitializeComponent();
            ProgUpdate += onProgUpdate;
        }

        private void onProgUpdate( int val ) {
            Dispatcher.Invoke( () => pgbProgress.Value = val ); //Update the progress bar
        }

        private void loadInternalComponents() { //TODO - Work out tasks to make this Async properly
            try {
                ProgUpdate( 1 );
                Logging.Write( "Loading API components", "Init", LogLevel.Info );

                Logging.Write( "Registering Data Encoders", "Init", LogLevel.Info );
                ContentFile.ActiveEncoders.Add( new FileEncoders.BaseEncoder1_1() ); //Registers the basic encoder for nanaData
                ProgUpdate( 2 );

                Logging.Write( "Registering Media Constructors", "Init", LogLevel.Info );
                Registry.RegisterMediaConstructor( typeof( Image ), Image.CTOR_ID );
                Registry.RegisterMediaConstructor( typeof( Audio ), Audio.CTOR_ID );
                Registry.RegisterMediaConstructor( typeof( Video ), Video.CTOR_ID ); //Registers the data types
                ProgUpdate( 3 );

                Logging.Write( "Registering Media Viewers", "Init", LogLevel.Info );
                MediaHandlers.Images imhnd = new MediaHandlers.Images();
                Registry.RegisterMediaViewer( imhnd.ID, imhnd );
                Registry.RegisterExtensions( "Image Files", Image.CTOR_ID, imhnd.ID, imhnd.GetCompatibleTypes() );
                MediaHandlers.Audio auhnd = new MediaHandlers.Audio();
                Registry.RegisterMediaViewer( auhnd.ID, auhnd );
                Registry.RegisterExtensions( "Audio Files", Audio.CTOR_ID, auhnd.ID, auhnd.GetCompatibleTypes() );
                MediaHandlers.Video vihnd = new MediaHandlers.Video();
                Registry.RegisterMediaViewer( vihnd.ID, vihnd );
                Registry.RegisterExtensions( "Video Files", Video.CTOR_ID, vihnd.ID, vihnd.GetCompatibleTypes() ); // Registering the valid file extensions
                ProgUpdate( 4 );

                Logging.Write( "Registering Cryptography Providers", "Init", LogLevel.Info );
                ContentFile.CryptographyProvider = new Cryptography.Cryptography(); //Registering the default cryptography provider (Unencrypted)
                ProgUpdate( 5 );
                
                try {
                    Logging.Write( "Initialising Plugins", "Init", LogLevel.Info );
                    Dispatcher.Invoke( () => pgbProgress.IsIndeterminate = true );
                    Plugins.LoadPlugins();
                } catch ( Exception ex ) {
                    Logging.Write( ex, "Plugins", LogLevel.Error );
                    if ( Debugger.IsAttached )
                        throw;
                }
            } catch ( Exception ex ) {
                Logging.Write( ex, "Preloading", LogLevel.Crash ); //Handle a load error and crash the application
                if ( Debugger.IsAttached )
                    throw;
            }
            Dispatcher.Invoke( () =>
             {
                 MainWindow main = new MainWindow();
                 Application.Current.MainWindow = main;
                 Close(); //Close and dispose the splash window, as we no longer need it
                 main.Show(); //Instance the application window and open it
             } );
        }

        private void window_ContentRendered( object sender, EventArgs e ) {
            pgbProgress.IsIndeterminate = false; //On load, set up the custom values and load the thread
            pgbProgress.Maximum = 5;
            pgbProgress.Value = 0;

            loadThread = new Thread( loadInternalComponents );
            loadThread.Start();
        }
    }
}
