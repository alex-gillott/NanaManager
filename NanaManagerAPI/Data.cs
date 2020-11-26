using NanaManagerAPI.Media;
using NanaManagerAPI.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NanaManagerAPI
{
    /// <summary>
    /// Provideds methods for tag-related operations, such as searching
    /// </summary>
    public static class Data
    {
        /// <summary>
        /// Defines whether hidden tags should be shown
        /// </summary>
        public static bool ShowHiddenTags = false;

        /// <summary>
        /// An array of all loaded tags
        /// </summary>
        public static Tag[] Tags = Array.Empty<Tag>();

        /// <summary>
        /// A dictionary containing the indices of corresponding tags
        /// </summary>
        public static readonly Dictionary<int, int> TagLocations = new Dictionary<int, int>();

        /// <summary>
        /// A dictionary of all loaded group names
        /// </summary>
        public static readonly Dictionary<int, string> Groups = new Dictionary<int, string>();

        /// <summary>
        /// An array of tags to be hidden unless specified
        /// </summary>
        public static int[] HiddenTags = Array.Empty<int>();

        /// <summary>
        /// A dictionary of all loaded images
        /// </summary>
        public static readonly Dictionary<string, IMedia> Media = new Dictionary<string, IMedia>();

        /// <summary>
        /// Returns true if the provided media object contains the tags
        /// </summary>
        /// <param name="SearchTags"></param>
        /// <param name="M"></param>
        /// <returns></returns>
        public static bool CheckMedia( int[] SearchTags, int[] ExcludeTags, IMedia M ) {
            int[] tags = M.GetTags();
            bool match = true;
            foreach ( int i in SearchTags )
                if ( !tags.Contains( i ) ) {
                    match = false;
                    break;
                }
            if ( match )
                foreach ( int i in ExcludeTags )
                    if ( tags.Contains( i ) ) {
                        match = false;
                        break;
                    }
            if ( match )
                foreach ( int i in tags )
                    if ( !ShowHiddenTags && HiddenTags.Contains( i ) ) {
                        match = false;
                        break;
                    }

            return match;
        }

        /// <summary>
        /// Searches for a piece of media with the specified tags
        /// </summary>
        /// <param name="SearchTags">The tags to match with the image</param>
        /// <param name="Iterations">How many in to search for</param>
        /// <returns>The media that matches the conditions provided or null if nothing was found</returns>
        public static IMedia SearchForMedia( int[] SearchTags, int Iterations, int[] ExcludeTags ) => SearchForMedia( SearchTags, Iterations, 0, ExcludeTags );

        /// <summary>
        /// Searches for a piece of media with the specified tags
        /// </summary>
        /// <param name="SearchTags">The tags to match with the image</param>
        /// <param name="Iterations">How many in to search for</param>
        /// <param name="Start">The index to search from</param>
        /// <returns>The media that matches the conditions provided or null, if nothing was found</returns>
        public static IMedia SearchForMedia( int[] SearchTags, int Iterations, int Start, int[] ExcludeTags ) {
            int it = 1;
            int step = 1;
            foreach ( KeyValuePair<string, IMedia> m in Media ) {
                if ( it < Start )
                    it++;
                else {
                    if ( CheckMedia( SearchTags, ExcludeTags, m.Value ) )
                        if ( step < Iterations )
                            step++;
                        else
                            return m.Value;
                    else
                        continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Searches for all media that fits the given search terms
        /// </summary>
        /// <param name="SearchTerms">The terms to match with the items</param>
        /// <returns>A list of IDs matching the search terms</returns>
        public static string[] SearchForAll( int[] SearchTerms, int[] RejectedTerms ) {
            List<string> matches = new List<string>();
            foreach ( KeyValuePair<string, IMedia> m in Media )
                if ( CheckMedia( SearchTerms, RejectedTerms, m.Value ) )
                    matches.Add( m.Key );

            return matches.ToArray();
        }

        /// <summary>
        /// A shortcut for getting a <see cref="Tag"/> by ID.
        /// </summary>
        /// <param name="ID">The ID of the tag to get</param>
        /// <returns>The <see cref="Tag"/> with the corresponding ID</returns>
        public static Tag GetTag( int ID ) => Tags[TagLocations[ID]];
    }
}