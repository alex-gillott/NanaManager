using NanaManagerAPI;
using NanaManagerAPI.EventArgs;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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