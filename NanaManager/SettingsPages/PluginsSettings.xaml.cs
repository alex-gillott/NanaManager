using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NanaManagerAPI.UI;

namespace NanaManager.SettingsPages
{
    /// <summary>
    /// Interaction logic for PluginsSettings.xaml
    /// </summary>
    public partial class PluginsSettings : Page
    {
        public PluginsSettings() {
            InitializeComponent();
        }

        private void resetButtons() {
            foreach ( object c in grdSettings.Children ) //Can easily add new buttons, in case philosophy changes or 
                if ( c is ToggleButton button ) //          a new settings group is made. This just means we don't
                    button.IsChecked = false;//             have to add a new manual set each time.
        }

        private void btnBack_Checked( object sender, RoutedEventArgs e ) {
            resetButtons();
            Paging.LoadPreviousPage();
        }
    }
}
