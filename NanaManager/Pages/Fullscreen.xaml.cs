using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

using NanaManagerAPI.Media;
using NanaManagerAPI.UI;
using NanaManagerAPI;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Fullscreen.xaml
    /// </summary>
    public partial class Fullscreen : Page
    {
        private string[] searched;
        private int idx;
        private string current;
        private readonly Storyboard hideMenu;
        private bool editing = false;

        public Fullscreen() {
            InitializeComponent();
            hideMenu = Resources["styHideMenu"] as Storyboard;
        }

        private void load(string path) {
            current = path;

            IMedia media = Data.Media[path];
            IMediaViewer viewer = Registry.Viewers[Registry.SupportedDataTypes[media.FileType]];
            viewer.LoadMedia( path, true );
            frmViewer.Content = viewer.Display;

            stkTags.Children.Clear();
            foreach ( int i in media.GetTags() ) {
                ToggleButton tb = new ToggleButton() { Content = Data.Tags[Data.TagLocations[i]].Name, Style = (Style)Application.Current.Resources["Tag Button"], Tag = i };
                tb.Checked += handleTagClick;
                stkTags.Children.Add(tb);
            }
        }

        /// <summary>
        /// Opens the fullscreen viewer with the specified piece of media, along with the search terms used
        /// </summary>
        /// <param name="Current">The media to open with</param>
        /// <param name="Search">The search term to use when seeking</param>
        /// <param name="Index">Where in the list the media is</param>
        public void OpenViewer(string Current, int[] Search, int[] Rejected, int Index) {

            searched = Data.SearchForAll( Search, Rejected );

            btnPrev.IsEnabled = rctPrev.IsEnabled = Index != 0;
            btnNext.IsEnabled = rctNext.IsEnabled = Index != searched.Length - 1 && searched.Length > 0;

            load( Current );
            idx = Index;
        }

        //TODO - Work out how to make this handle 
        private void page_PreviewKeyDown( object sender, KeyEventArgs e ) {
            switch (e.Key) {
                case Key.Escape:
                    UI.SetFullscreen( false );
                    Paging.LoadPreviousPage();
                    break;
                case Key.Left:
                    previous();
                    break;
                case Key.Right:
                    next();
                    break;
            }
        }
        private void exit_Click( object sender, RoutedEventArgs e ) {
            //UI.SetFullscreen( false );
            Paging.LoadPreviousPage();
        }

        private void grdMenu_MouseLeave( object sender, MouseEventArgs e ) {
            if ( !scrTags.IsMouseOver ) {
                btnTags.IsChecked = false;
                hideMenu.Begin( grdMenu, true );
            }
        }
        private void scrTags_MouseLeave( object sender, MouseEventArgs e ) {
            btnTags.IsChecked = false;
            if ( !grdMenu.IsMouseOver )
                hideMenu.Begin( grdMenu, true );
        }
        private void handleTagClick( object sender, RoutedEventArgs e) {
            Search.SearchTags = new int[] { (int)((ToggleButton)sender).Tag };
            Paging.LoadPage( Pages.Viewer );
        }

        private void edit_Click( object sender, RoutedEventArgs e ) {
            //UI.SetFullscreen( false );
            Import.Editing = current;
            Paging.LoadPage( Pages.Import );
            editing = true;
        }

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            UI.SetFullscreen( true );
            if ( editing ) {
                if ( Data.Media.ContainsKey( current ) )
                    load( current );
                else
                    Paging.LoadPreviousPage();
                editing = false;
            }
        }

        private void previous() {
            if ( idx > 0 ) {
                idx--;
                load( searched[idx] );
                btnNext.IsEnabled = rctNext.IsEnabled = true;
                btnPrev.IsEnabled = rctPrev.IsEnabled = idx > 0;
            }  
        }

        private void next() {
            if ( idx < searched.Length - 1) {
                idx++;
                load( searched[idx] );
                btnPrev.IsEnabled = rctPrev.IsEnabled = true;
                btnNext.IsEnabled = rctNext.IsEnabled = idx < searched.Length - 1;
            }
        }

        private void btnPrev_Click( object sender, RoutedEventArgs e ) {
            previous();
        }

        private void btnNext_Click( object sender, RoutedEventArgs e ) {
            next();
        }

        private void frmViewer_Loaded( object sender, RoutedEventArgs e ) {
            ((Frame)sender).Focus();
        }

        private void Page_Unloaded( object sender, RoutedEventArgs e ) {
            UI.SetFullscreen( false );
        }
    }
}
