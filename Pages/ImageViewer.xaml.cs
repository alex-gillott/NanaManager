using NanaManagerAPI;
using NanaManagerAPI.Threading;
using NanaManager.MediaHandlers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : Page
    {
        private delegate void imageHandler( BitmapImage B );
        private event imageHandler OnImageLoaded;

        NanaManagerAPI.Data.Image img;
        string toLoad;
        public ImageViewer(Images Parent) {
            InitializeComponent();
            Parent.RenderMedia += loadMedia;
            OnImageLoaded += ImageLoaded;
        }

        private void loadMedia(string id, bool edit) {
            RenderOptions.SetBitmapScalingMode( imgView, BitmapScalingMode.NearestNeighbor );
            if ( edit ) {
                BackgroundWorker worker = new BackgroundWorker();
                img = Globals.Media[id] as NanaManagerAPI.Data.Image;
                imgView.Source = img.GetSample();
                worker.DelegateThread(LoadHiRes);
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
                worker.DelegateThread( LoadHiRes );
            }
        }

        private void ImageLoaded( BitmapImage b ) {
            imgView.Dispatcher.BeginInvoke( new Action( () =>
            {
                RenderOptions.SetBitmapScalingMode( imgView, BitmapScalingMode.Fant );
                imgView.Source = b;
            } ) );
        }

        private void LoadHiRes() {
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

        private void Page_Unloaded( object sender, RoutedEventArgs e ) {
        }
    }
}
