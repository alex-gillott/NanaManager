using System;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

using NanaManagerAPI.Data;
using NanaManagerAPI;

namespace NanaManager
{
	/// <summary>
	/// Interaction logic for TagManager.xaml
	/// </summary>
	public partial class TagManager : Page
	{
		private TextBox selectAlias;
		private readonly List<string> tagNames = new List<string>();
		private readonly Dictionary<int, string> groupNames = new Dictionary<int, string>();
		public TagManager() {
			InitializeComponent();
		}

		#region Events
		private void stackPanel_Loaded( object sender, RoutedEventArgs e ) {
			stkGroups.Children.Clear();
			groupNames.Clear();
			createGroup( "Misc" );
			foreach ( string s in TagData.Groups )
				createGroup( s );

			foreach ( Tag t in TagData.Tags )
				addTag( getContent( t.Group == -1 ? "Misc" : TagData.Groups[t.Group] ), t.Name, new tagData( t.Index, t.GetAliases() ) );
		}
		private void handleListKeyPress( object sender, KeyEventArgs e ) {
			ListBox lst = sender as ListBox;
			switch ( e.Key ) {
				case Key.Delete:
					if ( lst.SelectedItems.Count > 0 )
						lst.Items.RemoveAt( lst.SelectedIndex );
					break;
				case Key.OemPlus:
					int newTags = 0;
					foreach ( TextBox t in lst.Items )
						if ( t.Text.Contains( "New Tag" ) )
							newTags++;
					addTag( lst, "New Tag " + newTags, new tagData( tagNames.Count, Array.Empty<int>() ) );
					break;
			}
		}
		private void handleKeyPress( object sender, KeyEventArgs e ) {
			TextBox txt = sender as TextBox;
			switch ( e.Key ) {
				case Key.Enter:
					if ( string.IsNullOrEmpty( txt.Text ) )
						(txt.Parent as ListBox).Items.Remove( txt );
					else {
						stkGroups.Focus();
						e.Handled = true;
					}
					break;
			}
		}
		private void btnNewGroup_Click( object sender, RoutedEventArgs e ) {
			int newGroups = 0;
			foreach ( GroupBox gb in stkGroups.Children )
				if ( gb.HeaderTemplate?.LoadContent() is TextBox tx && tx.Text.Contains( "New Group" ) )
					newGroups++;

			createGroup( "New Group " + newGroups );
		}
		private void btnDone_Click( object sender, RoutedEventArgs e ) {
			bool foundMisc = false;
			foreach ( KeyValuePair<int, string> name in groupNames ) {
				if ( (foundMisc && name.Value == "Misc") || string.IsNullOrEmpty( name.Value ) ) {
					MessageBox.Show( $"Cannot have a group named \"{(string.IsNullOrEmpty( name.Value ) ? "nothing" : name.Value )}\"" );
					return;
				}
				else if ( name.Value == "Misc" )
					foundMisc = true;
			}
			List<string> toReplaceG = new List<string>();
			Dictionary<int, Tag> toReplaceT = new Dictionary<int, Tag>();
			foreach ( KeyValuePair<int,string> gbName in groupNames ) {
				GroupBox gb = getGroupBox( gbName.Value );
				if ( gbName.Value == "Misc" )
					foreach ( TextBox tx in ((ListView)gb.Content).Items )
						if ( tx.Text.Contains( "," ) || tx.Text.Contains( "@" ) ) {
							MessageBox.Show( "Tag names cannot contain commas (,) or at symbols (@)" );
							tx.Select( 0, 0 );
							return;
						}
						else
							toReplaceT[((tagData)tx.Tag).tagIndex] = new Tag( tx.Text, ((tagData)tx.Tag).tagIndex, -1, ((tagData)tx.Tag).aliases.ToArray() );
				else {
					foreach ( TextBox tx in ((ListView)gb.Content).Items )
						if ( tx.Text.Contains( "," ) || tx.Text.Contains( "@" ) ) {
							MessageBox.Show( "Tag names cannot contain commas (,) or at symbols (@)" );
							tx.Select( 0, 0 );
							return;
						}
						else
							toReplaceT[((tagData)tx.Tag).tagIndex] = new Tag( tx.Text, ((tagData)tx.Tag).tagIndex, toReplaceG.Count, ((tagData)tx.Tag).aliases.ToArray() );
					toReplaceG.Add( gbName.Value );
				}
			}

			TagData.Tags = new Tag[toReplaceT.Count];
			TagData.TagLocations.Clear();
			TagData.Groups = new string[toReplaceG.Count];
			int c = 0;
			foreach ( KeyValuePair<int, Tag> i in toReplaceT ) {
				TagData.TagLocations.Add( i.Key, c );
				TagData.Tags[c++] = i.Value;
			}

			for ( int i = 0; i < toReplaceG.Count; i++ )
				TagData.Groups[i] = toReplaceG[i];
			MessageBox.Show( "Saved Groups and Tags" );
			Paging.LoadPreviousPage();
		}
		private void menuItem_Click( object sender, RoutedEventArgs e ) {
			int newGroups = 0;
			foreach ( GroupBox gb in stkGroups.Children )
				if ( gb.HeaderTemplate?.LoadContent() is TextBox tx && tx.Text.Contains( "New Group" ) )
					newGroups++;

			createGroup( "New Group " + newGroups );
		}
		private void btnBack_Click( object sender, RoutedEventArgs e ) => Paging.LoadPreviousPage( );
		private void clickHandle( object sender, RoutedEventArgs e ) => (sender as UIElement).Focus();
		private void Page_PreviewKeyUp( object sender, KeyEventArgs e ) {
			if ( e.Key == Key.Escape ) {
				selectAlias = null;
				bdrTag.Visibility = Visibility.Collapsed;
				foreach ( GroupBox gb in stkGroups.Children ) {
					ListView lsb = gb.Content as ListView;
					foreach ( TextBox tx in lsb.Items ) {
						tx.Style = (Style)Application.Current.Resources["Tag Textbox Deselect"];
						tx.ForceCursor = false;
						tx.Cursor = Cursors.IBeam;
					}
					gb.ForceCursor = false;
				}
				e.Handled = true;
			}
		}
		private void Page_PreviewMouseRightButtonDown( object sender, MouseButtonEventArgs e ) {
			if ( selectAlias != null )
				e.Handled = true;
		}
		#endregion

