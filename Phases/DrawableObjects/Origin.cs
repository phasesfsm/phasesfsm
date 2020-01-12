using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Phases.DrawableObjects
{
    class Origin : Link, IState, IGlobal
    {
        public static readonly int radio = 5;
        public static readonly int selectionRadio = 8;
        public override int Radio { get { return radio; } }
        public override int SelectionRadio { get { return selectionRadio; } }

        [Browsable(false)]
        public List<string> EnterOutputsList => new List<string>();
        [Browsable(false)]
        public List<string> ExitOutputsList => new List<string>();

        public Origin(DrawableCollection ownerDraw, Point location)
            : base(ownerDraw, location)
        {
        }

        public Origin(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {
        }

        [Category("General")]
        public int Priority
        {
            get
            {
                return OwnerDraw.OwnerSheet.OwnerBook.Globals.IndexOf(this);
            }
            set
            {
                if (value < 0) value = 0;
                else if (value >= OwnerDraw.OwnerSheet.OwnerBook.Globals.Count) value = OwnerDraw.OwnerSheet.OwnerBook.Globals.Count - 1;
                OwnerDraw.OwnerSheet.OwnerBook.Globals.Remove(this);
                OwnerDraw.OwnerSheet.OwnerBook.Globals.Insert(value, this);
            }
        }

        public override string Name
        {
            set
            {
                if (value == GetFormName())
                {
                    MessageBox.Show(string.Format("'{0}' is a reserved name.", value), "Value error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                base.Name = value;
            }
        }

        public override string Text => Father == null ? base.Text : Father.Name;

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            g.FillEllipse(Brushes.White, location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
            g.FillEllipse(new SolidBrush(att.Pen.Color), location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
        }

        public override void DrawSelectionBack(Graphics g)
        {
            g.DrawEllipse(Pens.Gray, location.X - SelectionRadio, location.Y - SelectionRadio, SelectionRadio * 2, SelectionRadio * 2);
        }

        public override void Moved()
        {
            base.Moved();
            if (Father == null)
            {
                if (!OwnerDraw.OwnerSheet.OwnerBook.Globals.Contains(this)) OwnerDraw.OwnerSheet.OwnerBook.Globals.Add(this);
            }
            else
            {
                if (OwnerDraw.OwnerSheet.OwnerBook.Globals.Contains(this)) OwnerDraw.OwnerSheet.OwnerBook.Globals.Remove(this);
            }
        }

        public override void Intersect(Point position, ref Point point, ref double angle)
        {
            point = location;
        }

        public override Point OutDir(Point position, out double angle)
        {
            angle = 0d;
            return location;
        }

        public override Point PointFromAngle(double angle)
        {
            return location;
        }

        public SimpleTransition Transition => outTransitions.FirstOrDefault() as SimpleTransition;
    }
}
