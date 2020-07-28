using System.Windows.Media;

namespace NanaManager
{
    public static class Theme
    {
        /// <summary>
        /// For transparency when XAML implementation is not available
        /// </summary>
        public static SolidColorBrush Transparent = new SolidColorBrush( Color.FromArgb( 0, 0, 0, 0 ) );
    }
}
