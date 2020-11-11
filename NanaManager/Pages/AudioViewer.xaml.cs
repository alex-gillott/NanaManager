using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO.Compression;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using NanaManagerAPI;
using NanaManagerAPI.IO;
using NanaManager.MediaHandlers;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for AudioViewer.xaml
    /// </summary>
    public partial class AudioViewer : Page
    {
        private static string lastViewed;

        private readonly MediaPlayer mediaPlayer = new MediaPlayer();

        private Timer intervalTimer;

        private bool elapse = false;

        public AudioViewer( Audio Parent ) {
            InitializeComponent();

            Parent.RenderMedia += loadMedia;

            mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += mediaPlayer_MediaEnded;
            mediaPlayer.BufferingStarted += mediaPlayer_BufferingStarted;
            mediaPlayer.BufferingEnded += mediaPlayer_BufferingEnded;
            mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
        }

        private void mediaPlayer_MediaEnded( object sender, EventArgs e ) {
            elapse = true;
            btnPlay.IsChecked = false;
            mediaPlayer.Position = new TimeSpan( 0 );
            sldSeek.Value = 0;
        }

        private void mediaPlayer_MediaFailed( object sender, ExceptionEventArgs e ) {
            Logging.Write( e.ErrorException, "AudioViewer", LogLevel.Error );
            lblLoading.Content = "An error occured while loading the video";
        }

        private void intervalTimer_Elapsed( object sender, ElapsedEventArgs e ) => Dispatcher.Invoke( () =>
        {
            elapse = true;
            sldSeek.Value = mediaPlayer.Position.Ticks;
        } );

        private void mediaPlayer_BufferingEnded( object sender, EventArgs e ) {
            ((Storyboard)Resources["spinBuffer"]).Stop();
            elpBuffering.Visibility = Visibility.Collapsed;
            intervalTimer.Start();
        }

        private void mediaPlayer_BufferingStarted( object sender, EventArgs e ) {
            elpBuffering.Visibility = Visibility.Visible;
            ((Storyboard)Resources["spinBuffer"]).Begin();
            intervalTimer.Stop();
        }

        private void mediaPlayer_MediaOpened( object sender, EventArgs e ) {
            bdrLoading.Visibility = Visibility.Collapsed;
            lblmaxTime.Content = $"{mediaPlayer.NaturalDuration.TimeSpan.Minutes:00}:{mediaPlayer.NaturalDuration.TimeSpan.Seconds:00}";
            sldSeek.Value = 0;
            sldSeek.Maximum = mediaPlayer.NaturalDuration.TimeSpan.Ticks;
            intervalTimer = new Timer( 10 );
            intervalTimer.Elapsed += intervalTimer_Elapsed;
            intervalTimer.Start();
        }

        private void loadMedia( string id, bool editing ) {
            if ( id != Path.GetFileNameWithoutExtension( lastViewed ) && lastViewed != null ) {
                File.Delete( lastViewed );
                lastViewed = null;
            }
            mediaPlayer.Stop();
            mediaPlayer.Close();
            lblLoading.Content = "Loading..";
            bdrLoading.Visibility = Visibility.Visible;
            if ( editing ) {
                extract( id );
                mediaPlayer.Open( new Uri( Path.Combine( ContentFile.TempPath, $"temp{Data.Media[id].FileType}" ), UriKind.Absolute ) );
                lastViewed = Path.Combine( ContentFile.TempPath, $"temp{Data.Media[id].FileType}" );
            }
            else
                mediaPlayer.Open( new Uri( id ) );
        }

        private void extract( string id ) {
            ContentFile.SetArchiveRead();
            ZipArchiveEntry entry = ContentFile.Archive.GetEntry( id );
            string path = Path.Combine( ContentFile.TempPath, $"temp{Data.Media[id].FileType}" );
            if ( File.Exists( path ) ) File.Delete( path );
            entry.ExtractToFile( path );
        }

        private void btnPlay_Checked( object sender, RoutedEventArgs e ) =>
            mediaPlayer.Play();

        private void btnPlay_Unchecked( object sender, RoutedEventArgs e ) =>
            mediaPlayer.Pause();

        private void sldSeek_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {
            if ( !elapse )
                mediaPlayer.Position = new TimeSpan( (long)sldSeek.Value );
            else
                elapse = false;
            lblTime.Content = $"{mediaPlayer.Position.Minutes:D2}:{mediaPlayer.Position.Seconds:D2}";
        }

        private void btnVolume_Checked( object sender, RoutedEventArgs e ) =>
            ((Storyboard)Resources["growVol"]).Begin();

        private void btnVolume_Unchecked( object sender, RoutedEventArgs e ) =>
            ((Storyboard)Resources["shrinkVol"]).Begin();

        private void sldSeek_MouseLeftButtonDown( object sender, MouseButtonEventArgs e ) =>
            btnPlay.IsChecked = false;

        private void sldSeek_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) =>
            btnPlay.IsChecked = true;

        private void slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {
            if ( lblVol != null ) {
                lblVol.Content = $"{(int)(sldVol.Value * 100):D1}%";
                if ( sldVol.Value == 0 )
                    pthMute.Visibility = Visibility.Visible;
                else {
                    pthMute.Visibility = Visibility.Collapsed;
                    if ( sldVol.Value > 0.3 ) {
                        pthVol1.Visibility = Visibility.Visible;
                        if ( sldVol.Value > 0.7 )
                            pthVol2.Visibility = Visibility.Visible;
                        else
                            pthVol2.Visibility = Visibility.Collapsed;
                    }
                    else
                        pthVol1.Visibility = Visibility.Collapsed;
                }
            }
            mediaPlayer.Volume = sldVol.Value;
        }

        private void page_Unloaded( object sender, RoutedEventArgs e ) {
            intervalTimer?.Stop();
            intervalTimer?.Dispose();
            mediaPlayer?.Stop();
            mediaPlayer?.Close();
            if ( lastViewed != null )
                File.Delete( lastViewed );
            lastViewed = null;
        }
    }
}