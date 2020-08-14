using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Threading;

using NanaManagerAPI;
using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI.Threading;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Page
    {
        public static bool Complete;

        private readonly BackgroundWorker bgWork = new BackgroundWorker();
        private Thread waitThread;
        public Welcome() {
            InitializeComponent();
            UI.StatusChanged += onStatusChange;
        }

        private void onStatusChange( string status, double progress, double? maximum ) {
            Dispatcher.Invoke( new Action( () =>
            {
                lblStatus.Content = status;
                if ( progress == -1 )
                    pgProgress.IsIndeterminate = true;
                else {
                    pgProgress.Maximum = (int)maximum;
                    pgProgress.Value = progress;
                }
            } ) );
        }

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            if ( Data.Groups.Count == 0 ) {
                pgProgress.Visibility = lblStatus.Visibility = Visibility.Visible;
                waitThread = new Thread( () => Thread.Sleep( 2000 ) );
                bgWork.DelegateThread( () =>
                {
                    if ( !ContentFile.CheckValidity() )
                        ContentFile.Decrypt( Login.Password );

                    ContentFile.LoadData();
                    Dispatcher.Invoke( () => { pgProgress.Visibility = lblStatus.Visibility = Visibility.Collapsed; } );
                    SpinWait.SpinUntil( () => !waitThread.IsAlive );
                    if ( MainWindow.ImportOnLogin )
                        Paging.LoadPage( Pages.Import );
                    else
                        Paging.LoadPage( Pages.Viewer );
                } );
                waitThread.Start();
            } else
                Paging.LoadPage( Pages.Viewer );
        }
    }
}
