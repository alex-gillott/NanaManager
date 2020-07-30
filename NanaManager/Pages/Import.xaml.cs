using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Input;
using System.IO.Compression;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

using NanaManager.Properties;
using NanaManagerAPI.Media;
using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI;

namespace NanaManager
{
	/// <summary>
	/// Interaction logic for Import.xaml
	/// </summary>
	public partial class Import : Page
	{
		string currentViewer = "";
		int index = 0;
		internal static string Editing = null;

		internal readonly List<string> toImport = new List<string>();
		private readonly List<List<int>> checkedTags = new List<List<int>>();
		private List<int> editTags = new List<int>();

		public Import() {
			InitializeComponent();
		}

		#region Events
		bool unimport = false;
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

			//UIElement[] toClear = new UIElement[stkTags.Children.Count];
			//stkTags.Children.CopyTo( toClear, 0 );
			//foreach ( GroupBox gb in toClear )
			//	if ( (string)gb.Header != "Misc" )
			//		stkTags.Children.Remove( gb );

			//generateGroups();
			//lsbMisc.Items.Clear();

			//foreach ( NanaManagerAPI.Data.Tag t in TagData.Tags )
			//	if ( !TagData.HiddenTags.Contains( t.Index ) || Globals.ShowHiddenTags )
			//		addTag( t.Index );

			index = toImport.Count - 1;
			btnBack.IsEnabled = false;
			if ( index == 0 )
				btnSkip.IsEnabled = false;
			else
				btnSkip.IsEnabled = true;

