using System.Windows;
using System.Windows.Controls;

using NanaManagerAPI.UI;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Error.xaml
    /// </summary>
    public partial class Error : Page
    {
        public Error() {
            InitializeComponent();
            txtMsg.Text = string.Format( txtMsg.Text, System.IO.Directory.GetCurrentDirectory() );
        }

        private void Button_Click( object sender, RoutedEventArgs e ) => Paging.LoadPreviousPage();
    }
}
