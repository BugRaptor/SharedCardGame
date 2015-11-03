namespace ScgServiceLibrary
{
    /// <summary>
    /// Interface IFlippable shall be implemented by game elements that can be flipped. 
    /// </summary>
    interface IFlippable
    {
        /// <summary>
        /// Gets or sets the recto picture of a flippable game element.
        /// </summary>
        /// <value>The verso picture.</value>
        string RectoPicture { get; set; }
        /// <summary>
        /// Gets or sets the verso picture of a flippable game element.
        /// </summary>
        /// <value>The verso picture.</value>
        string VersoPicture { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the flippable game element is visible face up (Recto face).
        /// </summary>
        /// <value><c>true</c> if Recto face is up; otherwise, <c>false</c>.</value>
        bool FaceUp { get; set; }
        /// <summary>
        /// Flips this instance to present the other face.
        /// </summary>
        void Flip();
    }
}
