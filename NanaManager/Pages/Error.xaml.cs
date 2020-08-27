using System.Windows;
using System.Windows.Controls;
using NanaManagerAPI;
using NanaManagerAPI.UI;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Error.xaml
    /// </summary>
    public partial class Error : Page
    {
        private bool crash = false;

        private const string RETURN_MSG = @"Unfortunately, a part of the program messed up and couldn't continue working. 
This is fine, however! Simply press the Return button to return to the Viewer.

The error was saved to the logs. 
Please report the issue to the plugin developer or Nana Manager developers, as we would greatly appreciate it!";
        private const string FATAL_MSG = @"Unfortuantely, the viewer messed up and couldn't continue working. 
This is fine, however! Your stuff is safe, and will still be there when you log back in. However, we must restart the application.

The error was saved to the logs. 
It's important that you report the issue to the plugin developer or Nana Manager developers, 
so that this kind of error may be fixed!";
        public Error() {
            InitializeComponent();
            txtMsg.Text = string.Format( txtMsg.Text, System.IO.Directory.GetCurrentDirectory() );
            Logging.NewMessageAdvanced += logging_NewMessage;
        }

        private void logging_NewMessage( string message, string location, LogLevel level, System.DateTime time, System.Exception @internal, int? logCode ) {
            if ( level == LogLevel.Fatal ) {
                lblLocation.Content = $"What part went wrong: {location}";
                crash = false;
                txtMsg.Text = RETURN_MSG;

                Paging.LoadPage( Pages.Error );
            }
            else if ( level == LogLevel.Crash ) {
                lblLocation.Content = $"What part went wrong: {location}";
                crash = true;
                txtMsg.Text = FATAL_MSG;

                Paging.LoadPage( Pages.Error );
            }
        }

        private void button_Click( object sender, RoutedEventArgs e ) {
            if ( crash )
                UI.CloseApplication();
            else {
                UI.SetFullscreen( false );
                Paging.LoadPage( Pages.Viewer );
            }
        }

        private void button_Click_1( object sender, RoutedEventArgs e ) {
            MessageBox.Show( "Error reporter is not yet implemented! Please make a post on the Nana Manager Github" );
        }

        private void button_Click_2( object sender, RoutedEventArgs e ) {
            MessageBox.Show( "Internal log viewer not yet implemented! Please download the log viewer plugin from the Github" );
        }
    }
}
