using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NanaManagerAPI;
using NanaManagerAPI.UI;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : Page
    {
        public static int[] SearchTags = Array.Empty<int>();
        public static int[] RejectedTags = Array.Empty<int>();
        private Thread checkThread;
        public Search() {
            InitializeComponent();
        }

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            cbxHiddenTags.IsChecked = tslSearch.ShowHiddenTags = Data.ShowHiddenTags;
            checkThread = new Thread( checktags );
            checkThread.Start();
        }

        private void checktags() {
            tslSearch.CheckTags( SearchTags );
            tslSearch.RejectTags( RejectedTags );
        }

        private void button_Click( object sender, RoutedEventArgs e ) {
            Paging.LoadPreviousPage();
        }

        private void button_Click_1( object sender, RoutedEventArgs e ) {
            tslSearch.ClearTags();
            //TODO - Add Prompt
        }

        private void button_Click_2( object sender, RoutedEventArgs e ) {
            SearchTags = tslSearch.GetCheckedTagsIndicies();
            RejectedTags = tslSearch.GetRejectedTagsIndicies();
            Paging.LoadPreviousPage();
        }

        private void cbxHiddenTags_Checked( object sender, RoutedEventArgs e ) {
            tslSearch.ShowHiddenTags = Data.ShowHiddenTags = true;
            tslSearch.Reload();
        }

        private void cbxHiddenTags_Unchecked( object sender, RoutedEventArgs e ) {
            tslSearch.ShowHiddenTags = Data.ShowHiddenTags = false;
            tslSearch.Reload();
        }
    }
}
