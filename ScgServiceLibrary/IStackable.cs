namespace ScgServiceLibrary
{
    /// <summary>
    /// Interface IStackable shall be implemented by game elements that can be stacked
    /// </summary>
    interface IStackable
    {
        /// <summary>
        /// Gets or sets the visible picture representing a stack of game elements when many game elements are stacked.
        /// </summary>
        /// <value>The stacked picture.</value>
        string StackedPicturePrefix { get; set; }
    }
}
