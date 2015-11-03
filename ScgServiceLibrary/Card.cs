namespace ScgServiceLibrary
{
    /// <summary>
    /// Class Card. Represents a playing card as a game element.
    /// </summary>
    class Card: GameElement, IFlippable, IStackable
    {
        #region Private Fields
        private bool _faceUp;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Card" /> class.
        /// </summary>
        /// <param name="rectoPicture">The recto picture.</param>
        /// <param name="versoPicture">The verso picture.</param>
        /// <param name="stackedPicturePrefix">The stacked picture.</param>
        /// <param name="faceUp">if set to <c>true</c> [face up].</param>
        public Card(string rectoPicture, string versoPicture, string stackedPicturePrefix, bool faceUp)
        {
            RectoPicture = rectoPicture;
            VersoPicture = versoPicture;
            StackedPicturePrefix = stackedPicturePrefix;
            VisiblePicture = versoPicture;
            FaceUp = faceUp;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the recto picture of a Card game element.
        /// </summary>
        /// <value>The recto picture.</value>
        public string RectoPicture { get; set; }

        /// <summary>
        /// Gets or sets the verso picture of a Card game element.
        /// </summary>
        /// <value>The verso picture.</value>
        public string VersoPicture { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Card is visible face up (Recto face).
        /// </summary>
        /// <value><c>true</c> if visible face up (Recto face); otherwise, <c>false</c>.</value>
        public bool FaceUp
        {
            get { return _faceUp; }
            set
            {
                if (_faceUp != value)
                {
                    _faceUp = value;
                    VisiblePicture = _faceUp
                        ? RectoPicture
                        : VersoPicture;
                }
            }
        }

        /// <summary>
        /// Gets or sets the visible picture representing a stack of cards when many cards are stacked as stacked game element.
        /// </summary>
        /// <value>The stacked picture.</value>
        public string StackedPicturePrefix { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Flips this instance to present the other face.
        /// </summary>
        public void Flip()
        {
            FaceUp = !FaceUp;
        }
        #endregion
    }
}
