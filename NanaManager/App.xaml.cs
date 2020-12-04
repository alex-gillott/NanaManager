using NanaManagerAPI;
using NanaManagerAPI.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace NanaManager
{
    /// <summary>
    /// Application entry point
    /// </summary>
    public partial class App : Application
    {
        private App() {
            InitializeComponent(); //Initialises all global XAML components (App.xaml)
        }

#pragma warning disable IDE1006 //Block Main() from creating naming alerts

        [STAThread] //Allows WPF to operate from this method
        [LoaderOptimization( LoaderOptimization.MultiDomain )] //Tells the compiler to optimise the method for multi-domain operation. This improves the performance of plugins
        private static void Main() {
#pragma warning restore IDE1006 //Block Main() from creating naming alerts

            ContentFile.LoadEnvironment(); //Ensures the directories and files are in place, which are required to run NanaManager
            Logging.Init(); //Initialises the logger

            NanaManager.Properties.Settings.Default.ToImport = new System.Collections.Specialized.StringCollection(); //Depricated
            
            void SaveLogs() { //Creating a local method saves on operating stack resources, and moves it to the static heap
                Logging.SaveLogs(); //Saves the logs to latest.log
                FileInfo[] logs = new DirectoryInfo( ContentFile.LogPath ).GetFiles.OrderBy( p => p.CreationTimeUtc ).ToArray(); //Gets all log files in creation order
                if ( logs.Length > 5 ) 
                    for ( int i = 0; i < logs.Length - 5; i++ )//If there are more than 5 logs, delete all logs excluding the most recent 5
                        logs[i].Delete();
            }
            
            try {
                App app = new App(); //Creates a new Application instance for COM to handle
                Splash wnd = new Splash(); //Create the splash screen
                app.Run( wnd ); //Loads the splash screen into the application, using it as the entry point

                //This part executes after the application closes
                //..
            } catch ( Exception e ) {
                Logging.Write( e, "Core", LogLevel.Crash ); //Write the error to the logs as a crash
                if ( Debugger.IsAttached ) { //If I am debugging the application, rethrow the error
                    SaveLogs();
                    ContentFile.Archive?.Dispose(); //Release the memory and file streams consumed by the Archive, if the Archive has been instanced
                    throw;
                }
            } finally { //Runs Once the application is complete. Will not run if the debugger is active, which allows for easier debugging to occur
                SaveLogs();
                ContentFile.Archive?.Dispose(); //Release the memory and file streams consumed by the Archive, if the Archive has been instanced
            }
        }
    }
}
