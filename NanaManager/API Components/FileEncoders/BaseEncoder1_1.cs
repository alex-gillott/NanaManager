using System.Collections.Generic;

using NanaManagerAPI.Media;
using NanaManagerAPI.Data;
using NanaManagerAPI.IO;
using NanaManagerAPI.UI;
using NanaManagerAPI;

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
			Globals.SetStatus( "Loading Data - First Time Check", 0, 2);
			if ( !ContentFile.Exists( "nanaData" ) ) {
				Globals.SetStatus( "Loading Data - First Time Check", 1, 2 );
				DataEncoder encoder = new DataEncoder();
				encoder.Write( VersionMajor );
				encoder.Write( VersionMinor );
				encoder.Write( 0 );
				encoder.Write( 0 );
				encoder.Write( 0 );
				Globals.SetStatus( "Loading Data - First Time Check", 2, 2 );
				ContentFile.WriteFile( "nanaData", encoder.ToString() );
				return;
			}
			Globals.SetStatus( "Loading Data - Loading Data", 0, 2 );

			DataDecoder decoder = new DataDecoder( ContentFile.ReadFile( "nanaData" ) ); //Initialise the decode tool
			Globals.SetStatus( "Loading Data - Checking Version", 1, 2 );
			//Versioning
			int major = decoder.ReadInt32();
			int minor = decoder.ReadInt32();
			Globals.SetStatus( "Loading Data - Checking Version", 2, 2 );
			if ( major == VersionMajor && minor == VersionMinor ) {

				Globals.SetStatus( "Loading Data - Fetching Data Quantity", 0, 3 );
				//Collection Counts
				int groupCount = decoder.ReadInt32();
				Globals.SetStatus( "Loading Data - Fetching Data Quantity", 1, 3 );
				int tagCount = decoder.ReadInt32();
				Globals.SetStatus( "Loading Data - Fetching Data Quantity", 2, 3 );
				int imageCount = decoder.ReadInt32();
				Globals.SetStatus( "Loading Data - Fetching Data Quantity", 3, 3 );
				double operations = groupCount + tagCount + imageCount;
				double progress = 0;

				TagData.TagLocations.Clear();
				TagData.Groups.Clear();
				TagData.Tags = new Tag[tagCount];

				Globals.SetStatus( $"Loading Data - Getting Groups 0/{groupCount}", progress, operations );
				//Groups
				for ( int i = 0; i < groupCount; i++ ) {
					TagData.Groups[decoder.ReadInt32()] = decoder.ReadString(); //Groups are just names and an ID
					Globals.SetStatus( $"Loading Data - Getting Groups {i + 1}/{groupCount}", progress++, operations );
				}

				Globals.SetStatus( $"Loading Data - Getting Tags 0/{tagCount}", progress, operations );
				//Tags
				for ( int i = 0; i < tagCount; i++ ) {
					int index = decoder.ReadInt32();
					TagData.TagLocations.Add( index, i );
					string name = decoder.ReadString(); //Get the name
					int aliasCount = decoder.ReadInt32(); //Get the amount of aliases
					int[] aliases = new int[aliasCount];
					for ( int o = 0; o < aliasCount; o++ )
						aliases[o] = decoder.ReadInt32(); //Get the aliase indices
					TagData.Tags[i] = new Tag( name, index, decoder.ReadInt32(), aliases ); //Last integer is the group
					Globals.SetStatus( $"Loading Data - Getting Tags {i+1}/{tagCount}", progress++, operations );
				}

				Globals.SetStatus( $"Loading Data - Getting Images 0/{imageCount}", progress, operations );
				//Images
				for ( int i = 0; i < imageCount; i++ ) {
					string uID = decoder.ReadString();
					string fileType = decoder.ReadString();
					tagCount = decoder.ReadInt32();
					int[] tags = new int[tagCount];
					for ( int o = 0; o < tagCount; o++ )
						tags[o] = decoder.ReadInt32();
					Globals.Media.Add(uID, new Image( uID, tags, fileType ));
					Globals.SetStatus( $"Loading Data - Getting Images {i + 1}/{imageCount}", progress++, operations );
				}
			} else {
				//TODO - CODE TO DIVERT OR ERROR IF WRONG VERSION
			}
		}
		public void SaveData() {
			DataEncoder encoder = new DataEncoder();

			//Versioning
			encoder.Write( VersionMajor ); //                                             4b
			encoder.Write( VersionMinor ); //                                             4b

			//Group and Tag Count
			encoder.Write( TagData.Groups.Length ); //32-bit integer                   4b
			encoder.Write( TagData.Tags.Length ); //32-bit integer                     4b
			encoder.Write( Globals.Media.Count ); //32-bit integer                    4b

			//Groups
			foreach ( string name in TagData.Groups )
				encoder.Write( name ); //32-bit integer defining length + string          4b+name.Length
			//Tags
			foreach ( Tag t in TagData.Tags ) {
				encoder.Write( t.Index ); //                                              4b
				encoder.Write(t.Name); //32-bit integer defining length + string          4b+name.Length
				int[] aliases = t.GetAliases(); //Get aliases
				encoder.Write( aliases.Length ); //32-bit integer defining alias quantity 4b
				foreach ( int a in aliases )
					encoder.Write( a ); //32-bit integer referring to alias               4b
				encoder.Write( t.Group ); //32-bit integer referring to group             4b
			}
			//Images
			foreach ( KeyValuePair<string, IMedia> img in Globals.Media) {
				if ( img.Value is Image ) {
					encoder.Write( img.Key );
					encoder.Write( img.Value.FileType );
					int[] tags = img.Value.GetTags();
					encoder.Write( tags.Length ); //32-bit integer defining tag quantity      4b
					foreach ( int t in tags )
						encoder.Write( t ); //32-bit integer referring to a tag               4b
				}
			}

			ContentFile.WriteFile( "nanaData", encoder.ToString() );
		}
	}
}
