using System;
using System.Collections.Generic;
using System.Linq;
using NanaManagerAPI.Media;
using NanaManagerAPI.Data;

namespace NanaManagerAPI
{
    public static class TagData
    {
		/// <summary>
		/// An array of all loaded tags
		/// </summary>
		public static Tag[] Tags = Array.Empty<Tag>();
		/// <summary>
		/// A dictionary containing the indices of corresponding tags
		/// </summary>
		public static readonly Dictionary<int, int> TagLocations = new Dictionary<int, int>();
		/// <summary>
		/// An array of all loaded group names
		/// </summary>
		public static string[] Groups = Array.Empty<string>();
		/// <summary>
		/// An array of tags to be hidden unless specified
		/// </summary>
		public static int[] HiddenTags = Array.Empty<int>();


		private static bool checkMedia(int[] searchTags, IMedia m) {
			int[] tags = m.GetTags();
			bool match = true;
			foreach (int i in searchTags)
				if (!tags.Contains(i)) {
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
		/// <returns>The media that matches the conditions provided</returns>
		public static IMedia SearchForMedia(int[] SearchTags, int Iterations) {
			return SearchForMedia( SearchTags, Iterations, 0 );
        }
		/// <summary>
		/// Searches for a piece of media with the specified tags
		/// </summary>
		/// <param name="SearchTags">The tags to match with the image</param>
		/// <param name="Iterations">How many in to search for</param>
		/// <param name="Start">The index to search from</param>
		/// <returns>The media that matches the conditions provided</returns>
		public static IMedia SearchForMedia(int[] SearchTags, int Iterations, int Start) {
			int it = 1;
			int step = 1;
			foreach ( KeyValuePair<string, IMedia> m in Globals.Media ) {
				if ( it < Start )
					it++;
				else {
					if ( checkMedia( SearchTags, m.Value ) )
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
		public static string[] SearchForAll(int[] SearchTerms) {
			List<string> matches = new List<string>();
			foreach ( KeyValuePair<string, IMedia> m in Globals.Media )
				if ( checkMedia( SearchTerms, m.Value ) )
					matches.Add( m.Key );

			return matches.ToArray();
        }
	}
}
