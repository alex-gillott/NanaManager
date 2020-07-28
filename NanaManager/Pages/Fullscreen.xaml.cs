using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

using NanaManager.MediaHandlers;
using NanaManagerAPI.Media;
using NanaManagerAPI;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Fullscreen.xaml
    /// </summary>
    public partial class Fullscreen : Page
    {
        int[] searchTerms;
        string[] searched;
        int idx;
        string current;
        Storyboard hideMenu;
        bool editing = false;

        public Fullscreen() {
            InitializeComponent();
            hideMenu = Resources["styHideMenu"] as Storyboard;
        }

        private void load(string path) {
            current = path;

            IMedia media = Globals.Media[path];
            IMediaViewer viewer = Globals.Viewers[Globals.SupportedDataTypes[media.FileType]];
            viewer.LoadMedia( path, true );
            frmViewer.Content = viewer.Display;

            stkTags.Children.Clear();
            foreach ( int i in media.GetTags() ) {
                ToggleButton tb = new ToggleButton() { Content = TagData.Tags[TagData.TagLocations[i]].Name, Style = (Style)Application.Current.Resources["Tag Button"]};
                stkTags.Children.Add(tb);
            }
        }

        /// <summary>
        /// Opens the fullscreen viewer with the specified piece of media, along with the search terms used
        /// </summary>
        /// <param name="Current">The media to open with</param>
        /// <param name="Search">The search term to use when seeking</param>
        /// <param name="Index">Where in the list the media is</param>
        public void OpenViewer(string Current, int[] Search, int Index) {
            searchTerms = Search;
            searched = TagData.SearchForAll( Search );

            btnPrev.IsEnabled = rctPrev.IsEnabled = Index != 0;
            btnNext.IsEnabled = rctNext.IsEnabled = Index != searched.Length - 1 && searched.Length > 0;

            load( Current );
            idx = Index;
        }

        //TODO - Work out how to make this handle 
        private void Page_PreviewKeyDown( object sender, KeyEventArgs e ) {
            switch (e.Key) {
                case Key.Escape:
                    Globals.SetFullscreen( false );
                    Paging.LoadPreviousPage();
                    break;
            }
        }
        private void Exit_Click( object sender, RoutedEventArgs e ) {
            Globals.SetFullscreen( false );
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
            //TODO - Link to Searching
        }

        private void Edit_Click( object sender, RoutedEventArgs e ) {
            Globals.SetFullscreen( false );
            Import.Editing = current;
            Paging.LoadPage( Pages.Import );
            editing = true;
        }

        private void Page_Loaded( object sender, RoutedEventArgs e ) {
            Globals.SetFullscreen( true );
            if ( editing ) {
                if ( Globals.Media.ContainsKey( current ) )
                    load( current );
                else
                    Paging.LoadPreviousPage();
                editing = false;
            }
        }

        private void Previous() {
            if ( idx > 0 ) {
                idx--;
                load( searched[idx] );
                btnNext.IsEnabled = rctNext.IsEnabled = true;
                btnPrev.IsEnabled = rctPrev.IsEnabled = idx > 0;
            }  
        }

        private void Next() {
            if ( idx < searched.Length - 1) {
                idx++;
                load( searched[idx] );
                btnPrev.IsEnabled = rctPrev.IsEnabled = true;
                btnNext.IsEnabled = rctNext.IsEnabled = idx < searched.Length - 1;
            }
        }

        private void grdMenu_MouseDown( object sender, MouseButtonEventArgs e ) {
        }

        private void btnPrev_Click( object sender, RoutedEventArgs e ) {
            Previous();
        }

        private void btnNext_Click( object sender, RoutedEventArgs e ) {
            Next();
        }
    }
}
