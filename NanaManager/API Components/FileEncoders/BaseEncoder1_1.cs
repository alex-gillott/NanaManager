using System.Collections.Generic;

using NanaManagerAPI.Media;
using NanaManagerAPI.Types;
using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI;
using System.Linq;

namespace NanaManager.FileEncoders
{
	public class BaseEncoder1_1 : IEncoder
	{
		public string Name { get; } = "Nana Encoder";
		public string InternalName { get; } = "hydroxa.nanaBrowser.baseEncoder-1.2";
		public int VersionMajor { get; } = 1;
		public int VersionMinor { get; } = 2;
		public string Description { get; } = "The default data writer of Nana Browser. This encodes and stores the data within the vanilla application";
		public EncoderType Type { get; } = EncoderType.Data;

		public void LoadData() {
			UI.SetStatus( "Loading Data - First Time Check", 0, 2);
			if ( !ContentFile.Exists( "nanaData" ) ) {
				UI.SetStatus( "Loading Data - First Time Check", 1, 2 );
				DataEncoder encoder = new DataEncoder();
				encoder.Write( VersionMajor );
				encoder.Write( VersionMinor );
				encoder.Write( 0 );
				encoder.Write( 0 );
				encoder.Write( 0 );
				UI.SetStatus( "Loading Data - First Time Check", 2, 2 );
				ContentFile.WriteFile( "nanaData", encoder.ToString() );
				return;
			}
			UI.SetStatus( "Loading Data - Loading Data", 0, 2 );

			DataDecoder decoder = new DataDecoder( ContentFile.ReadFile( "nanaData" ) ); //Initialise the decode tool
			UI.SetStatus( "Loading Data - Checking Version", 1, 2 );
			//Versioning
			int major = decoder.ReadInt32();
			int minor = decoder.ReadInt32();
			UI.SetStatus( "Loading Data - Checking Version", 2, 2 );
			if ( major == VersionMajor && minor == VersionMinor ) {

				UI.SetStatus( "Loading Data - Fetching Data Quantity", 0, 3 );
				//Collection Counts
				int groupCount = decoder.ReadInt32();
				UI.SetStatus( "Loading Data - Fetching Data Quantity", 1, 3 );
				int tagCount = decoder.ReadInt32();
				UI.SetStatus( "Loading Data - Fetching Data Quantity", 2, 3 );
				int imageCount = decoder.ReadInt32();
				UI.SetStatus( "Loading Data - Fetching Data Quantity", 3, 3 );
				double operations = groupCount + tagCount + imageCount;
				double progress = 0;

				Data.TagLocations.Clear();
				Data.Groups.Clear();
				Data.Tags = new Tag[tagCount];
				Data.HiddenTags = new int[tagCount];

				UI.SetStatus( $"Loading Data - Getting Groups 0/{groupCount}", progress, operations );
				//Groups
				for ( int i = 0; i < groupCount; i++ ) {
					Data.Groups[decoder.ReadInt32()] = decoder.ReadString(); //Groups are just names and an ID
					UI.SetStatus( $"Loading Data - Getting Groups {i + 1}/{groupCount}", progress++, operations );
				}

				UI.SetStatus( $"Loading Data - Getting Tags 0/{tagCount}", progress, operations );
				int hiddenCount = 0;
				//Tags
				for ( int i = 0; i < tagCount; i++ ) {
					bool hidden = decoder.ReadBoolean();
					int index = decoder.ReadInt32();
					Data.TagLocations.Add( index, i );
					string name = decoder.ReadString(); //Get the name
					int aliasCount = decoder.ReadInt32(); //Get the amount of aliases
					int[] aliases = new int[aliasCount];
					for ( int o = 0; o < aliasCount; o++ )
						aliases[o] = decoder.ReadInt32(); //Get the aliase indices
					Data.Tags[i] = new Tag( name, index, decoder.ReadInt32(), aliases ); //Last integer is the group
					if ( hidden )
						Data.HiddenTags[hiddenCount++] = index;
					UI.SetStatus( $"Loading Data - Getting Tags {i+1}/{tagCount}", progress++, operations );
				}

				UI.SetStatus( $"Loading Data - Getting Images 0/{imageCount}", progress, operations );
				//Images
				for ( int i = 0; i < imageCount; i++ ) {
					string uID = decoder.ReadString();
					string fileType = decoder.ReadString();
					tagCount = decoder.ReadInt32();
					int[] tags = new int[tagCount];
					for ( int o = 0; o < tagCount; o++ )
						tags[o] = decoder.ReadInt32();
					Data.Media.Add( uID, (IMedia)Registry.MediaConstructors[Registry.ExtensionConstructors[fileType]].Invoke( new object[] { uID, tags, fileType } ) );
					UI.SetStatus( $"Loading Data - Getting Images {i + 1}/{imageCount}", progress++, operations );
				}
			} else {
				//TODO - CODE TO DIVERT OR ERROR IF WRONG VERSION
			}
		}
		public void SaveData() {
			DataEncoder encoder = new DataEncoder();

			//Versioning
			encoder.Write( VersionMajor );
			encoder.Write( VersionMinor );

			//Group and Tag Count
			encoder.Write( Data.Groups.Count );
			encoder.Write( Data.Tags.Length );
			encoder.Write( Data.Media.Count );

			//Groups
			foreach ( KeyValuePair<int, string> name in Data.Groups ) {
				encoder.Write( name.Key );
				encoder.Write( name.Value );
			}
			//Tags
			foreach ( Tag t in Data.Tags ) {
				encoder.Write( Data.HiddenTags.Contains( t.Index ) );
				encoder.Write( t.Index );
				encoder.Write(t.Name);
				int[] aliases = t.GetAliases();
				encoder.Write( aliases.Length );
				foreach ( int a in aliases )
					encoder.Write( a );
				encoder.Write( t.Group );
			}
			//Images
			foreach ( KeyValuePair<string, IMedia> img in Data.Media) {
				if ( img.Value is Image ) {
					encoder.Write( img.Key );
					encoder.Write( img.Value.FileType );
					int[] tags = img.Value.GetTags();
					encoder.Write( tags.Length );
					foreach ( int t in tags )
						encoder.Write( t );
				}
			}

			ContentFile.WriteFile( "nanaData", encoder.ToString() );
		}
	}
}
