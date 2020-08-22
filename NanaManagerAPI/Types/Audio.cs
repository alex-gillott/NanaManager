using NanaManagerAPI.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NanaManagerAPI.Types
{
    public class Audio : IMedia
    {
        public const string CTOR_ID = "hydroxa.nanaManager:data_Audio";

        private readonly int[] tags;

        public string ID { get; }
        public string FileType { get; }

        /// <summary>
        /// Constructs a new reference to audio
        /// </summary>
        /// <param name="ID">The location of the audio in the database</param>
        /// <param name="Tags">The tags associated with this image</param>
        /// <param name="FileType">The file extension of the media</param>
        public Audio( string ID, int[] Tags, string FileType ) {
            this.ID = ID;
            tags = Tags;
            this.FileType = FileType;
        }

        public BitmapImage GetSample() {
            return new BitmapImage();
        }

        public int[] GetTags() => tags;
    }
}
