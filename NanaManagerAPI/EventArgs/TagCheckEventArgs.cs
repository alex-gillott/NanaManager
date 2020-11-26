namespace NanaManagerAPI.EventArgs
{
    /// <summary>
    /// Event Arguments for a tag toggling
    /// </summary>
    public sealed class TagCheckEventArgs : System.EventArgs
    {
        /// <summary>
        /// True if the tag was checked or unchecked
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// True if the toggle was a rejection
        /// </summary>
        public bool Rejection;

        /// <summary>
        /// The tag that was toggled
        /// </summary>
        public int TagIndex;
    }
}