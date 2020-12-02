using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;

namespace NanaManagerAPI.UI
{
    /// <summary>
    /// Handles changing page in the UI
    /// </summary>
    public static class Paging
    {
        private const string ADD_NULL_KEY_FORMAT = "{0} tried to add a new page with a null key\nStack Trace:\n{1}";
        private const string ADD_EXISTING_KEY_FORMAT = "{0} tried to add a new page with the existing key \"{1}\"\nStack Trace:\n{2}";
        private const string REMOVE_NULL_KEY_FORMAT = "{0} tried to remove a page with a null key\nStack Trace:\n{1}";
        private const string CHECK_NULL_KEY_FORMAT = "{0} tried to check the existence of a page with a null key\nStack Trace:\n{1}";
        private const string GET_NULL_KEY_FORMAT = "{0} tried to get a page with a null key\nStack Trace:\n{1}";
        private const string GET_NONEXISTENT_KEY_FORMAT = "{0} tried to get a page with the non-exisistent key \"{1}\"\nStack Trace:\n{2}";
        private const string LOAD_NULL_PAGE_FORMAT = "{0} tried to load a page with a null key\nStack Trace:\n{1}";
        private const string LOAD_INVALID_PAGE_FORMAT = "{0} tried to load an invalid page \"{1}\"\nStack Trace:\n{2}";

        /// <summary>
        /// Handles switching to a new <see cref="Page"/>
        /// </summary>
        /// <param name="NewPage">The <see cref="Page"/> to switch to</param>
        public delegate void PageHandler( Page NewPage, string ID );

        /// <summary>
        /// Triggers whenever the focused page changes
        /// </summary>
        public static event PageHandler PageChanged;

        private static readonly Dictionary<string, Page> pages = new Dictionary<string, Page>();
        private static readonly Stack<string> history = new Stack<string>();

        /// <summary>
        /// Returns true if the page already exists
        /// </summary>
        /// <param name="ID">The ID of the page to confirm</param>
        public static bool CheckPageExists( string ID ) {
            if ( string.IsNullOrEmpty( ID ) ) {
                ArgumentNullException ex = new ArgumentNullException( "The provided ID was null or empty." );
                Logging.Write( string.Format( CHECK_NULL_KEY_FORMAT, Assembly.GetCallingAssembly().GetName().Name, ex.StackTrace ), "Paging", LogLevel.Error );
                throw ex;
            }
            return pages.ContainsKey( ID );
        }

        /// <summary>
        /// Adds a new <see cref="Page"/> with the specified ID
        /// </summary>
        /// <param name="ID">The ID of the <see cref="Page"/> being added</param>
        /// <param name="Page">The <see cref="Page"/> to be added</param>
        public static void AddPage( string ID, Page Page ) {
            try {
                pages.Add( ID, Page );
                Logging.Write( $"Successfully added page \"{ID}\"", "Paging" );
            } catch ( ArgumentNullException ex ) {
                Logging.Write( string.Format( ADD_NULL_KEY_FORMAT, Page.GetType().Assembly.GetName().Name, ex.StackTrace ), "Paging", LogLevel.Error );
                throw new ArgumentNullException( "Page ID was null or empty.", ex );
            } catch ( ArgumentException ex ) {
                Logging.Write( string.Format( ADD_EXISTING_KEY_FORMAT, Page.GetType().Assembly.GetName().Name, ID, ex.StackTrace ), "Paging", LogLevel.Error );
                throw new ArgumentException( $"Page ID {ID} already exists.", ex );
            }
        }

        /// <summary>
        /// Removes the <see cref="Page"/> under the specified ID
        /// </summary>
        /// <param name="ID">The ID of the <see cref="Page"/> to remove</param>
        public static bool RemovePage( string ID ) {
            try {
                Logging.Write( $"Attempting to remove page \"{ID}\"", "Paging" );
                return pages.Remove( ID );
            } catch ( ArgumentNullException ex ) {
                Logging.Write( string.Format( REMOVE_NULL_KEY_FORMAT, Assembly.GetCallingAssembly().GetName().Name, Environment.StackTrace ), "Paging", LogLevel.Error );
                throw new ArgumentNullException( "Page ID was null.", ex );
            }
        }

        /// <summary>
        /// Returns the <see cref="Page"/> under the specified ID
        /// </summary>
        /// <param name="ID">The ID of the <see cref="Page"/> to return</param>
        /// <returns>The <see cref="Page"/> under the specified ID</returns>
        public static Page GetPage( string ID ) {
            try {
                return pages[ID];
            } catch ( ArgumentNullException ex ) {
                Logging.Write( string.Format( GET_NULL_KEY_FORMAT, Assembly.GetCallingAssembly().GetName().Name, Environment.StackTrace ), "Paging", LogLevel.Error );
                throw new ArgumentNullException( "Page ID was null.", ex );
            } catch ( KeyNotFoundException ex ) {
                Logging.Write( string.Format( GET_NONEXISTENT_KEY_FORMAT, Assembly.GetCallingAssembly().GetName().Name, ID, Environment.StackTrace ), "Paging", LogLevel.Error );
                throw new KeyNotFoundException( "The specified page did not exist.", ex );
            }
        }

        /// <summary>
        /// Tells the application to load a specific <see cref="Page"/>
        /// </summary>
        /// <param name="ID">The ID of the <see cref="Page"/> to load</param>
        public static void LoadPage( string ID ) {
            try {
                PageChanged?.Invoke( GetPage( ID ), ID );
                history.Push( ID );
                Logging.Write( $"Changed to page \"{ID}\"", "Paging" );
            } catch ( ArgumentNullException) {
                Logging.Write( string.Format( LOAD_NULL_PAGE_FORMAT, Assembly.GetCallingAssembly().GetName().Name, Environment.StackTrace ), "Paging", LogLevel.Fatal );
            } catch ( KeyNotFoundException ) {
                Logging.Write( string.Format( LOAD_INVALID_PAGE_FORMAT, Assembly.GetCallingAssembly().GetName().Name, ID, Environment.StackTrace ), "Paging", LogLevel.Fatal );
            }
        }

        /// <summary>
        /// Returns the current <see cref="Page"/> loaded
        /// </summary>
        /// <returns>The currently loaded <see cref="Page"/></returns>
        public static string GetCurrentPage() => history.Count > 0 ? history.Peek() : null;

        /// <summary>
        /// Loads the <see cref="Page"/> loaded before the current <see cref="Page"/>
        /// </summary>
        /// <returns>The ID of the new <see cref="Page"/>. Returns <see cref="null"/> if the end of the history is met</returns>
        public static string LoadPreviousPage() {
            if ( history.Count == 0 )
                return null;
            history.Pop();
            string toLoad = history.Pop();
            LoadPage( toLoad );
            return toLoad;
        }
    }
}