using Microsoft.Win32;
using NanaManagerAPI;
using NanaManagerAPI.IO;
using NanaManagerAPI.Media;
using NanaManagerAPI.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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

        #endregion VariablesInit

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

        #endregion MenuButton

        #region ImageDisplay

        private void fillListView( int[] tags, int[] reject ) {
            lstImages.Items.Clear();

            stkTags.Children.Clear();
            foreach ( int t in tags ) {
                Border bd = new Border()
                {
                    CornerRadius = new CornerRadius( 8 ),
                    Background = (Brush)Application.Current.Resources["Highlight"],
                    Margin = new Thickness( 2 )
                };
                Label lb = new Label()
                {
                    Foreground = (Brush)Application.Current.Resources["LightText"],
                    Content = Data.Tags[Data.TagLocations[t]].Name,
                    FontSize = 11,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                bd.Child = lb;
                stkTags.Children.Add( bd );
            }
            foreach ( int t in reject ) {
                Border bd = new Border()
                {
                    CornerRadius = new CornerRadius( 8 ),
                    Background = (Brush)Application.Current.Resources["TagRejectColor"],
                    Margin = new Thickness( 2 )
                };
                Label lb = new Label()
                {
                    Foreground = (Brush)Application.Current.Resources["LightText"],
                    Content = Data.Tags[Data.TagLocations[t]].Name,
                    FontSize = 11,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                bd.Child = lb;
                stkTags.Children.Add( bd );
            }

            ((TranslateTransform)scrTags.RenderTransform).Y = -300;

            if ( stkTags.Children.Count == 0 )
                bdrSearch.Visibility = Visibility.Collapsed;
            else
                bdrSearch.Visibility = Visibility.Visible;

            foreach ( string id in Data.SearchForAll( tags, reject ) )
                lstImages.Items.Add( new System.Windows.Controls.Image() { Tag = id, Source = Data.Media[id].GetSample(), Width = 100, Height = 100 } );
        }

        #endregion ImageDisplay

        #region Events

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            ((Storyboard)Resources["stbCloseMenuFast"]).Begin();
            menuOpen = false;
            lstImages.Dispatcher.BeginInvoke( new Action( () => fillListView( Search.SearchTags, Search.RejectedTags ) ) );
        }

        private void btnImport_Click( object sender, RoutedEventArgs e ) {
            bool? import = ofd.ShowDialog();
            if ( import == true ) {
                Properties.Settings.Default.ToImport.AddRange( ofd.FileNames );
                Import.Editing = null;
                Paging.LoadPage( Pages.Import );
            }
        }

        private void btnSearch_Click( object sender, RoutedEventArgs e ) => Paging.LoadPage( Pages.Search );

        private void lstImages_MouseDoubleClick( object sender, MouseButtonEventArgs e ) {
            if ( lstImages.SelectedIndex > -1 ) {
                UI.OpenMedia( (string)((System.Windows.Controls.Image)lstImages.SelectedItem).Tag, Search.SearchTags, Search.RejectedTags, lstImages.SelectedIndex );
                UI.SetFullscreen( true );
                Paging.LoadPage( Pages.Fullscreen );
            }
        }

        private void btnEdit_Click( object sender, RoutedEventArgs e ) {
            Import.Editing = (string)((System.Windows.Controls.Image)lstImages.SelectedItem).Tag;
            Paging.LoadPage( Pages.Import );
        }

        private void btnExport_Click( object sender, RoutedEventArgs e ) {
            if ( lstImages.SelectedIndex > -1 ) {
                IMedia m = Data.Media[(string)((System.Windows.Controls.Image)lstImages.SelectedItem).Tag];
                File.WriteAllBytes( Path.Combine( ContentFile.ExportPath, m.ID + m.FileType ), ContentFile.ReadFile( m.ID ) );
                Process.Start( ContentFile.ExportPath );
            }
        }

        private void lstImages_PreviewMouseDown( object sender, MouseButtonEventArgs e ) => lstImages.SelectedIndex = -1;

        private void lstImages_SelectionChanged( object sender, SelectionChangedEventArgs e ) => btnEdit.IsEnabled = btnExport.IsEnabled = lstImages.SelectedIndex != -1;

        private void btnTags_Click( object sender, RoutedEventArgs e ) => Paging.LoadPage( Pages.TagManager );

        private void btnSettings_Click( object sender, RoutedEventArgs e ) => Paging.LoadPage( Pages.Settings );

        #endregion Events
    }
}