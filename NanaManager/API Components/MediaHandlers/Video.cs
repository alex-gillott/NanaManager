using NanaManagerAPI.Media;
using NanaManagerAPI.UI;
using System.Windows.Controls;

namespace NanaManager.MediaHandlers
{
    public class Video : IMediaViewer
    {
        public string ID { get; } = "hydroxa.nanabrowser.media.videoHandler";
        public Page Display { get; set; }

        private readonly string[] compatibleTypes = new string[] { ".mp4", ".webm", ".mov", ".mpg", ".mpeg", ".mkv" };

        public string[] GetCompatibleTypes() => compatibleTypes;

        internal event UI.MediaLoader RenderMedia;

        public void LoadMedia( string Path, bool Editing ) {
            Display = new VideoViewer( this ); //Opens the video viewer
            RenderMedia.Invoke( Path, Editing ); //Loads the media onto the screen
        }
    }
}
