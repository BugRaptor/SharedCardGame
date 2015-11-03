using System.Drawing;
using System.Windows.Forms;

namespace SharedCardGame
{
    class MouseEventTransparentGatheringRectangleControl : MouseEventTransparentControl
    {

        private Pen _pen;
        private Rectangle _rectangle;
 
        public Rectangle Rectangle 
        {
            get { return _rectangle; }
            set
            {
                _rectangle = value;
                Location = Rectangle.Location;
                Size = Rectangle.Size;
                Invalidate();
            } 
        }

        public MouseEventTransparentGatheringRectangleControl(Rectangle rectangle)
        {
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.ResizeRedraw, true);
            _rectangle = rectangle;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_pen == null)
            {
                _pen = new Pen(Color.Black, 1);
            }

            Rectangle toDim = ClientRectangle;

            e.Graphics.DrawRectangle(_pen,new Rectangle(toDim.Location, new Size(toDim.Width -1, toDim.Height-1)));
            e.Graphics.Flush();
        }
    }

}
