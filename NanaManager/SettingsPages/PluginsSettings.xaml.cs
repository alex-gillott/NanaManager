using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly List<string> pages = new List<string>();
        private int starting = 0;
        private int upperBound = 0;

        private readonly ToggleButton[] buttons; //Used to simplify iteration for loading plugins

        public PluginsSettings() {
            InitializeComponent();

            buttons = new ToggleButton[] { btnT1, btnT2, btnT3, btnT4, btnT5, btnT6, btnT7 };
        }

        private void loadPlugins() {
            pages.Clear();
            foreach ( string st in Registry.SettingsTabs.Keys ) //How the hell did I not know about this before!?
                if ( !st.Contains( "nanaManager" ) )
                    pages.Add( st );

            if ( pages.Count > 7 ) {
                upperBound = pages.Count - 5;
                starting = 0;
                btnT1.Tag = "hydroxa.nanaManager:cmd_prev";
                btnT7.Tag = "hydroxa.nanaManager:cmd_next";

                btnT1.IsEnabled = false;
                btnT7.IsEnabled = true;

                loadButtons( true );
            }
            else
                loadButtons( false );
        }

        private void loadButtons( bool limited ) {
            if ( limited ) {
                for ( int i = 0; i < 5; i++ ) {
                    string id = pages[starting + i];
                    ToggleButton btn = buttons[i + 1];
                    btn.Content = Registry.SettingsTabs[id].Title;
                    btn.Tag = id;
                }
            } else {
                for (int i = 0; i < pages.Count; i++ ) {
                    ToggleButton btn = buttons[i];
                    string id = pages[i];
                    btn.Content = Registry.SettingsTabs[id].Title;
                    btn.Tag = id;
                }
                for (int i = pages.Count; i < 7; i++ ) {
                    buttons[i].IsEnabled = false;
                }
            }
        }

        private void handlePluginClick( object sender, RoutedEventArgs e ) {
            string id = (string)((ToggleButton)sender).Tag;
            switch (id) {
                case "hydroxa.nanaManager:cmd_prev":
                    if ( starting > 0 ) {
                        starting--;
                        loadButtons( true );
                        btnT7.IsEnabled = true;
                        if ( starting == 0 )
                            btnT1.IsEnabled = false;
                    }
                    break;
                case "hydroxa.nanaManager:cmd_next":
                    if ( starting < upperBound ) {
                        starting++;
                        loadButtons( true );
                        btnT1.IsEnabled = true;
                        if ( starting == upperBound )
                            btnT7.IsEnabled = false;
                    }
                    break;
                default:
                    frmSettings.Content = Registry.SettingsTabs[id].Display;
                    break;
            }
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

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            loadPlugins();
        }
    }
}