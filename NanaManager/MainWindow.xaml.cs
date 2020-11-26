using NanaManager.SettingsPages;
using NanaManagerAPI;
using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using System;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The period between each auto-save in milliseconds
        /// </summary>
        public static int SaveTimeMS = 900000;

        /// <summary>
        /// The time a notification displays before attempting to fade out
        /// </summary>
        public static int NotificationFadeTimeMS = 5000;

        #region Paging

        private void onNewPage( Page newPage, string iD ) {
            switch ( iD ) {
                case Pages.Welcome:
                    Dispatcher.Invoke( () => //As this on a separate thread, invoke the animation through the UI thread
                    {
                        frmMain.Content = newPage; //Show the welcome page
                        ((Storyboard)Resources["blinkScreen"]).Begin();
                    }, System.Windows.Threading.DispatcherPriority.Render ); //Process this operation at the same priority as rendering
                    return;

                default:
                    frmMain.Dispatcher.Invoke( (Action)(() => frmMain.Content = newPage) ); //Invoke page change on UI thread, as this is a separate thread
                    break;
            }
        }

        #endregion Paging

        public static bool ImportOnLogin = false;
        public bool IsFullscreen = false;
        public bool Maximised = false;
        public Size PrevSize;
        public Point PrevLoc;

        private readonly Timer saveTimer = new Timer( SaveTimeMS );
        private readonly Timer notifLeaveTimer = new Timer( NotificationFadeTimeMS );

        public MainWindow() {
            InitializeComponent(); //Initialises the UI elements

            UI.ThemeLightnessChanged += onLightThemeSet;
            UI.IsLightTheme = false;

            Logging.Write( "Running STAThread plugins", "Plugins", LogLevel.Info );
            foreach ( MethodInfo mi in Plugins.NeedSTA )
                mi.Invoke( null, null );

            addPages();
            addSettings();

            Paging.PageChanged += onNewPage;
            UI.Fullscreen += toggleFullscreen;
            UI.MediaOpened += (Paging.GetPage( Pages.Fullscreen ) as Fullscreen).OpenViewer;
            UI.NotificationRaised += onNotificationChange;
            UI.WindowClosed += onWindowClose;

            if ( !string.IsNullOrEmpty( Properties.Settings.Default.Password ) ) //Checking if the user has an account. If they do, go to login page, otherwise go to register page
                Paging.LoadPage( Pages.Login );
            else
                Paging.LoadPage( Pages.Register );

            saveTimer.Elapsed += onSaveTimerTick;
            saveTimer.Start();
        }

        #region Helper Methods

        private void addPages() {
            Logging.Write( "Registering Pages", "Init" );
            Paging.AddPage( Pages.Error, new Error() );
            Paging.AddPage( Pages.Fullscreen, new Fullscreen() );
            Paging.AddPage( Pages.Import, new Import() );
            Paging.AddPage( Pages.Login, new Login() );
            Paging.AddPage( Pages.Register, new Register() );
            Paging.AddPage( Pages.Settings, new SettingsPage() );
            Paging.AddPage( Pages.TagManager, new TagManager() );
            Paging.AddPage( Pages.Viewer, new Viewer() );
            Paging.AddPage( Pages.Welcome, new Welcome() );
            Paging.AddPage( Pages.Search, new Search() );
            Paging.AddPage( Pages.PluginsSettings, new PluginsSettings() );
        }

        private void addSettings() {
            Registry.RegisterSettings( new SettingsTab( Pages.ThemesAndColoursSettings, "Colours and Themes", new ThemesAndColours() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.LanguagesSettings, "Languages", new ComingSoon() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.TagsSettings, "Tags", new TagSettings() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.SoonSettings, "Coming Soon", new ComingSoon() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.InvalidSettings, "Invalid Settings Page", new InvalidSettingsPage() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.AdvancedSettings, "Advanced Settings", new AdvancedSettings() ) );
        }

        private void refreshMaximizeRestoreButton() {
            if ( WindowState == WindowState.Maximized ) {
                btnMaximise.Visibility = Visibility.Collapsed;
                btnRestore.Visibility = Visibility.Visible;
            }
            else {
                btnMaximise.Visibility = Visibility.Visible;
                btnRestore.Visibility = Visibility.Collapsed;
            }
        }

        #endregion Helper Methods

        #region Events

        private void onWindowClose() => Close();

        private void onMinimizeButtonClick( object sender, RoutedEventArgs e ) => WindowState = WindowState.Minimized;

        private void onMaximizeRestoreButtonClick( object sender, RoutedEventArgs e ) {
            if ( WindowState == WindowState.Maximized )
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void onCloseButtonClick( object sender, RoutedEventArgs e ) => UI.CloseApplication();

        private void onLightThemeSet( bool set ) => imgIcon.Source = set ? UI.LogoDark : UI.LogoLight;

        private void onNotificationChange( string text, object[] _ ) {
            lblNotif.Content = text;
            notifLeaveTimer.Start();
        }

        private void onSaveTimerTick( object sender, ElapsedEventArgs e ) {
            Logging.Write( "Autosaving..", "Auto Save" );
            if ( ContentFile.CheckValidity() )
                ContentFile.SaveData();
            Logging.SaveLogs();
        }

        private void grdMouseDown( object sender, MouseButtonEventArgs e ) {
            if ( e.ChangedButton == MouseButton.Left ) {
                if ( WindowState == WindowState.Maximized ) { //If already maximised, return to normal
                    WindowState = WindowState.Normal;
                    Left = e.GetPosition( this ).X - (Width / 2);
                    Top = 0;
                }
                DragMove(); //Drag the window
            }
        }

        private void window_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {
            Logging.Write( "Closing application", "UI", LogLevel.Info );

            saveTimer.Stop();
            if ( ContentFile.CheckValidity() ) {
                ContentFile.SaveData();
                if ( Login.Password != null )
                    ContentFile.Encrypt( Login.Password );
            }
            Logging.SaveLogs();
        }

        private void toggleFullscreen( bool fullscreen ) {
            IsFullscreen = fullscreen;
            if ( fullscreen ) {
                Logging.Write( "Changing to Fullscreen mode", "UI" );
                WindowState = WindowState.Maximized;
                Grid.SetRow( frmMain, 0 );

                grdTitle.Visibility = Visibility.Collapsed;
                wchChrome.CaptionHeight = 0;
                MaxHeight = MaxWidth = double.PositiveInfinity;
            }
            else {
                Logging.Write( "Leaving Fullscreen mode", "UI" );
                Grid.SetRow( frmMain, 1 );
                grdTitle.Visibility = Visibility.Visible;
                wchChrome.CaptionHeight = 25;
                if ( !Maximised )
                    WindowState = WindowState.Normal;
                else {
                    var sc = System.Windows.Forms.Screen.FromPoint( new System.Drawing.Point( (int)Left, (int)Top ) );
                    MaxHeight = sc.WorkingArea.Height + 7;
                    MaxWidth = sc.WorkingArea.Width + 7;
                }
            }
        }

        private void bdrNotif_MouseEnter( object sender, MouseEventArgs e ) {
        }

        private void bdrNotif_MouseLeave( object sender, MouseEventArgs e ) {
        }

        private void stkNotifList_MouseLeave( object sender, MouseEventArgs e ) {
        }

        private void window_StateChanged( object sender, EventArgs e ) {
            refreshMaximizeRestoreButton();
            if ( WindowState == WindowState.Maximized ) {
                if ( !IsFullscreen ) {
                    Maximised = true;
                    grdTitle.Visibility = Visibility.Visible;
                    wchChrome.CaptionHeight = 25;
                    var sc = System.Windows.Forms.Screen.FromPoint( new System.Drawing.Point( (int)Left, (int)Top ) );
                    MaxHeight = sc.WorkingArea.Height + 7;
                    MaxWidth = sc.WorkingArea.Width + 7;
                }
            }
            else {
                MaxHeight = MaxWidth = double.PositiveInfinity;
                Maximised = false;
            }
        }

        #endregion Events
    }
}