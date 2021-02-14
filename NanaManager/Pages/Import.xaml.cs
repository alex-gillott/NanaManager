using NanaManager.Properties;
using NanaManagerAPI;
using NanaManagerAPI.IO;
using NanaManagerAPI.Media;
using NanaManagerAPI.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : Page
    {
        private string currentViewer = "";
        private int index = 0;
        internal static string Editing = null;

        internal readonly List<string> toImport = new List<string>();
        private readonly List<int[]> checkedTags = new List<int[]>();
        private int[] editTags = Array.Empty<int>();

        private Thread renderThread;
        private Thread importThread;

        public Import() {
            InitializeComponent();
        }

        #region Events

        private bool unimport = false;

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            foreach ( string s in Settings.Default.ToImport )
                if ( !toImport.Contains( s ) )
                    if ( File.Exists( s ) ) {
                        toImport.Add( s );
                        checkedTags.Add( scanForTags( s ) );
                    }
                    else if ( Directory.Exists( s ) )
                        foreach ( string f in scanDir( s ) ) {
                            toImport.Add( f );
                            checkedTags.Add( scanForTags( s ) );
                        }

            tslEditor.ShowHiddenTags = Data.ShowHiddenTags;
            index = toImport.Count - 1;
            btnBack.IsEnabled = false;
            if ( index == 0 )
                btnSkip.IsEnabled = false;
            else
                btnSkip.IsEnabled = true;

            bdrImporting.Visibility = Visibility.Collapsed;
            if ( Editing != null ) {
                editTags = Data.Media[Editing].GetTags();
                btnSkip.IsEnabled = false;
                btnUnimport.Content = "Delete";
                btnAdd.Content = "Done";
                cbxCopy.Visibility = Visibility.Collapsed;
            }
            else {
                btnUnimport.Content = "Don't Import";
                btnAdd.Content = "Add";
                cbxCopy.Visibility = Visibility.Visible;
            }
            renderIndex();
        }

        private void btnSkip_Click( object sender, RoutedEventArgs e ) {
            if ( checkedTags.Count > index )
                checkedTags[index] = tslEditor.GetCheckedTagsIndicies();
            index--;
            if ( index == 0 )
                btnSkip.IsEnabled = false;
            btnBack.IsEnabled = true;
            renderIndex();
        }

        private void btnBack_Click( object sender, RoutedEventArgs e ) {
            if ( checkedTags.Count > index )
                checkedTags[index] = tslEditor.GetCheckedTagsIndicies();
            index++;
            if ( index == toImport.Count - 1 )
                btnBack.IsEnabled = false;
            btnSkip.IsEnabled = true;
            renderIndex();
        }

        private void btnAdd_Click( object sender, RoutedEventArgs e ) {
            if ( Editing != null ) {
                Data.Media[Editing] = (IMedia)Registry.MediaConstructors[Registry.ExtensionConstructors[Data.Media[Editing].FileType]].Invoke( new object[] { Editing, tslEditor.GetCheckedTagsIndicies(), Data.Media[Editing].FileType } );
                Paging.LoadPreviousPage();
                return;
            }
            else {
                bdrImporting.Visibility = Visibility.Visible;

                importThread = new Thread( addEntry );
                importThread.Start();
            }
        }

        private void page_PreviewKeyDown( object sender, KeyEventArgs e ) {
            switch ( e.Key ) {
                case Key.Right:
                    if ( btnSkip.IsEnabled )
                        btnSkip_Click( sender, e );
                    break;

                case Key.Left:
                    if ( btnBack.IsEnabled )
                        btnBack_Click( sender, e );
                    break;
            }
        }

        private void btnCancel_Click( object sender, RoutedEventArgs e ) {
            if ( Editing != null ) {
                Editing = null;
                frmPreview.Content = null;
                Paging.LoadPreviousPage();
                return;
            }
            unimport = false;
            lblClarify.Content = "Are you sure you want to cancel?";
            lblProgress.Visibility = Visibility.Visible;
            bdrCancel.Visibility = Visibility.Visible;
        }

        private void btnUnimport_Click( object sender, RoutedEventArgs e ) {
            if ( Editing == null )
                lblClarify.Content = "Are you sure you don't want to import this image?";
            else
                lblClarify.Content = "Are you sure you want to delete this image?";
            unimport = true;
            lblProgress.Visibility = Visibility.Hidden;
            bdrCancel.Visibility = Visibility.Visible;
        }

        private void btnYes_Click( object sender, RoutedEventArgs e ) {
            bdrCancel.Visibility = Visibility.Collapsed;
            if ( unimport ) {
                if ( Editing == null ) {
                    toImport.RemoveAt( index );
                    checkedTags.RemoveAt( index );
                    Settings.Default.ToImport.RemoveAt( index );
                    if ( index > 0 )
                        index--;

                    if ( toImport.Count == 0 ) {
                        frmPreview.Content = null;
                        Paging.LoadPreviousPage();
                        return;
                    }
                    renderIndex();
                }
                else {
                    Data.Media.Remove( Editing );
                    ContentFile.Archive.GetEntry( Editing ).Delete();
                    frmPreview.Content = null;
                    Editing = null;
                    Paging.LoadPreviousPage();
                }
            }
            else {
                frmPreview.Content = null;
                Paging.LoadPreviousPage();
            }
        }

        private void btnNo_Click( object sender, RoutedEventArgs e ) => bdrCancel.Visibility = Visibility.Collapsed;

        private void btnManageTags_Click( object sender, RoutedEventArgs e ) {
            if ( Editing == null ) {
                checkedTags[index] = tslEditor.GetCheckedTagsIndicies();
            }
            Paging.LoadPage( Pages.TagManager );
        }

        #endregion Events

        #region Tags

        private void checkTags( int[] toCheck ) {
            renderThread = new Thread( () =>
            {
                tslEditor.ClearTags();
                tslEditor.CheckTags( toCheck );
            } );
            renderThread.Start();
        }

        #endregion Tags

        #region Importing

        private void addEntry() {
            bool saved = false;
            try {
                string uID = Guid.NewGuid().ToString();
                ContentFile.SetArchiveWrite();
                ContentFile.Archive.CreateEntryFromFile( toImport[index], uID );
                Data.Media.Add( uID, (IMedia)Registry.MediaConstructors[Registry.ExtensionConstructors[Path.GetExtension( toImport[index] )]].Invoke( new object[] { uID, tslEditor.GetCheckedTagsIndicies(), Path.GetExtension( toImport[index] ) } ) ); ;

                ContentFile.SaveData();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                saved = true;

                Dispatcher.Invoke( () =>
                 {
                     tslEditor.UncheckTags();

                     string toDelete = toImport[index];
                     toImport.RemoveAt( index );
                     checkedTags.RemoveAt( index );
                     Settings.Default.ToImport.RemoveAt( index );
                     if ( index > 0 )
                         btnSkip_Click( null, null );

                     if ( cbxCopy.IsChecked == false )
                         File.Delete( toDelete );

                     if ( toImport.Count == 0 ) {
                         frmPreview.Content = null;
                         currentViewer = "";
                         Paging.LoadPreviousPage();
                     }
                     bdrImporting.Visibility = Visibility.Collapsed;
                 } );
            } catch ( Exception ex ) {
                Logging.Write( ex, "Import" );
                if ( Debugger.IsAttached )
                    throw ex;

                if ( saved )
                    MessageBox.Show( "Could not remove file from original position. Data saved" );
                else
                    MessageBox.Show( "Could not save data. The error was recorded and your media was recovered" );
            }
        }

        private void renderIndex() {
            if ( Editing != null ) {
                IMedia media = Data.Media[Editing];
                string fileType = media.FileType;
                if ( Registry.SupportedDataTypes.ContainsKey( fileType ) ) {
                    string toLoad = Registry.SupportedDataTypes[fileType];
                    IMediaViewer viewer = Registry.Viewers[toLoad];
                    viewer.LoadMedia( Editing, true );
                    if ( currentViewer != toLoad )
                        frmPreview.Content = viewer.Display;
                }
                lblIndex.Content = "";
                checkTags( editTags.ToArray() );
            }
            else {
                string path = toImport[index];
                string ext = Path.GetExtension( path ).ToLower();
                if ( Registry.SupportedDataTypes.ContainsKey( ext ) ) {
                    try {
                        string toLoad = Registry.SupportedDataTypes[ext];
                        IMediaViewer viewer = Registry.Viewers[toLoad];
                        viewer.LoadMedia( path, false );
                        if ( currentViewer != toLoad )
                            Dispatcher.Invoke( () => frmPreview.Content = viewer.Display );
                        lblIndex.Content = $"{toImport.Count - index}/{toImport.Count}";
                        checkTags( checkedTags[index].ToArray() );
                    } catch ( Exception ) {
                        if ( Debugger.IsAttached )
                            throw;
                        //TODO - HANDLE APPROPRIATE ERRORS AND LOG THEM
                        MessageBox.Show( $"The file at \"{path}\" could not be read. Please check if it's still there, and if it can be opened" );
                        toImport.RemoveAt( index );
                        checkedTags.RemoveAt( index );
                        if ( index > 0 )
                            index--;

                        if ( toImport.Count == 0 ) {
                            frmPreview.Content = null;
                            Paging.LoadPreviousPage();
                            return;
                        }
                        renderIndex();
                    }
                }
                else {
                    //TODO - HANDLE INVALID FILE TYPE
                    //Announce "Do not have a handler for that file type! ({ext})
                }
            }
        }

        private List<string> scanDir( string dir ) {
            List<string> toAppend = new List<string>();
            foreach ( string s in Directory.GetFiles( dir ) )
                if ( Registry.SupportedDataTypes.ContainsKey( Path.GetExtension( s ) ) )
                    toAppend.Add( s );
            foreach ( string s in Directory.GetDirectories( dir ) )
                foreach ( string f in scanDir( s ) )
                    toAppend.Add( f );

            return toAppend;
        }

        private int[] scanForTags( string name ) {
            string smolName = name.ToLower();
            List<int> found = new List<int>();
            foreach ( KeyValuePair<int, int> kvp in Data.TagLocations )
                if ( smolName.Contains( Data.Tags[kvp.Value].Name.ToLower() ) )
                    found.Add( kvp.Value );
            return found.ToArray();
        }

        #endregion Importing
    }
}
