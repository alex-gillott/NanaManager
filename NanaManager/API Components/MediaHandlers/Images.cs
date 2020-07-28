using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using NanaManagerAPI;
using NanaManagerAPI.Media;

namespace NanaManager.MediaHandlers
{
    public class Images : IMediaViewer
    {
        public string ID { get; }
        public Page Display { get; set; }

        private readonly string[] compatibleTypes;

        public string[] GetCompatibleTypes() {
            return compatibleTypes;
        }

        internal event MediaLoader RenderMedia;

        public void LoadMedia( string Path, bool Editing ) {
            Display = new ImageViewer( this );
            RenderMedia.Invoke( Path, Editing );
        }

        public Images() {
            ID = "hydroxa.nanabrowser.media.imageHandler";
            compatibleTypes = new string[] { ".jpg", ".jpeg", ".gif", ".png", ".webp", ".tif", ".bmp" };
            Globals.AddToCatagory( "Image Files", compatibleTypes );
        }
    }
}
