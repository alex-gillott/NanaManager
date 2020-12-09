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

        private void onNewPage( Page newPage, string iD ) { //Handles when the page changes
            switch ( iD ) {
                case Pages.Welcome: //Specifically animate if the page is the Welcome page (Loading)
                    Dispatcher.Invoke( () => //As this on a separate thread, invoke the animation through the UI thread
                    {
                        frmMain.Content = newPage; //Show the welcome page
                        ((Storyboard)Resources["blinkScreen"]).Begin(); //Animation to blink the screen
                    }, System.Windows.Threading.DispatcherPriority.Render ); //Process this operation at the same priority as rendering
                    saveTimer.Start(); //Start the save timer after login such that we don't try to save invalid data and run a useless thread in the background
                    break;
                default:
                    frmMain.Dispatcher.Invoke( (Action)(() => frmMain.Content = newPage) ); //Invoke page change on UI thread, as this is a separate thread
                    break;
            }
        }

        #endregion Paging

        ///<summary>
        ///Future feature. Please do not access
        ///</summary>
        public static bool ImportOnLogin = false; //For shell integration. Currently unused, but is in prospect
        ///<summary>
        ///Is true when the screen is in Fullscreen mode
        //</summary>
        public bool IsFullscreen { private set; get; } = false;
        ///<summary>
        ///Is true when the screen is maximised
        ///</summary>
        public bool Maximised { private set; get; } = false;
        ///<summary>
        ///The Size of the window before fullscreen
        ///</summary>
        public Size PrevSize;
        ///<summary>
        ///The location of the window before fullscreen
        ///</summary>
        public Point PrevLoc;

        private readonly Timer saveTimer = new Timer( SaveTimeMS ); //Ticks every SaveTimeMS milliseconds to automatically save the data
        private readonly Timer notifLeaveTimer = new Timer( NotificationFadeTimeMS ); //Starts whenever a notification is displayed. Restarts when mouse hovers over notif control

        public MainWindow() {
            InitializeComponent(); //Initialises the UI elements

            UI.ThemeLightnessChanged += onLightThemeSet; //Used for theme light/dark modes
            UI.IsLightTheme = false;

            Logging.Write( "Running STAThread plugins", "Plugins", LogLevel.Info );
            foreach ( MethodInfo mi in Plugins.NeedSTA ) //Executing plugins that require STA Threads (Slowest, but thread safe and can access COM)
                mi.Invoke( null, null );

            addPages(); //Registering the internal pages
            addSettings(); //Registering the internal settings pages

            Paging.PageChanged += onNewPage; //Handles the PageChanged event, so that the window may display the desired page
            UI.Fullscreen += toggleFullscreen; //Handles the Fullscreen event, so that the window may become fullscreen when desired
            UI.MediaOpened += (Paging.GetPage( Pages.Fullscreen ) as Fullscreen).OpenViewer; //Binds the Fullscreen.OpenViewer method to the MediaOpened event, as to allow it to handle when media is opened
            UI.NotificationRaised += onNotificationChange; //When the UI wants a notification displayed, display with the default notifier
            UI.WindowClosed += onWindowClose; //Executes when the UI wants to close the window

            if ( !string.IsNullOrEmpty( Properties.Settings.Default.Password ) ) //Checking if the user has an account. If they do, go to login page, otherwise go to register page
                Paging.LoadPage( Pages.Login );
            else
                Paging.LoadPage( Pages.Register );

            saveTimer.Elapsed += onSaveTimerTick; //Save when the Save Timer elapses
        }

        #region Helper Methods

        private void addPages() { //Registers the pages
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

        private void addSettings() { //Registers the settings
            Registry.RegisterSettings( new SettingsTab( Pages.ThemesAndColoursSettings, "Colours and Themes", new ThemesAndColours() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.LanguagesSettings, "Languages", new ComingSoon() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.TagsSettings, "Tags", new TagSettings() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.SoonSettings, "Coming Soon", new ComingSoon() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.InvalidSettings, "Invalid Settings Page", new InvalidSettingsPage() ) );
            Registry.RegisterSettings( new SettingsTab( Pages.AdvancedSettings, "Advanced Settings", new AdvancedSettings() ) );
        }

        private void refreshMaximizeRestoreButton() { //Controls the Maximise/Restore button's visuals
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
            if ( WindowState == WindowState.Maximized ) //Toggle Restored and Maximised window sizes
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void onCloseButtonClick( object sender, RoutedEventArgs e ) => UI.CloseApplication(); //Allows plugins to easily execute code when the application closes

        private void onLightThemeSet( bool set ) => imgIcon.Source = set ? UI.LogoDark : UI.LogoLight; //Sets the Logo between dark and light mode

        private void onNotificationChange( string text, object[] _ ) {
            lblNotif.Content = text;
            notifLeaveTimer.Restart();
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
                    Left = e.GetPosition( this ).X - (Width / 2); //Center window to the mouse
                    Top = 0;
                }
                DragMove(); //Drag the window
            }
        }

        private void window_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {
            Logging.Write( "Closing application", "UI", LogLevel.Info );

            saveTimer.Stop();
            if ( ContentFile.CheckValidity() ) { //If the ContentFile is decrypted, attempt to save data
                ContentFile.SaveData(); //TODO - DISCREPENCY IN UNENCRYPTED SAVING. ENSURE THE ARCHIVE IS ACTUALLY OPEN!!
                if ( Login.Password != null )
                    ContentFile.Encrypt( Login.Password ); //Encrypt the data
            }
        }

        private void toggleFullscreen( bool fullscreen ) {
            IsFullscreen = fullscreen;
            if ( fullscreen ) {
                Logging.Write( "Changing to Fullscreen mode", "UI" );
                WindowState = WindowState.Maximized;
                Grid.SetRow( frmMain, 0 ); //Fill the window with the frame

                grdTitle.Visibility = Visibility.Collapsed;
                wchChrome.CaptionHeight = 0; //Hide the window chrome
                MaxHeight = MaxWidth = double.PositiveInfinity; //Don't limit the scale, so Maximised actually fills the screen
            }
            else {
                Logging.Write( "Leaving Fullscreen mode", "UI" );
                Grid.SetRow( frmMain, 1 ); //Make space for the window chrome
                grdTitle.Visibility = Visibility.Visible;
                wchChrome.CaptionHeight = 25;
                if ( Maximised ) {
                    var sc = System.Windows.Forms.Screen.FromPoint( new System.Drawing.Point( (int)Left, (int)Top ) ); //Get the screen at the provided coordinates (Multiscreen support)
                    MaxHeight = sc.WorkingArea.Height + 7; //For some reason, WPF scales to +7 of the working area. Visuals break if I don't do this
                    MaxWidth = sc.WorkingArea.Width + 7;
                } else
                    WindowState = WindowState.Normal;
            }
        }

        private void bdrNotif_MouseEnter( object sender, MouseEventArgs e ) {
            notifLeaveTimer.Stop();
        }

        private void bdrNotif_MouseLeave( object sender, MouseEventArgs e ) {
            notifLeaveTimer.Restart();
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
                } else
                    Maximised = false; //Considering fullscreen to be a different window state
            }
            else {
                MaxHeight = MaxWidth = double.PositiveInfinity;
                Maximised = false;
            }
        }

        #endregion Events
    }
}
