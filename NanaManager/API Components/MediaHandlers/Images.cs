using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using NanaManagerAPI.Media;
using NanaManagerAPI.UI;
using NanaManagerAPI;

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
            Display = new ImageViewer( this );
            RenderMedia.Invoke( Path, Editing );
        }
    }
}