		#region Creation
		int curGroupID = 0;
		private GroupBox createGroup( string name ) {
			int idx = curGroupID;
			GroupBox gb = new GroupBox() { Tag = idx };
			groupNames.Add( curGroupID++, name );
			ContextMenu ctx = new ContextMenu()
			{
				FontSize = 12
			};
			ListView gbContent = new ListView()
			{
				Background = Theme.Transparent,
				BorderThickness = new Thickness( 0 ),
				Foreground = (Brush)Application.Current.Resources["DarkText"],
				Focusable = true,
				Tag = name,
				AllowDrop = true
			};
			gbContent.Drop += ( sender, ev ) =>
			{
				if ( sender != ev.OriginalSource ) {
					ListView parent = (ListView)sender;
					TextBox data = (TextBox)ev.Data.GetData( typeof( TextBox ) );
					dragparent.Items.Remove( data );
					addTag( gbContent, data.Text, (tagData)data.Tag );
				}
			};
			gbContent.PreviewMouseMove += ( sender, ev ) =>
			{
				if ( ev.LeftButton == MouseButtonState.Pressed && selectAlias == null ) {
					ListView parent = (ListView)sender;
					dragparent = parent;
					object data = getDataFromListBox( dragparent, ev.GetPosition( parent ) );
					if ( data != null )
						Dispatcher.BeginInvoke( (Action<ListView, object>)(( p, d ) => { try { DragDrop.DoDragDrop( p, d, DragDropEffects.Move ); } catch ( Exception e ) { Logging.Write( e, "Tag Manager" ); } }), parent, data );
				}
			};
			gbContent.PreviewMouseDown += ( sender, ev ) =>
			{
				gbContent.Focus();
			};
			gbContent.SelectionChanged += ( sender, ev ) =>
			{
				if ( gbContent.SelectedIndex != -1 ) {
					if ( selectAlias != null ) {
						TextBox txt = gbContent.SelectedItem as TextBox;
						if ( selectAlias != txt ) {
							if ( ((tagData)selectAlias.Tag).aliases.Contains( ((tagData)txt.Tag).tagIndex ) ) {
								txt.Style = (Style)Application.Current.Resources["Tag Textbox Deselect"];
								tagData t = selectAlias.Tag as tagData;
								t.aliases.Remove( ((tagData)txt.Tag).tagIndex );
								gbContent.Focus();
							}
							else {
								txt.Style = (Style)Application.Current.Resources["Tag Textbox Select"];
								tagData t = selectAlias.Tag as tagData;
								t.aliases.Add( ((tagData)txt.Tag).tagIndex );
								gbContent.Focus();
							}
						}
						Keyboard.ClearFocus();
					}
					gbContent.SelectedIndex = -1;
				}
			};

			if ( name != "Misc" ) {
				DataTemplate template = new DataTemplate { DataType = typeof( GroupBox ) };
				FrameworkElementFactory factory = new FrameworkElementFactory( typeof( TextBox ) );
				factory.SetValue( TextBox.BackgroundProperty, Theme.Transparent );
				factory.SetValue( TextBox.ForegroundProperty, Application.Current.Resources["LightText"] );
				factory.SetValue( TextBox.BorderThicknessProperty, new Thickness( 0 ) );
				factory.SetValue( TextBox.TextProperty, name );
				factory.SetValue( TextBox.FontSizeProperty, 18d );
				factory.SetValue( TextBox.MaxLengthProperty, 16 );
				factory.AddHandler( TextBox.KeyDownEvent, new KeyEventHandler( ( sender, ev ) =>
				{
					KeyEventArgs e = ev as KeyEventArgs;
					TextBox txt = sender as TextBox;
					if ( e.Key == Key.Enter ) {
						if ( txt.Text == "Misc" || string.IsNullOrEmpty( txt.Text ) ) {
							MessageBox.Show( "Group name cannot be " + (txt.Text == "Misc" ? "Misc" : "blank") + ". Please enter a new name" );
							txt.Select( 0, 0 );
							return;
						}
						else
							stkGroups.Focus();
					}
					groupNames[idx] = txt.Text;
				} ) );
				factory.AddHandler( TextBox.LostFocusEvent, new RoutedEventHandler( ( sender, _ ) =>
				{
					TextBox txt = sender as TextBox;
					if ( txt.Text == "Misc" || string.IsNullOrEmpty( txt.Text ) ) {
						MessageBox.Show( "Group name cannot be " + (txt.Text == "Misc" ? "Misc" : "blank") + ". Please enter a new name" );
						txt.Select( 0, 0 );
						return;
					}
					groupNames[idx] = txt.Text;
				} ) );
				factory.AddHandler( TextBox.PreviewMouseDownEvent, new MouseButtonEventHandler( ( _, e ) =>
				{
					if (selectAlias != null ) {
						Keyboard.ClearFocus();
						gbContent.Focus();
						e.Handled = true;
					}
				} ) );
				template.VisualTree = factory;
				gb.HeaderTemplate = template;

				MenuItem rmv = new MenuItem() { Header = "Remove Group" };
				rmv.Click += ( sender, e ) =>
				{
					groupNames.Remove( idx );
					stkGroups.Children.Remove( gb );
				};

				ctx.Items.Add( rmv );
			}
			else {
				gb.Header = "Misc";
				gb.Foreground = (Brush)Application.Current.Resources["LightText"];
				gb.FontSize = 18d;
			}
			MenuItem add = new MenuItem() { Header = "Add Tag" };
			add.Click += ( sender, e ) =>
			{
				int newTags = 0;
				foreach ( TextBox t in gbContent.Items )
					if ( t.Text.Contains( "New Tag" ) )
						newTags++;
				addTag( gbContent, "New Tag " + newTags, new tagData( tagNames.Count, Array.Empty<int>() ) );
			};
			ctx.Items.Add( add );
			gbContent.KeyDown += handleListKeyPress;
			gbContent.MouseDown += clickHandle;


			gb.ContextMenu = ctx;

			gb.Width = 171;
			gb.Margin = new Thickness( 7, 0, 0, 0 );
			gb.Background = new SolidColorBrush( (Color)ColorConverter.ConvertFromString( "#22FFFFFF" ) );
			gb.Foreground = (Brush)Application.Current.Resources["LightText"];


			gb.Content = gbContent;
			stkGroups.Children.Add( gb );
			return gb;
		}
		private void addTag( ListBox lb, string name, tagData data ) {
			TextBox txt = new TextBox() { HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center, Text = name, Style = Application.Current.Resources["Tag Textbox Deselect"] as Style, MaxLength = 16, Foreground = (Brush)Application.Current.Resources["LightText"], FontSize = 14, Tag = data, Width = 140 };
			ContextMenu ctx = new ContextMenu()
			{
				FontSize = 12
			};
			MenuItem rmv = new MenuItem() { Header = "Remove" };
			rmv.Click += ( _, ev ) =>
			{
				lb.Items.Remove( txt );
			};
			ctx.Items.Add( rmv );
			MenuItem alias = new MenuItem() { Header = "Manage Aliases" };
			alias.Click += ( _, ev ) =>
			{
				bdrTag.Visibility = Visibility.Visible;
				List<int> aliases = ((tagData)txt.Tag).aliases;
				foreach ( GroupBox gb in stkGroups.Children ) {
					ListView lsb = gb.Content as ListView;
					foreach ( TextBox tx in lsb.Items ) {
						if ( aliases.Contains( ((tagData)tx.Tag).tagIndex ) )
							tx.Style = (Style)Application.Current.Resources["Tag Textbox Select"];
						else if ( tx == txt )
							tx.Style = (Style)Application.Current.Resources["Tag Textbox Highlight"];
						tx.ForceCursor = true;
						tx.Cursor = Cursors.Arrow;
					}
					gb.ForceCursor = true;
					gb.Cursor = Cursors.Arrow;
				}
				selectAlias = txt;
				lb.Focus();
			};
			ctx.Items.Add( alias );
			txt.ContextMenu = ctx;
			txt.KeyDown += handleKeyPress;
			txt.PreviewMouseLeftButtonDown += ( _, ev ) =>
			{
				if ( selectAlias != null ) {
					if ( selectAlias != txt ) {
						if ( ((tagData)selectAlias.Tag).aliases.Contains( ((tagData)txt.Tag).tagIndex ) ) {
							txt.Style = (Style)Application.Current.Resources["Tag Textbox Deselect"];
							tagData t = selectAlias.Tag as tagData;
							t.aliases.Remove( ((tagData)txt.Tag).tagIndex );
						}
						else {
							txt.Style = (Style)Application.Current.Resources["Tag Textbox Select"];
							tagData t = selectAlias.Tag as tagData;
							t.aliases.Add( ((tagData)txt.Tag).tagIndex );
						}
					}
					Keyboard.ClearFocus();
					lb.Focus();
					ev.Handled = true;
				}
			};

			lb.Items.Add( txt );
			tagNames.Add( name );
		}
		#endregion

