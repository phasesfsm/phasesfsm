using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class End : Link
    {
        public static readonly int smallRadio = 8;
        public static readonly int radio = 12;
        public static readonly int selectionRadio = 12;
        public override int Radio { get { return radio; } }
        public override int SelectionRadio { get { return selectionRadio; } }

        public End(DrawableCollection ownerDraw, Point location)
            : base(ownerDraw, location)
        {
            this.location = location;
        }

        public End(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            g.FillEllipse(Brushes.White, location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
            g.DrawEllipse(att.Pen, location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
            g.FillEllipse(new SolidBrush(att.Pen.Color), location.X - smallRadio, location.Y - smallRadio, smallRadio * 2, smallRadio * 2);
        }
    }
}
