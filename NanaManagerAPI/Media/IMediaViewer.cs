using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NanaManagerAPI.Media
{
    public interface IMediaViewer {
        string ID { get; }
        Page Display { set; get; }

        string[] GetCompatibleTypes();
        void LoadMedia( string Path, bool Editing );

    }
}