		#region DragDrop
		ListBox dragparent;
		private static object getDataFromListBox( ListBox source, Point point ) {
			if ( source.InputHitTest( point ) is UIElement element ) {
				object data = DependencyProperty.UnsetValue;
				while ( data == DependencyProperty.UnsetValue ) {
					data = source.ItemContainerGenerator.ItemFromContainer( element );

					if ( data == DependencyProperty.UnsetValue )
						element = VisualTreeHelper.GetParent( element ) as UIElement;
					if ( element == source )
						return null;
				}

				if ( data != DependencyProperty.UnsetValue )
					return data;
			}
			return null;
		}
		#endregion

		#region Utilities
		private GroupBox getGroupBox( string groupName ) {
			if ( groupName == "Misc" )
				return stkGroups.Children[0] as GroupBox;
			foreach ( KeyValuePair<int, string> kvp in groupNames )
				if ( kvp.Value == groupName )
					foreach ( GroupBox gb in stkGroups.Children )
						if ( (int)gb.Tag == kvp.Key )
							return gb;
			return null;
		}
		private ListBox getContent( string groupName ) => getGroupBox( groupName ).Content as ListBox;
		#endregion

	}

#pragma warning disable IDE1006 // Naming Styles
	internal class tagData
	{
		public int tagIndex;
		public List<int> aliases;

		public tagData( int tagIndex, int[] aliases ) {
			this.tagIndex = tagIndex;
			this.aliases = aliases.ToList();
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}