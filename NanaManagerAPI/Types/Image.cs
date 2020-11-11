using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;

using NanaManagerAPI.Media;
using NanaManagerAPI.IO;

namespace NanaManagerAPI.Types
{
    /// <summary>
    /// A reference to a stored image, containing data describing it
    /// </summary>
    public class Image : IMedia
    {
        public const string CTOR_ID = "hydroxa.nanaManager:data_image";

        private readonly int[] tags;

        public string ID { get; }
        public string FileType { get; }

        /// <summary>
        /// Where the picture was taken.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Constructs a new reference to an image
        /// </summary>
        /// <param name="ID">The location of the image in the database</param>
        /// <param name="Tags">The tags associated with this image</param>
        /// <param name="FileType">The file extension of the media</param>
        public Image( string ID, int[] Tags, string FileType ) {
            this.ID = ID;
            tags = Tags;
            this.FileType = FileType;
        }

        public int[] GetTags() => tags;

        public BitmapImage GetSample() {
            if ( ContentFile.CheckValidity() ) {
                ContentFile.SetArchiveRead();
                ZipArchiveEntry entry = ContentFile.Archive.GetEntry( ID );
                if ( entry == null )
                    throw new FileNotFoundException( "Image was not found within the database", ID );
                else {
                    using Stream zipStream = entry.Open();
                    using MemoryStream ms = new MemoryStream();
                    zipStream.CopyTo( ms );
                    ms.Position = 0;

                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.DecodePixelWidth = 100;
                    bmp.DecodePixelHeight = 100;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();

                    return bmp;
                }
            }
            else
                throw new FileFormatException( "The content file is not in a readable state" );
        }
        /// <summary>
        /// Gets the <see cref="BitmapImage"/> referred to by this data
        /// </summary>
        /// <returns>The image as a <see cref="BitmapImage"/></returns>
        public BitmapImage GetImage() {
            if ( ContentFile.CheckValidity() ) {
                ContentFile.SetArchiveRead();
                ZipArchiveEntry entry = ContentFile.Archive.GetEntry( ID );
                if ( entry == null )
                    throw new FileNotFoundException( "Image was not found within the database", ID );
                else {
                    using Stream zipStream = entry.Open();
                    using MemoryStream ms = new MemoryStream();
                    zipStream.CopyTo( ms );
                    ms.Position = 0;

                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();

                    return bmp;
                }
            }
            else
                throw new FileFormatException( "The content file is not in a readable state" );
        }

        public string GetCustomData() {
            DataEncoder de = new DataEncoder();
            de.Write( Location );

            return de.ToString();
        }

        public void LoadCustomData( string Data ) {
            DataDecoder dd = new DataDecoder( Data );
            Location = dd.ReadString();
        }
    }
}