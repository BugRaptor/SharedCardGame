using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Diagnostics;

namespace SharedCardGame
{
    class MouseEventTransparentVectorControl: MouseEventTransparentControl 
    {

        private Pen pen = null;
        public Point From { get; set; }
        public Point To { get; set; }
    

        public MouseEventTransparentVectorControl(Point from, Point to): base()
        {
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.ResizeRedraw, true);
            From = from;
            To = to;
        }

        public void UpdateLocation()
        {
            Location = new Point(Math.Min(From.X, To.X), Math.Min(From.Y, To.Y));
            Size = new Size(Math.Max(From.X, To.X) - Location.X, Math.Max(From.Y, To.Y) - Location.Y);            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (pen == null)
            {
                pen = new Pen(Color.Ivory, 3);
            }

            Rectangle toDim = ClientRectangle;

            e.Graphics.DrawLine(pen, new Point(From.X - Left, From.Y - Top), new Point(To.X - Left, To.Y - Top));
            e.Graphics.Flush();
        }
    }
}
