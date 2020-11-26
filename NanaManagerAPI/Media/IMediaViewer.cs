using System.Windows.Controls;

namespace NanaManagerAPI.Media
{
    public interface IMediaViewer
    {
        string ID { get; }
        Page Display { set; get; }

        string[] GetCompatibleTypes();

        void LoadMedia( string Path, bool Editing );
    }
}