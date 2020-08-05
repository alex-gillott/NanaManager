using System;
using System.Timers;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Animation;

using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI;
using NanaManager.SettingsPages;

namespace NanaManager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const int WM_SIZE = 0x5; //The windows message for resizing
		const int SIZE_MAXIMIZED = 2; //The message for resizing to maximised
		const int SIZE_RESTORED = 0; //The message for resizing to restored
		public static int SaveTimeMS = 1800000; //The period between each auto-save in milliseconds


		#region Paging
		private Storyboard sb; //Animator for the welcome screen

		private void onNewPage(Page newPage, string iD) {
			switch (iD) {
				case Pages.Welcome:
					Dispatcher.Invoke( () => //As this on a separate thread, invoke the animation through the UI thread
					{
						var da = new DoubleAnimation //TODO - MOVE THIS ONTO XAML
						{
							From = 1.0,
							To = 0.0,
							Duration = new Duration( TimeSpan.FromMilliseconds( 200 ) ),
							AutoReverse = true
						}; //Define the animation
						sb = new Storyboard();
						sb.Children.Add( da );
						Storyboard.SetTarget( da, frmMain );
						Storyboard.SetTargetProperty( da, new PropertyPath( Frame.OpacityProperty ) ); //We're making it blink, so target opacity
						frmMain.Content = newPage; //Show the welcome page
						sb.Begin(); //Initiate the animation
					}, System.Windows.Threading.DispatcherPriority.Render ); //Prioritise graphics over processing (We're just animating here)
					return;
				default:
					frmMain.Dispatcher.Invoke( (Action)(() => frmMain.Content = newPage) ); //Invoke page change on UI thread, as this is a separate thread
					break;
            }
        }
		#endregion


		public static bool ImportOnLogin = false;
		public bool Maximised = false;
		public Size PrevSize;
		public Point PrevLoc;

		private readonly System.Timers.Timer saveTimer = new System.Timers.Timer( SaveTimeMS );

		public MainWindow( int Instruction ) {
			ImportOnLogin = Instruction == 0x1;
			InitializeComponent(); //Initialises the UI elements

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

			Registry.RegisterSettings( new SettingsTab(Pages.ThemesAndColoursSettings, "Colours and Themes", new ThemesAndColours() ) );
			Registry.RegisterSettings( new SettingsTab( Pages.LanguagesSettings, "Languages", new ComingSoon() ) );
			Registry.RegisterSettings( new SettingsTab( Pages.TagsSettings, "Tags", new TagSettings() ) );
			Registry.RegisterSettings( new SettingsTab( Pages.SoonSettings, "Coming Soon", new ComingSoon() ) );
			Registry.RegisterSettings( new SettingsTab( Pages.InvalidSettings, "Invalid Settings Page", new InvalidSettingsPage() ) );

			Paging.PageChanged += onNewPage;
			Globals.Fullscreen += toggleFullscreen;
			Globals.OnOpenMedia += (Paging.GetPage( Pages.Fullscreen ) as Fullscreen).OpenViewer;
			if ( !string.IsNullOrEmpty(Properties.Settings.Default.Password) ) //Checking if the user has an account. If they do, go to login page, otherwise go to register page
				Paging.LoadPage(Pages.Login);
			else
				Paging.LoadPage(Pages.Register);

			saveTimer.Elapsed += onSaveTimerTick;
			saveTimer.Enabled = true;
		}

		#region Events
		private void onSaveTimerTick( object sender, ElapsedEventArgs e ) {
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
		private void textBlock_MouseUp( object sender, MouseButtonEventArgs e ) {
			txtClose.Background = (Brush)Application.Current.Resources["CloseButtonHighlight"];
			Close();
		}
		private void window_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {
			saveTimer.Stop();
			if ( ContentFile.CheckValidity() ) {
				ContentFile.SaveData();
				if ( Login.Password != null )
					ContentFile.Encrypt( Login.Password );
			}
			Logging.SaveLogs();
		}
		private void txtClose_MouseEnter( object sender, MouseEventArgs e ) => txtClose.Background = (Brush)Application.Current.Resources["CloseButtonHighlight"];
		private void txtClose_MouseDown( object sender, MouseButtonEventArgs e ) => txtClose.Background = (Brush)Application.Current.Resources["CloseButtonPressed"];
		private void txtClose_MouseLeave( object sender, MouseEventArgs e ) => txtClose.Background = Theme.Transparent;
		private void toggleFullscreen(bool fullscreen) {
			if (fullscreen) {
				rctControlBar.Visibility = Visibility.Collapsed;
				txtClose.Visibility = Visibility.Collapsed;
				Grid.SetRow( frmMain, 0 );
			} else {
				Grid.SetRow(frmMain, 1 );
				rctControlBar.Visibility = Visibility.Visible;
				txtClose.Visibility = Visibility.Visible;
			}
		}
		#endregion

		#region WNDProc
		protected override void OnSourceInitialized( EventArgs e ) {
			base.OnSourceInitialized( e );
			HwndSource source = PresentationSource.FromVisual( this ) as HwndSource;
			source.AddHook( wndProc );
		}
		private IntPtr wndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled ) {
			switch ( msg ) {
				case WM_SIZE:
					if ( wParam.ToInt32() == SIZE_MAXIMIZED ) {
						Maximised = true;
						var sc = System.Windows.Forms.Screen.FromPoint( new System.Drawing.Point( (int)Left, (int)Top ) );
						Height = sc.WorkingArea.Height;
						Width = sc.WorkingArea.Width;
						Left = sc.WorkingArea.X;
						Top = sc.WorkingArea.Y;
					}
					else if ( wParam.ToInt32() == SIZE_RESTORED )
						Maximised = false;
					break;
			}

			return IntPtr.Zero;
		}

		#endregion
	}
}