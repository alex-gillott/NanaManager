using NanaManagerAPI;
using NanaManagerAPI.Threading;
using NanaManager.MediaHandlers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Input;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : Page
    {
        private delegate void imageHandler( BitmapImage B );
        private event imageHandler OnImageLoaded;

        NanaManagerAPI.Types.Image img;
        string toLoad;

        private readonly ScaleTransform imgST;
        private readonly TranslateTransform imgTT;

        public ImageViewer(Images Parent) {
            InitializeComponent();
            Parent.RenderMedia += loadMedia;
            OnImageLoaded += imageLoaded;

            imgST = (ScaleTransform)((TransformGroup)imgView.RenderTransform).Children.First( tr => tr is ScaleTransform );
            imgTT = (TranslateTransform)((TransformGroup)imgView.RenderTransform).Children.First( tr => tr is TranslateTransform );
        }

        private void loadMedia(string id, bool edit) {
            RenderOptions.SetBitmapScalingMode( imgView, BitmapScalingMode.NearestNeighbor );
            if ( edit ) {
                BackgroundWorker worker = new BackgroundWorker();
                img = Data.Media[id] as NanaManagerAPI.Types.Image;
                imgView.Source = img.GetSample();
                worker.DelegateThread(loadHiRes);
            }
            else {
                img = null;
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.DecodePixelWidth = 100;
                bmp.DecodePixelHeight = 100;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri( id );
                bmp.EndInit();
                imgView.Source = bmp;
                toLoad = id;

                BackgroundWorker worker = new BackgroundWorker();
                worker.DelegateThread( loadHiRes );
            }
        }

        private void imageLoaded( BitmapImage b ) {
            imgView.Dispatcher.BeginInvoke( new Action( () =>
            {
                RenderOptions.SetBitmapScalingMode( imgView, BitmapScalingMode.Fant );
                imgView.Source = b;
                
                imgST.ScaleX = imgST.ScaleY = 1;
                imgTT.X = imgTT.Y = 0;
            } ) );
        }

        private void loadHiRes() {
            if ( img == null ) {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri( toLoad );
                bmp.EndInit();
                bmp.Freeze();
                OnImageLoaded.Invoke( bmp );
            }
            else {
                BitmapImage b = img.GetImage();
                b.Freeze();
                OnImageLoaded.Invoke( b );
            }
        }

        private void page_Unloaded( object sender, RoutedEventArgs e ) {
        }

        private void imgView_MouseWheel( object sender, MouseWheelEventArgs e ) {
            if ( e.Delta > 0 || imgST.ScaleX > 1 ) {
                double zoom = e.Delta > 0 ? .2 : -.2;
                imgST.ScaleX += zoom;
                imgST.ScaleY += zoom;
            }
            else if ( e.Delta < 0 )
                imgST.ScaleX = imgST.ScaleY = 1;
        }

        private Point start;
        private Point origin;

        private void imgView_MouseLeftButtonDown( object sender, MouseButtonEventArgs e ) {
            if ( Keyboard.IsKeyDown( Key.LeftCtrl ) )
                imgST.ScaleX = imgST.ScaleY = 1;
            else {
                imgView.Width = ActualWidth;
                start = e.GetPosition( bdrImg );
                origin = new Point( imgTT.X, imgTT.Y );

                imgView.CaptureMouse();
            }
        }

        private void imgView_MouseMove( object sender, MouseEventArgs e ) {
            if (imgView.IsMouseCaptured) {
                Vector v = start - e.GetPosition( bdrImg );
                imgTT.X = origin.X - (v.X / imgST.ScaleX);
                imgTT.Y = origin.Y - (v.Y / imgST.ScaleY);
                if ( imgTT.X < ActualWidth - (imgView.ActualWidth * imgST.ScaleX) )
                    imgTT.X = ActualWidth - (imgView.ActualWidth * imgST.ScaleX);
            }
        }

        private void imgView_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
            imgView.ReleaseMouseCapture();
        }
    }
}
