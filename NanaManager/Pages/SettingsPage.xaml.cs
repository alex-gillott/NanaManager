using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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
            tbtLang.Tag = Pages.LanguagesSettings;
            tbtTaC.Tag = Pages.ThemesAndColoursSettings; //In case the IDs ever change, these are set so that they don't become invalid
            tbtAdv.Tag = Pages.AdvancedSettings;
            tbtTags.Tag = Pages.TagsSettings;
        }

        private void resetButtons() {
            foreach ( object c in grdSettings.Children ) //Can easily add new buttons, in case philosophy changes or 
                if ( c is ToggleButton button ) //          a new settings group is made. This just means we don't
                    button.IsChecked = false;//             have to add a new manual set each time.
        }

        private void toggleButton_Click( object sender, RoutedEventArgs e ) {
            resetButtons();
            ToggleButton b = sender as ToggleButton;
            b.IsChecked = true;
            SettingsTab set = Registry.SettingsTabs[Pages.InvalidSettings];
                if ( Registry.SettingsTabs.ContainsKey( b.Tag as string ) )
                set = Registry.SettingsTabs[b.Tag as string];
            frmSettings.Content = set.Display;
            lblTitle.Content = set.Title;
        }

        private void button_Click( object sender, RoutedEventArgs e ) {
            Paging.LoadPreviousPage();
        }

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            toggleButton_Click( tbtTaC, null );
        }

        private void toggleButton_Click_1( object sender, RoutedEventArgs e ) {
            Paging.LoadPage( Pages.PluginsSettings );
        }
    }
}