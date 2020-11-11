using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using NanaManagerAPI.Properties;

namespace NanaManagerAPI.UI
{
    public static class UI
    {
        /// <summary>
        /// A delegate for setting a boolean value
        /// </summary>
        /// <param name="Set">The value to set</param>
        public delegate void SetterDelegate( bool Set );

        /// <summary>
        /// A delegate for loading media
        /// </summary>
        /// <param name="Current">The media to load</param>
        /// <param name="Search">The search terms used</param>
        /// <param name="Index">Where in the list it is</param>
        public delegate void LoadMedia( string Current, int[] Search, int[] Reject, int Index );
        /// <summary>
        /// An event handler for when the program's status changes
        /// </summary>
        /// <param name="Status">The text to display</param>
        /// <param name="Progress">Progress into completing the task. -1 if indeterminate</param>
        /// <param name="Maximum">Target progress value. Can be null if indeterminate</param>
        public delegate void StatusChange( string Status, double Progress, double? Maximum );
        /// <summary>
        /// An event handler for loading a piece of media
        /// </summary>
        /// <param name="ID">The ID of the file</param>
        public delegate void MediaLoader( string ID, bool Editing );
        /// <summary>
        /// Encapsulates delegates that process notifications when sent
        /// </summary>
        /// <param name="Text">The text to display with the notification</param>
        /// <param name="Additional">Any additional information to pass to the Handlers</param>
        public delegate void NotificationHandler( string Text, object[] Additional );
        
        /// <summary>
        /// Fires whenever a notification is to be displayed
        /// </summary>
        public static event NotificationHandler NotificationRaised;
        /// <summary>
        /// The event for when the program's status changes
        /// </summary>
        public static event StatusChange StatusChanged;
        /// <summary>
        /// Raises whenever the screen mode is to be changed
        /// </summary>
        public static event SetterDelegate Fullscreen;
        /// <summary>
        /// Raises whenever a media object is to be displayed
        /// </summary>
        public static event LoadMedia MediaOpened;
        /// <summary>
        /// Raises whenever the application is closed
        /// </summary>
        public static event Action WindowClosed;
        /// <summary>
        /// Raises whenever the <see cref="IsLightTheme"/> property is set
        /// </summary>
        public static event SetterDelegate ThemeLightnessChanged;

        /// <summary>
        /// The image displayed over audio files without an album cover
        /// </summary>
        public static BitmapImage AudioSymbol;

        private static bool lightTheme;
        /// <summary>
        /// Determines whether the current theme is a light theme. Changes certain static elements to fit light themes better
        /// </summary>
        public static bool IsLightTheme { 
            set
            {
                ThemeLightnessChanged?.Invoke( value );
                lightTheme = value;
            }
            get => lightTheme; }
        public static BitmapImage LogoDark = Resources.Nana_Manager_Icon_Dark.ToBitmapImage( BitmapCacheOption.OnLoad );
        public static BitmapImage LogoLight = Resources.Nana_Manager_Icon_Light.ToBitmapImage( BitmapCacheOption.OnLoad );

        /// <summary>
        /// Raises a notification to be displayed
        /// </summary>
        /// <param name="Message">The message to display as the notification</param>
        /// <param name="Additional">Any additional information to pass</param>
        public static void RaiseNotification( string Message, params object[] Additional ) => NotificationRaised?.Invoke( Message, Additional );

        /// <summary>
        /// Sets the status with an indeterminate amount of time remaining
        /// </summary>
        /// <param name="Status">The status message</param>
        public static void SetStatus( string Status ) => StatusChanged?.Invoke( Status, -1, null );
        /// <summary>
        /// Sets the status with progress monitored
        /// </summary>
        /// <param name="Status">The status message</param>
        /// <param name="Progress">How far into the operation</param>
        /// <param name="Maximum">The target value for Progress</param>
        public static void SetStatus( string Status, double Progress, double Maximum ) => StatusChanged?.Invoke( Status, Progress, Maximum );
        /// <summary>
        /// Sets the application into fullscreen if true
        /// </summary>
        /// <param name="Value">Whether to use Fullscreen or Windowed</param>
        public static void SetFullscreen( bool Value ) => Fullscreen?.Invoke( Value );
        
        /// <summary>
        /// Opens the specified media with any search queries and the index the media is found in said search
        /// </summary>
        /// <param name="Current">The media to load</param>
        /// <param name="Search">The search terms used</param>
        /// <param name="Index">Where in the list the media is</param>
        public static void OpenMedia( string Current, int[] Search, int[] Reject, int Index ) => MediaOpened?.Invoke( Current, Search, Reject, Index );

        /// <summary>
        /// Instructs the application to close
        /// </summary>
        public static void CloseApplication() => WindowClosed?.Invoke();
    }
}
