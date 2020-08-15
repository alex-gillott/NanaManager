using Ionic.Zip;
using NanaManager.MediaHandlers;
using NanaManagerAPI.IO;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for AudioViewer.xaml
    /// </summary>
    public partial class AudioViewer : Page
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public AudioViewer( Audio Parent ) {
            InitializeComponent();
            Parent.RenderMedia += loadMedia;
        }

        private void loadMedia( string id, bool editing ) {
            if ( editing ) {
                
            } else {

            }
        }

        private async void ExtractAsync( string id ) {
            using ( ZipArchive archive = ZipFile.OpenRead( ContentFile.ContentPath ) )
                using (ZipEntry entry = archive.GetEntry(id) )

        }
    }
}
