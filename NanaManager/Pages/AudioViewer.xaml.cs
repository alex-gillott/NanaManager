using NanaManager.MediaHandlers;
using NanaManagerAPI.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using NanaManagerAPI;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for AudioViewer.xaml
    /// </summary>
    public partial class AudioViewer : Page
    {
        private static string lastViewed;

        private readonly MediaPlayer mediaPlayer = new MediaPlayer();

        private readonly Timer intervalTimer;

        private bool elapse = false;

        public AudioViewer( Audio Parent ) {
            InitializeComponent();
            intervalTimer = new Timer( 10 );

            Parent.RenderMedia += loadMedia;

            mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
            mediaPlayer.BufferingStarted += mediaPlayer_BufferingStarted;
            mediaPlayer.BufferingEnded += mediaPlayer_BufferingEnded;
            mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
            intervalTimer.Elapsed += intervalTimer_Elapsed;
        }

        private void mediaPlayer_MediaFailed( object sender, ExceptionEventArgs e ) {
            Logging.Write( e.ErrorException, "AudioViewer" );
        }

        private void intervalTimer_Elapsed( object sender, ElapsedEventArgs e ) => Dispatcher.Invoke( () =>
                                                                                   {
                                                                                       elapse = true;
                                                                                       if ( mediaPlayer.Position.Ticks >= mediaPlayer.NaturalDuration.TimeSpan.Ticks ) {
                                                                                           btnPlay.IsChecked = false;
                                                                                           mediaPlayer.Position = new TimeSpan( 0 );
                                                                                           sldSeek.Value = 0;
                                                                                       }
                                                                                       else
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
            intervalTimer.Start();
        }

        

        private void loadMedia( string id, bool editing ) {
            if ( id != Path.GetFileNameWithoutExtension( lastViewed ) && lastViewed != null ) {
                File.Delete( lastViewed );
                lastViewed = null;
            }
            bdrLoading.Visibility = Visibility.Visible;
            if ( editing ) {
                extract( id );
                mediaPlayer.Open( new Uri( Path.Combine( ContentFile.TempPath, $"{id}{Data.Media[id].FileType}" ) ) );
                lastViewed = Path.Combine( ContentFile.TempPath, $"{id}{Data.Media[id].FileType}" );
            }
            else
                mediaPlayer.Open( new Uri( id ) );
        }

        private void extract( string id ) {
            using ZipArchive archive = ZipFile.OpenRead( ContentFile.ContentPath );
            ZipArchiveEntry entry = archive.GetEntry( id );
            using StreamWriter fs = new StreamWriter( Path.Combine( ContentFile.TempPath, $"{id}{Data.Media[id].FileType}" ) );
            using StreamReader sr = new StreamReader( entry.Open() );
            fs.Write( sr.ReadToEnd() );
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
            intervalTimer.Stop();
            intervalTimer.Dispose();
            File.Delete( lastViewed );
            lastViewed = null;
        }
    }
}