using NanaManagerAPI;
using NanaManagerAPI.IO;
using NanaManagerAPI.Media;
using NanaManagerAPI.Types;
using NanaManagerAPI.UI;
using System.Collections.Generic;
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
            UI.SetStatus( "Loading Data - First Time Check", 0, 2 );
            if ( !ContentFile.Exists( "nanaData" ) ) { //Checks for the existence of the nanaData file (Holds tag, group and media information)
                UI.SetStatus( "Loading Data - First Time Check", 1, 2 ); //If not, generate a new file
                DataEncoder encoder = new DataEncoder(); //Initialise the encoding tool
                encoder.Write( VersionMajor ); //Setting the file version
                encoder.Write( VersionMinor );
                encoder.Write( 0 );
                encoder.Write( 0 ); //No tags, groups or media yet
                encoder.Write( 0 );
                UI.SetStatus( "Loading Data - First Time Check", 2, 2 );
                ContentFile.WriteFile( "nanaData", encoder.ToString() ); //Write the data to a nanaData file
                return; //As there is no non-default data, don't try to read any
            }
            UI.SetStatus( "Loading Data - Loading Data", 0, 2 );

            DataDecoder decoder = new DataDecoder( ContentFile.ReadFile( "nanaData" ) ); //Initialise the decoding tool
            UI.SetStatus( "Loading Data - Checking Version", 1, 2 );
            //Versioning
            int major = decoder.ReadInt32();
            int minor = decoder.ReadInt32(); //Read the version number
            UI.SetStatus( "Loading Data - Checking Version", 2, 2 );
            if ( major == VersionMajor && minor == VersionMinor ) { //Check if the version is the current version
                UI.SetStatus( "Loading Data - Fetching Data Quantity", 0, 3 );
                
                //Getting the amount of data to read (No eof for zip streams)
                int groupCount = decoder.ReadInt32();
                UI.SetStatus( "Loading Data - Fetching Data Quantity", 1, 3 );
                int tagCount = decoder.ReadInt32();
                UI.SetStatus( "Loading Data - Fetching Data Quantity", 2, 3 );
                int imageCount = decoder.ReadInt32();
                UI.SetStatus( "Loading Data - Fetching Data Quantity", 3, 3 );
                double operations = groupCount + tagCount + imageCount; //How much data to read. Used to display progress
                double progress = 0;

                Data.TagLocations.Clear(); //Initialise default values (Mostly used in case data is loaded again)
                Data.Groups.Clear();
                Data.Tags = new Tag[tagCount];
                Data.HiddenTags = new int[tagCount];

                UI.SetStatus( $"Loading Data - Getting Groups 0/{groupCount}", progress, operations );
                //Begin reading group data
                for ( int i = 0; i < groupCount; i++ ) {
                    Data.Groups[decoder.ReadInt32()] = decoder.ReadString(); //Groups are just names and an ID, so this can be limited to a single line
                    UI.SetStatus( $"Loading Data - Getting Groups {i + 1}/{groupCount}", progress++, operations );
                }

                UI.SetStatus( $"Loading Data - Getting Tags 0/{tagCount}", progress, operations );
                int hiddenCount = 0;
                //Begin reading tag data
                for ( int i = 0; i < tagCount; i++ ) {
                    bool hidden = decoder.ReadBoolean(); //First data point is whether the tag is hidden or not
                    int index = decoder.ReadInt32(); //The ID index of the tag
                    Data.TagLocations.Add( index, i ); //Links the tag's ID to its location
                    string name = decoder.ReadString(); //Get the name
                    int[] aliases = decoder.ReadInt32Array().ToArray(); //Read the linked IDs of this tag as an array
                    Data.Tags[i] = new Tag( name, index, decoder.ReadInt32(), aliases ); //Last integer in the data is the group ID
                    if ( hidden )
                        Data.HiddenTags[hiddenCount++] = index; //Adds the tag to the hidden tag list if the tag is a hidden tag
                    UI.SetStatus( $"Loading Data - Getting Tags {i + 1}/{tagCount}", progress++, operations );
                }

                UI.SetStatus( $"Loading Data - Getting Images 0/{imageCount}", progress, operations );
                //Begin reading image data
                for ( int i = 0; i < imageCount; i++ ) {
                    string uID = decoder.ReadString(); //Get the UID of the media (The key in the database)
                    string fileType = decoder.ReadString(); //Get the file extension of the media (For handling different data types)
                    int[] tags = decoder.ReadInt32Array().ToArray(); //Read the selected tags' IDs for the media as an array
                    Data.Media.Add( uID, (IMedia)Registry.MediaConstructors[Registry.ExtensionConstructors[fileType]].Invoke( new object[] { uID, tags, fileType } ) ); //Gets which data type can represent the media, and constructs the object with the provided data
                    UI.SetStatus( $"Loading Data - Getting Images {i + 1}/{imageCount}", progress++, operations );
                }
            }
            else {
                //TODO - CODE TO DIVERT OR ERROR IF WRONG VERSION
            }
        }

        public void SaveData() {
            DataEncoder encoder = new DataEncoder();

            //Writes the version number of this encoder
            encoder.Write( VersionMajor );
            encoder.Write( VersionMinor );

            //Writes the amount of groups, tags and media there is
            encoder.Write( Data.Groups.Count );
            encoder.Write( Data.Tags.Length );
            encoder.Write( Data.Media.Count );

            //Writes the groups to the encoder
            foreach ( KeyValuePair<int, string> name in Data.Groups ) {
                encoder.Write( name.Key );
                encoder.Write( name.Value );
            }
            //Writes the tags to the encoder
            foreach ( Tag t in Data.Tags ) {
                encoder.Write( Data.HiddenTags.Contains( t.Index ) );
                encoder.Write( t.Index );
                encoder.Write( t.Name );
                int[] aliases = t.GetAliases();
                encoder.Write( aliases );
                encoder.Write( t.Group );
            }
            //Writes the media to the encoder
            foreach ( KeyValuePair<string, IMedia> img in Data.Media ) {
                encoder.Write( img.Key );
                encoder.Write( img.Value.FileType );
                int[] tags = img.Value.GetTags();
                encoder.Write( tags );
            }

            ContentFile.WriteFile( "nanaData", encoder.ToString() ); //Writes the encoded data to the nanaData file
        }
    }
}
