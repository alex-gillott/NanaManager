using System;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace NanaManagerAPI.UI
{
    /// <summary>
    /// Represents a tab in the Settings Page
    /// </summary>
    public class SettingsTab
    {
        /// <summary>
        /// The parent <see cref="SettingsTab"/>, if this tab is subordinate
        /// </summary>
        public readonly SettingsTab Parent;
        /// <summary>
        /// The ID of the <see cref="SettingsTab"/>
        /// </summary>
        public readonly string ID;
        /// <summary>
        /// The title displayed for the <see cref="SettingsTab"/>
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// The <see cref="Page"/> that is displayed when the tab is selected
        /// </summary>
        public readonly Page Display;
        private StringCollection Subordinates = new StringCollection();

        /// <summary>
        /// Initialises a new <see cref="SettingsTab"/> with a display and no subordinates
        /// </summary>
        /// <param name="ID">The ID of the <see cref="SettingsTab"/></param>
        /// <param name="Title">The title to be displayed</param>
        /// <param name="Display">The display of the tab</param>
        public SettingsTab(string ID, string Title, Page Display) {
            if ( string.IsNullOrWhiteSpace( ID ) )
                throw new ArgumentNullException( nameof( ID ), "The ID was null or whitespace" );
            if ( string.IsNullOrEmpty( Title ) )
                throw new ArgumentNullException( nameof( Title ), "The Title did not have any content");

            this.ID = ID;
            this.Title = Title;
            this.Display = Display ?? throw new ArgumentNullException( nameof( Display ), "The Display Page was null" );
        }

        /// <summary>
        /// Initialises a new <see cref="SettingsTab"/> with subordinates and no display
        /// </summary>
        /// <param name="ID">The ID of the <see cref="SettingsTab"/></param>
        /// <param name="Title">The title to be displayed</param>
        /// <param name="Subordinates">The tabs to be used as Subordinates</param>
        public SettingsTab(string ID, string Title, params SettingsTab[] Subordinates ) {
            if ( string.IsNullOrWhiteSpace( ID ) )
                throw new ArgumentNullException( nameof( ID ), "The ID was null or whitespace" );
            if ( string.IsNullOrEmpty( Title ) )
                throw new ArgumentNullException( nameof( Title ), "The Title did not have any content" );
            if ( Subordinates.Length == 0 )
                throw new ArgumentException( "No subordinates were supplied", nameof( Subordinates ) );

            this.ID = ID;
            this.Title = Title;

            foreach ( SettingsTab st in Subordinates )
                if ( st.ID == Parent.ID )
                    throw new InvalidOperationException( "Cannot set the tab's parent as a subordinate" );
                else if ( st.ID == ID )
                    throw new InvalidOperationException( "Cannot set self as subordinate" );
                else {
                    this.Subordinates.Add( st.ID );
                    Registry.RegisterSettings( st ); //This is so that no subordinates are missing
                }
        }

        /// <summary>
        /// Initialises a new <see cref="SettingsTab"/> with subordinates and no display
        /// </summary>
        /// <param name="ID">The ID of the <see cref="SettingsTab"/></param>
        /// <param name="Title">The title to be displayed</param>
        /// <param name="Display">The display of the tab</param> 
        /// <param name="Subordinates">The tabs to be used as Subordinates</param>
        public SettingsTab(string ID, string Title, Page Display, params SettingsTab[] Subordinates) {
            if ( string.IsNullOrWhiteSpace( ID ) )
                throw new ArgumentNullException( nameof( ID ), "The ID was null or whitespace" );
            if ( string.IsNullOrEmpty( Title ) )
                throw new ArgumentNullException( nameof( Title ), "The Title did not have any content" );
            if ( Subordinates.Length == 0 )
                throw new ArgumentException( "No subordinates were supplied", nameof( Subordinates ) );

            this.Display = Display ?? throw new ArgumentNullException( nameof( Display ), "The Display Page was null" );
            this.ID = ID;
            this.Title = Title;

            foreach ( SettingsTab st in Subordinates )
                if ( st.ID == Parent.ID )
                    throw new InvalidOperationException( "Cannot set the tab's parent as a subordinate" );
                else if ( st.ID == ID )
                    throw new InvalidOperationException( "Cannot set self as subordinate" );
                else {
                    this.Subordinates.Add( st.ID );
                    Registry.RegisterSettings( st ); //This is so that no subordinates are missing
                }
        }

        /// <summary>
        /// Sets the title to the provided string
        /// </summary>
        /// <param name="NewTitle">The text to set the title to</param>
        public void SetTitle(string NewTitle) {
            if ( string.IsNullOrEmpty( NewTitle ) )
                throw new ArgumentNullException( nameof( NewTitle ), "The provided text had no content");
            Title = NewTitle;
        }
    }
}
