using System.Windows.Forms;

namespace SharedCardGame
{
    /// <summary>
    /// Interface IMouseEventTransparentControl must be implemented by all MouseEvent Transparent Controls
    /// </summary>
    interface IMouseEventTransparentControl
    {
        /// <summary>
        /// Redirects the mouse events to other MouseEventHandlers
        /// </summary>
        /// <param name="mouseDownEventHandler">The mouse down event handler.</param>
        /// <param name="mouseMoveEventHandler">The mouse move event handler.</param>
        /// <param name="mouseUpEventHandler">The mouse up event handler.</param>
        void RedirectMouseEvents(MouseEventHandler mouseDownEventHandler, 
            MouseEventHandler mouseMoveEventHandler,
            MouseEventHandler mouseUpEventHandler);

        /// <summary>
        /// Resets the mouse events redirections.
        /// </summary>
        void ResetMouseEventsRedirection();
    }
}
