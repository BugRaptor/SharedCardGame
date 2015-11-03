namespace ScgServiceLibrary
{
    /// <summary>
    /// Class GameElement. Represents any game element associated with a SharedObject
    /// </summary>
    public class GameElement
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the visible picture of the Game Element.
        /// </summary>
        /// <value>The picture.</value>
        public string VisiblePicture { get; set; }
        #endregion

        #region Constructors
        public GameElement(string visiblePicture)
        {
            VisiblePicture = visiblePicture;
        }

        protected GameElement()
        {
            VisiblePicture = null;
        }
        #endregion
    }
}