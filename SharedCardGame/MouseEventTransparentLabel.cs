using System.Windows.Forms;

namespace SharedCardGame
{
    sealed class MouseEventTransparentLabel: Label, IMouseEventTransparentControl
    {
        private MouseEventHandler _onMouseDown;
        private MouseEventHandler _onMouseMove;
        private MouseEventHandler _onMouseUp;

        /// <summary>
        /// Redirects the mouse events to other MouseEventHandlers
        /// </summary>
        /// <param name="mouseDownEventHandler">The mouse down event handler.</param>
        /// <param name="mouseMoveEventHandler">The mouse move event handler.</param>
        /// <param name="mouseUpEventHandler">The mouse up event handler.</param>
        public void RedirectMouseEvents(MouseEventHandler mouseDownEventHandler, MouseEventHandler mouseMoveEventHandler, MouseEventHandler mouseUpEventHandler)
        {
            _onMouseDown = mouseDownEventHandler;
            _onMouseMove = mouseMoveEventHandler;
            _onMouseUp = mouseUpEventHandler;
            if (_onMouseDown != null)
            {
                MouseDown += _onMouseDown;
            }
            if (_onMouseMove != null)
            {
                MouseMove += _onMouseMove;
            }
            if (_onMouseUp != null)
            {
                MouseUp += _onMouseUp;
            }
        }

        /// <summary>
        /// Resets the mouse events redirections.
        /// </summary>
        public void ResetMouseEventsRedirection()
        {
            if (_onMouseDown != null)
            {
                MouseDown -= _onMouseDown;
            }
            if (_onMouseMove != null)
            {
                MouseMove -= _onMouseMove;
            }
            if (_onMouseUp != null)
            {
                MouseUp -= _onMouseUp;
            }
            _onMouseDown = null;
            _onMouseMove = null;
            _onMouseUp = null;
        }


        public MouseEventTransparentLabel()
        {
            DoubleBuffered = true;
        }

        //// In order do make this control "transparent" to mouse events
        //// The mouse events will not be sent to this control when the mouse cursor
        //// is over it. They will be sent to the owner control.
        //protected override void WndProc(ref Message m)
        //{
        //    const int WM_NCHITTEST = 0x0084;
        //    const int HTTRANSPARENT = (-1);

        //    if (m.Msg == WM_NCHITTEST)
        //    {
        //        m.Result = (IntPtr)HTTRANSPARENT;
        //    }
        //    else
        //    {
        //        base.WndProc(ref m);
        //    }
        //}
    }
}
