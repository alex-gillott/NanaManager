using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using NanaManager.SettingsPages;
using NanaManagerAPI.UI;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage() {
            InitializeComponent();
        }

        private void resetButtons() {
            foreach ( object c in grdSettings.Children ) //Can easily add new buttons, in case philosophy changes or 
                if ( c is ToggleButton button ) //          a new settings group is made. This just means we don't
                    button.IsChecked = false;//             have to add a new manual set each time.
        }

        private void ToggleButton_Click( object sender, RoutedEventArgs e ) {
            resetButtons();
            ToggleButton b = sender as ToggleButton;
            b.IsChecked = true;
            SettingsTab set = Registry.SettingsTabs[Pages.InvalidSettings];
            if ( Registry.SettingsTabs.ContainsKey( b.Tag as string ) )
                set = Registry.SettingsTabs[b.Tag as string];
            frmSettings.Content = set.Display;
            lblTitle.Content = set.Title;
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            Paging.LoadPreviousPage();
        }

        private void Page_Loaded( object sender, RoutedEventArgs e ) {
            ToggleButton_Click( btnTaC, null );
        }
    }
}