using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NanaManagerAPI.Media
{
    public interface IMedia
    {
        /// <summary>
		/// The location of the media in the database
		/// </summary>
        string ID { get; }
        /// <summary>
        /// The file type the media originally was
        /// </summary>
        string FileType { get; }
        /// <summary>
        /// Gets the tags associated with the media
        /// </summary>
        /// <returns>An int array of tags</returns>
        int[] GetTags();
        /// <summary>
        /// Gets a 100x100 sample image of the media for display in the Viewer
        /// </summary>
        /// <returns>A small image</returns>
        BitmapImage GetSample();
    }
}