			if ( Editing != null ) {
				editTags = Globals.Media[Editing].GetTags().ToList();
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
			index--;
			if ( index == 0 )
				btnSkip.IsEnabled = false;
			btnBack.IsEnabled = true;
			renderIndex();
		}
		private void btnBack_Click( object sender, RoutedEventArgs e ) {
			index++;
			if ( index == toImport.Count - 1 )
				btnBack.IsEnabled = false;
			btnSkip.IsEnabled = true;
			renderIndex();
		}
		private void btnAdd_Click( object sender, RoutedEventArgs e ) {
			using ZipArchive archive = ZipFile.Open( ContentFile.ContentPath, ZipArchiveMode.Update );
			bool saved = false;
			if ( Editing != null ) {
				Globals.Media[Editing] = (IMedia)Registry.MediaConstructors[Globals.Media[Editing].FileType].Invoke( new object[] { Editing, tslEditor.GetCheckedTagsIndicies(), Globals.Media[Editing].FileType } );
				Paging.LoadPreviousPage();
				return;
			}
			else {
				try {
					byte[] data = File.ReadAllBytes( toImport[index] );
					string uID = Guid.NewGuid().ToString();
					archive.CreateEntryFromFile( toImport[index], uID );
					Globals.Media.Add( uID, new NanaManagerAPI.Data.Image( uID, tslEditor.GetCheckedTagsIndicies(), Path.GetExtension( toImport[index] ) ) );;

					saved = true;

					tslEditor.UncheckTags();

					string toDelete = toImport[index];
					toImport.RemoveAt( index );
					checkedTags.RemoveAt( index );
					if ( index > 0 )
						btnSkip_Click( sender, e );

					if ( cbxCopy.IsChecked == false )
						File.Delete( toDelete );

					if ( toImport.Count == 0 ) {
						frmPreview.Content = null;
						currentViewer = "";
						Paging.LoadPreviousPage();
					}
				} catch ( Exception ex ) {
					if ( Debugger.IsAttached )
						throw ex;
					//TODO - RECORD ERROR DATA
					if ( saved )
						MessageBox.Show( "Could not remove file from original position. Data saved" );
					else
						MessageBox.Show( "Could not save data. The error was recorded and your image was recovered" );
				}
			}
		}
		private void Page_PreviewKeyDown( object sender, KeyEventArgs e ) {
			switch (e.Key)
			{
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
					Globals.Media.Remove( Editing );
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
		private void btnManageTags_Click( object sender, RoutedEventArgs e ) => Paging.LoadPage( Pages.TagManager );
		#endregion

		#region Tags
		///// <summary>
		///// Sets whether each tag is ticked or not
		///// </summary>
		//private void checkTags() {
		//	List<int> toCheck;
		//	if ( Editing != null )
		//		toCheck = editTags;
		//	else
		//		toCheck = checkedTags[index];
		//	foreach ( int i in toCheck )
		//		foreach ( ToggleButton t in getListBox( TagData.Tags[TagData.TagLocations[i]].Group ).Items )
		//			if ( (int)t.Tag == i ) {
		//				t.IsChecked = true;
		//				if ( Editing == null && !checkedTags[index].Contains( i ) )
		//					checkedTags[index].Add( i );
		//				else if ( !editTags.Contains( i ) )
		//					editTags.Add( i );
		//			}
		//}
		//private void checkTag( int tag ) {
		//	foreach ( ToggleButton t in getListBox( TagData.Tags[TagData.TagLocations[tag]].Group ).Items )
		//		if ( (int)t.Tag == tag ) {
		//			t.IsChecked = true;
		//			if ( Editing == null && !checkedTags[index].Contains( tag ) )
		//				checkedTags[index].Add( tag );
		//			else if ( !editTags.Contains( tag ) )
		//				editTags.Add( tag );
		//		}
		//}

		//private void generateGroups() {
		//	foreach ( string s in TagData.Groups ) {
		//		GroupBox gb = new GroupBox() { Header = s, Style = (Style)Application.Current.Resources["Tag Groupbox"], Margin = new Thickness( 10, 0, 0, 10 ), Width = 171, FontSize = 18 };
		//		ListBox content = new ListBox() { Background = Theme.Transparent, BorderThickness = new Thickness( 0 ), Foreground = (Brush)Application.Current.Resources["DarkText"], HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch };
		//		content.SelectionChanged += ( _, _ ) => content.SelectedIndex = -1;
		//		gb.Content = content;
		//		stkTags.Children.Add( gb );
		//	}
		//}
		//private void addTag( int t ) {
		//	ToggleButton tag = new ToggleButton() { Content = TagData.Tags[TagData.TagLocations[t]].Name, Style = (Style)Application.Current.Resources["Tag Button"], Tag = t };
		//	tag.Checked += ( s, ev ) =>
		//	{
		//		int id = (int)((ToggleButton)s).Tag;
		//		if ( Editing == null && !checkedTags[index].Contains( id ) ) {
		//			checkedTags[index].Add( id );
		//			foreach ( int t in TagData.Tags[TagData.TagLocations[id]].GetAliases() )
		//				checkTag( t );
		//		}
		//		else if ( !editTags.Contains( id ) ) {
		//			editTags.Add( id );
		//			foreach ( int t in TagData.Tags[TagData.TagLocations[id]].GetAliases() )
		//				checkTag( t );
		//		}
		//	};
		//	tag.Unchecked += ( s, ev ) =>
		//	{
		//		int id = (int)((ToggleButton)s).Tag;
		//		if ( Editing == null && checkedTags[index].Contains( id ) )
		//			checkedTags[index].Remove( id );
		//		else if ( editTags.Contains( id ) )
		//			editTags.Remove( id );
		//	};
		//	getListBox( TagData.Tags[TagData.TagLocations[t]].Group ).Items.Add( tag );
		//}
		///// <summary>
		///// Returns the group under the specified index
		///// </summary>
		///// <param name="index">The internal index of the group. Use -1 for Misc</param>
		//private ListBox getListBox( int index ) => (stkTags.Children[index + 1] as GroupBox).Content as ListBox;
		private void checkTags(int[] toCheck) {
			tslEditor.ClearTags();
			tslEditor.CheckTags( toCheck );
        }
		#endregion

		#region Importing
		private void renderIndex() {
			if ( Editing != null ) {
				IMedia media = Globals.Media[Editing];
				string fileType = media.FileType;
				if ( Registry.SupportedDataTypes.ContainsKey( fileType ) ) {
					string toLoad = Registry.SupportedDataTypes[fileType];
					IMediaViewer viewer = Registry.Viewers[toLoad];
					viewer.LoadMedia( Editing, true );
					if ( currentViewer != toLoad )
						frmPreview.Content = viewer.Display;
				}
				lblIndex.Content = "";
				checkTags(media.GetTags());
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
							frmPreview.Content = viewer.Display;
						lblIndex.Content = $"{toImport.Count - index}/{toImport.Count}";
						checkTags(checkedTags[index].ToArray());
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
		private List<int> scanForTags( string name ) {
			string smolName = name.ToLower();
			List<int> found = new List<int>();
			for ( int i = 0; i < TagData.Tags.Length; i++ )
				if ( smolName.Contains( TagData.Tags[TagData.TagLocations[i]].Name.ToLower() ) )
					found.Add( i );
			return found;
		}
		#endregion
	}
}