using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.IO.Compression;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

using NanaManagerAPI;
using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI.Data;
using NanaManagerAPI.Media;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : Page
    {
        public Viewer() {
            InitializeComponent();

            StringBuilder filter = new StringBuilder( "Compatible Files|" );
            foreach ( KeyValuePair<string, string> s in NanaManagerAPI.UI.Registry.SupportedDataTypes )
                filter.Append( $"*{s.Key};" );
            filter.Remove( filter.Length - 1, 1 );

            foreach ( KeyValuePair<string, List<string>> c in NanaManagerAPI.UI.Registry.Categories ) {
                filter.Append( $"|{c.Key}|" );
                foreach ( string s in c.Value )
                    filter.Append( $"*{s};" );
                filter.Remove( filter.Length - 1, 1 );
            }

            ofd.Filter = filter.ToString();
            ofd.Multiselect = true;
        }

        #region VariablesInit
        public static List<int> LoadedIndices = new List<int>();

        private readonly OpenFileDialog ofd = new OpenFileDialog();

        private bool menuOpen = false;

        private int[] searchTags = Array.Empty<int>();
        #endregion

        #region MenuButton
        private void image_MouseUp( object sender, MouseButtonEventArgs e ) {
            elpMenuButton.Fill = (Brush)Application.Current.Resources["MenuButtonHighlight"];
            if ( menuOpen )
                ((Storyboard)Resources["stbCloseMenu"]).Begin();
            else
                ((Storyboard)Resources["stbOpenMenu"]).Begin();
            menuOpen = !menuOpen;
        }
        private void image_MouseLeave( object sender, MouseEventArgs e ) => elpMenuButton.Fill = (Brush)Application.Current.Resources["MenuButtonIdle"];
        private void image_MouseEnter( object sender, MouseEventArgs e ) => elpMenuButton.Fill = (Brush)Application.Current.Resources["MenuButtonHighlight"];
        private void image_MouseDown( object sender, MouseButtonEventArgs e ) => elpMenuButton.Fill = (Brush)Application.Current.Resources["MenuButtonPressed"];
        #endregion

        #region ImageDisplay
        private void fillListView( int[] tags ) {
            lstImages.Items.Clear();

            foreach ( KeyValuePair<string, NanaManagerAPI.Media.IMedia> i in Globals.Media ) {
                bool fits = true;
                int[] t = i.Value.GetTags();
                foreach ( int tag in tags )
                    if ( !t.Contains( tag ) ) {
                        fits = false;
                        break;
                    }
                if ( fits )
                    lstImages.Items.Add( new System.Windows.Controls.Image() { Tag = i.Key, Source = i.Value.GetSample(), Width = 100, Height = 100 } );
            }
        }
        #endregion

        #region Search
        private readonly List<int> checkedItems = new List<int>();

        private void scanTags( string filter ) {
            lstTags.Items.Clear();
            if ( string.IsNullOrEmpty( filter ) )
                foreach ( Tag t in TagData.Tags ) {
                    CheckBox cbx = new CheckBox() { Content = t.Name, IsChecked = checkedItems.Contains( t.Index ), Foreground = (Brush)Application.Current.Resources["LightText"], Tag = t.Index };
                    cbx.Click += ( sender, e ) =>
                    {
                        int idx = (int)((CheckBox)sender).Tag;
                        if ( cbx.IsChecked == true && !checkedItems.Contains( idx ) )
                            checkedItems.Add( idx );
                        else if ( cbx.IsChecked == false && checkedItems.Contains( idx ) )
                            checkedItems.Remove( idx );
                    };
                    lstTags.Items.Add( cbx );
                }
            else
                foreach ( Tag t in TagData.Tags )
                    if ( t.Name.Contains( filter ) ) {
                        CheckBox cbx = new CheckBox() { Content = t.Name, IsChecked = checkedItems.Contains( t.Index ), Foreground = (Brush)Application.Current.Resources["LightText"], Tag = t.Index };
                        cbx.Click += ( sender, e ) =>
                        {
                            int idx = (int)((CheckBox)sender).Tag;
                            if ( cbx.IsChecked == true && !checkedItems.Contains( idx ) )
                                checkedItems.Add( idx );
                            else if ( cbx.IsChecked == false && checkedItems.Contains( idx ) )
                                checkedItems.Remove( idx );
                        };
                        lstTags.Items.Add( cbx );
                    }
        }

        private void btnSearch_Click( object sender, RoutedEventArgs e ) {
            //TODO - SEARCH
        }
        private void txtSearch_TextChanged( object sender, TextChangedEventArgs e ) => scanTags( txtSearch.Text );
        #endregion

        #region Events
        private void page_Loaded( object sender, RoutedEventArgs e ) {
            ((Storyboard)Resources["stbCloseMenuFast"]).Begin();
            menuOpen = false;
            scanTags( "" );
            lstImages.Dispatcher.BeginInvoke( new Action( () => fillListView( Array.Empty<int>() ) ) );
        }
        private void btnImport_Click( object sender, RoutedEventArgs e ) {
            bool? import = ofd.ShowDialog();
            if ( import == true ) {
                Properties.Settings.Default.ToImport.AddRange( ofd.FileNames );
                Import.Editing = null;
                Paging.LoadPage( Pages.Import );
            }
        }
        private void lstImages_MouseDoubleClick( object sender, MouseButtonEventArgs e ) {
            if ( lstImages.SelectedIndex > -1 ) {
                Globals.OpenMedia( (string)((System.Windows.Controls.Image)lstImages.SelectedItem).Tag, searchTags, lstImages.SelectedIndex );
                Globals.SetFullscreen( true );
                Paging.LoadPage( Pages.Fullscreen );
            }
        }
        private void btnEdit_Click( object sender, RoutedEventArgs e ) {
            Import.Editing = (string)((System.Windows.Controls.Image)lstImages.SelectedItem).Tag;
            Paging.LoadPage( Pages.Import );
        }
        private void btnExport_Click( object sender, RoutedEventArgs e ) {
            if ( lstImages.SelectedIndex > -1 ) {
                IMedia m = Globals.Media[(string)((System.Windows.Controls.Image)lstImages.SelectedItem).Tag];
                File.WriteAllBytes( Path.Combine( ContentFile.ExportPath, m.ID + m.FileType ), ContentFile.ReadFile( m.ID ) );
                Process.Start( ContentFile.ExportPath );
            }
        }
        private void lstImages_PreviewMouseDown( object sender, MouseButtonEventArgs e ) => lstImages.SelectedIndex = -1;
        private void lstImages_SelectionChanged( object sender, SelectionChangedEventArgs e ) => btnEdit.IsEnabled = btnExport.IsEnabled = lstImages.SelectedIndex != -1;
        private void btnTags_Click( object sender, RoutedEventArgs e ) => Paging.LoadPage( Pages.TagManager );
        private void btnSettings_Click( object sender, RoutedEventArgs e ) => Paging.LoadPage( Pages.Settings );
        #endregion

    }
}