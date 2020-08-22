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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using System.Windows.Media.Animation;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for AudioViewer.xaml
    /// </summary>
    public partial class AudioViewer : Page
    {
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();

        private readonly Timer intervalTimer = new Timer( 10 );

        private bool elapse = false;

        public AudioViewer( Audio Parent ) {
            InitializeComponent();
            Parent.RenderMedia += loadMedia;

            mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
            mediaPlayer.BufferingStarted += mediaPlayer_BufferingStarted;
            mediaPlayer.BufferingEnded += mediaPlayer_BufferingEnded;
            intervalTimer.Elapsed += intervalTimer_Elapsed;
        }

        private void intervalTimer_Elapsed( object sender, ElapsedEventArgs e ) {
            Dispatcher.Invoke( () =>
            {
                elapse = true;
                if ( mediaPlayer.Position.Ticks >= mediaPlayer.NaturalDuration.TimeSpan.Ticks ) {
                    mediaPlayer.Stop();
                    sldSeek.Value = 0;
                } else
                    sldSeek.Value = mediaPlayer.Position.Ticks;
            } );
        }

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
            bdrLoading.Visibility = Visibility.Visible;
            if ( editing ) {
                extract( id );
                mediaPlayer.Open( new Uri( Path.Combine( ContentFile.TempPath, id ) )  );
            } else {
                mediaPlayer.Open( new Uri( id ) );
            }
        }

        private void extract( string id ) {
            using ZipArchive archive = ZipFile.OpenRead( ContentFile.ContentPath );
            ZipArchiveEntry entry = archive.GetEntry( id );
            using StreamWriter fs = new StreamWriter( Path.Combine( ContentFile.TempPath, id ) );
            using StreamReader sr = new StreamReader( entry.Open() );
            fs.Write( sr.ReadToEnd() );
        }

        private void btnPlay_Checked( object sender, RoutedEventArgs e ) {
            mediaPlayer.Play();
        }

        private void btnPlay_Unchecked( object sender, RoutedEventArgs e ) {
            mediaPlayer.Pause();
        }

        private void sldSeek_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {
            if ( !elapse ) {
                mediaPlayer.Position = new TimeSpan( (long)sldSeek.Value );
                elapse = false;
            }
            lblTime.Content = $"{mediaPlayer.Position.Minutes:D2}:{mediaPlayer.Position.Seconds:D2}";
        }
    }
}