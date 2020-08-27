using System.IO;
using System.Drawing;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;

using NanaManagerAPI.Media;
using NanaManagerAPI.IO;
using System.Drawing.Imaging;

namespace NanaManagerAPI.Types
{
    /// <summary>
    /// A reference to a stored video, containing data describing it
    /// </summary>
    public class Video : IMedia
    {
        public BitmapImage thumbnail;

        public const string CTOR_ID = "hydroxa.nanaManager:data_Video";

        private readonly int[] tags;

        public string ID { get; }
        public string FileType { get; }

        /// <summary>
        /// Constructs a new reference to an video
        /// </summary>
        /// <param name="ID">The location of the video in the database</param>
        /// <param name="Tags">The tags associated with this video</param>
        /// <param name="FileType">The file extension of the media</param>
        public Video( string ID, int[] Tags, string FileType ) {
            this.ID = ID;
            tags = Tags;
            this.FileType = FileType;
        }

        public int[] GetTags() => tags;

        public BitmapImage GetSample() {
            if (thumbnail == null) {
                ZipArchiveEntry entry = ContentFile.Archive.GetEntry( ID );
                if ( entry == null )
                    throw new FileNotFoundException( "Image was not found within the database", ID );
                else {
                    string filePath = Path.Combine( ContentFile.TempPath, $"{ID}{FileType}" );
                    entry.ExtractToFile( filePath );
                    ShellFile sf = ShellFile.FromFilePath( filePath );
                    Bitmap bm = sf.Thumbnail.Bitmap;

                    using MemoryStream ms = new MemoryStream();
                    bm.Save( ms, ImageFormat.Jpeg );
                    ms.Position = 0;

                    File.Delete( filePath );

                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.DecodePixelWidth = 100;
                    bmp.DecodePixelHeight = 100;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    thumbnail = bmp;
                }
            }
            return thumbnail;
        }
    }
}