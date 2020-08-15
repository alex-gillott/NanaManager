using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;

using NanaManagerAPI.Media;
using NanaManagerAPI.UI;

namespace NanaManager.MediaHandlers
{
    public class Audio : IMediaViewer
        {
            public string ID { get; } = "hydroxa.nanabrowser.media.audioHandler";
            public Page Display { get; set; }

            private readonly string[] compatibleTypes = new string[] { ".wmv", ".mov", ".mpeg", ".wav", ".asf", ".midi", ".aiff", ".avi", ".aif" };

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
