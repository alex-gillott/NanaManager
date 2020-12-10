using NanaManagerAPI.Media;
using NanaManagerAPI.UI;
using System.Windows.Controls;

namespace NanaManager.MediaHandlers
{
    public class Images : IMediaViewer
    {
        public string ID { get; } = "hydroxa.nanabrowser.media.imageHandler";
        public Page Display { get; set; }

        private readonly string[] compatibleTypes = new string[] { ".jpg", ".jpeg", ".gif", ".png", ".webp", ".tif", ".bmp" };

        public string[] GetCompatibleTypes() {
            return compatibleTypes;
        }

        internal event UI.MediaLoader RenderMedia;

        public void LoadMedia( string Path, bool Editing ) {
            Display = new ImageViewer( this ); //Opens the image viewer
            RenderMedia.Invoke( Path, Editing ); //Loads the media onto the screen
        }
    }
}
