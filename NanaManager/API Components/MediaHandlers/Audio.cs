using NanaManagerAPI.Media;
using NanaManagerAPI.UI;
using System.Windows.Controls;

namespace NanaManager.MediaHandlers
{
    public class Audio : IMediaViewer
    {
        public string ID { get; } = "hydroxa.nanabrowser.media.audioHandler";
        public Page Display { get; set; }

        private readonly string[] compatibleTypes = new string[] { ".wav", ".flac", ".mp3", ".ogg", ".aiff", ".aac", ".wma", ".mka" };

        public string[] GetCompatibleTypes() {
            return compatibleTypes;
        }

        internal event UI.MediaLoader RenderMedia;

        public void LoadMedia( string Path, bool Editing ) {
            Display = new AudioViewer( this );
            RenderMedia.Invoke( Path, Editing );
        }
    }
}