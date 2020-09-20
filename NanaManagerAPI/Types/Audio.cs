using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using NanaManagerAPI.IO;
using NanaManagerAPI.Media;

namespace NanaManagerAPI.Types
{
    public class Audio : IMedia {
        public const string CTOR_ID = "hydroxa.nanaManager:data_audio";

        private readonly int[] tags;

        public string ID { get; }
        public string FileType { get; }
        /// <summary>
        /// The album this piece of audio belongs to.
        /// </summary>
        public string Album { get; private set; }
        /// <summary>
        /// The list of artists that contributed in this audio.
        /// </summary>
        public StringCollection ContributingArtists { private set; get; }
        /// <summary>
        /// The genre this audio fits into.
        /// </summary>
        public string Genre { get; private set; }
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

        public BitmapImage GetSample() => UI.UI.AudioSymbol;

        public int[] GetTags() => tags;

        public string GetCustomData() {
            DataEncoder de = new DataEncoder();
            de.Write( Album );
            de.Write( Genre );
            de.Write( (IEnumerable<string>)ContributingArtists );


            return de.ToString();
        }
        public void LoadCustomData( string Data ) {
            DataDecoder dd = new DataDecoder( Data );

            Album = dd.ReadString();
            Genre = dd.ReadString();
            ContributingArtists = (StringCollection)dd.ReadStringArray();
        }
    }
}