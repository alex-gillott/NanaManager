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
using NanaManagerAPI.EventArgs;

namespace NanaManager.SettingsPages
{
    /// <summary>
    /// Interaction logic for TagSettings.xaml
    /// </summary>
    public partial class TagSettings : Page
    {
        private Thread initCheck;

        public TagSettings() {
            InitializeComponent();
        }

        private void btnAcceptHiddenTags_Click( object sender, RoutedEventArgs e ) {
            Data.HiddenTags = tslHiddenTags.GetCheckedTagsIndicies();
            btnAcceptHiddenTags.IsEnabled = false;
        }

        private void tagSelector_TagChecked( object sender, TagCheckEventArgs e ) => btnAcceptHiddenTags.IsEnabled = true;

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            initCheck = new Thread( () =>
            {
                tslHiddenTags.ClearTags();
                tslHiddenTags.CheckTags( Data.HiddenTags );
                Dispatcher.Invoke( () => btnAcceptHiddenTags.IsEnabled = false );
            } );
            initCheck.Start();
            ckbShowHiddenTags.IsChecked = Data.ShowHiddenTags;
        }

        private void checkBox_Checked( object sender, RoutedEventArgs e ) {
            Data.ShowHiddenTags = true;
        }

        private void checkBox_Unchecked( object sender, RoutedEventArgs e ) {
            Data.ShowHiddenTags = false;
        }
    }
}
