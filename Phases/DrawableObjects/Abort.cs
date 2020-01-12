using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class Abort : Link
    {
        public static readonly int radio = 12;
        public static readonly int selectionRadio = 12;
        public override int Radio { get { return radio; } }
        public override int SelectionRadio { get { return selectionRadio; } }

        public Abort(DrawableCollection ownerDraw, Point location)
            : base(ownerDraw, location)
        {

        }

        public Abort(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            Pen pen1 = new Pen(att.Pen.Color, 2f);
            g.FillEllipse(Brushes.White, location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
            g.DrawEllipse(pen1, location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
            int side = (int)Math.Round(Radio / Math.Sqrt(2f));
            g.DrawLine(pen1, location.X - side, location.Y - side, location.X + side, location.Y + side);
            g.DrawLine(pen1, location.X - side, location.Y + side, location.X + side, location.Y - side);
        }
    }
}
