using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class Text : DrawableObject
    {
        public Rectangle rect;
        public Color BackColor => Color.LightYellow;

        public Text(DrawableCollection ownerDraw, Rectangle startRect)
            : base(ownerDraw)
        {
            rect = startRect;
        }

        public Text(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        public override Point Center => Util.Center(rect);
        public override SuperState Father => null;

        public override string Description
        {
            set
            {
                base.Description = value;
                //AdjustSize();
            }
        }

        public override void DrawingRectangle(Point startPoint, Point endPoint)
        {
            rect = Util.GetRectangle(startPoint, endPoint);
        }

        protected void AdjustSize()
        {
            Point sizeRef = Point.Empty;
            ResizeCheck(ref sizeRef, MouseTool.ResizingTypes.Right_Bottom);
            Resize(sizeRef, MouseTool.ResizingTypes.Right_Bottom);
        }

        public override List<MouseTool.SelectionRectangle> DrawSelection(Graphics g, bool focused)
        {
            Rectangle sel = rect;
            sel.Inflate(2, 2);
            List<MouseTool.SelectionRectangle> srs = new List<MouseTool.SelectionRectangle>();
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Left_Top, sel.Location, focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Top, new Point(sel.X + sel.Width / 2, sel.Y), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Right_Top, new Point(sel.Right, sel.Y), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Left, new Point(sel.X, sel.Y + sel.Height / 2), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Right, new Point(sel.Right, sel.Y + sel.Height / 2), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Left_Bottom, new Point(sel.X, sel.Bottom), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Bottom, new Point(sel.X + sel.Width / 2, sel.Bottom), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Right_Bottom, new Point(sel.Right, sel.Bottom), focused));
            return srs;
        }

        public override void Resize(Point offset, MouseTool.ResizingTypes dir)
        {
            if ((dir & MouseTool.ResizingTypes.Left) != 0)
            {
                if (rect.Width - offset.X >= 0)
                {
                    rect.X += offset.X;
                    rect.Width -= offset.X;
                }
            }
            if ((dir & MouseTool.ResizingTypes.Right) != 0)
            {
                if (rect.Width + offset.X >= 0)
                {
                    rect.Width += offset.X;
                }
            }
            if ((dir & MouseTool.ResizingTypes.Top) != 0)
            {
                if (rect.Height - offset.Y >= 0)
                {
                    rect.Y += offset.Y;
                    rect.Height -= offset.Y;
                }
            }
            if ((dir & MouseTool.ResizingTypes.Bottom) != 0)
            {
                if (rect.Height + offset.Y >= 0)
                {
                    rect.Height += offset.Y;
                }
            }
        }

        public override void ResizeCheck(ref Point offset, MouseTool.ResizingTypes dir)
        {
            Size textOffset = new Size(0, 0);
            Size textSize = new Size(10, TextRenderer.MeasureText(Description, font).Height); //TextRenderer.MeasureText(Description, font);
            if ((dir & MouseTool.ResizingTypes.Left) != 0)
            {
                if (rect.Width - offset.X < textSize.Width + textOffset.Width)
                {
                    offset.X = rect.Width - textSize.Width - textOffset.Width;
                }
            }
            if ((dir & MouseTool.ResizingTypes.Right) != 0)
            {
                if (rect.Width + offset.X < textSize.Width + textOffset.Width)
                {
                    offset.X = -rect.Width + textSize.Width + textOffset.Width;
                }
            }
            if ((dir & MouseTool.ResizingTypes.Top) != 0)
            {
                if (rect.Height - offset.Y < textSize.Height + textOffset.Height)
                {
                    offset.Y = rect.Height - textSize.Height - textOffset.Height;
                }
            }
            if ((dir & MouseTool.ResizingTypes.Bottom) != 0)
            {
                if (rect.Height + offset.Y < textSize.Height + textOffset.Height)
                {
                    offset.Y = -rect.Height + textSize.Height + textOffset.Height;
                }
            }
        }

        public override Rectangle GetSelectionRectangle()
        {
            Rectangle srec = rect;
            srec.Inflate(3, 3);
            return srec;
        }

        public override bool HasFather(SuperState machine)
        {
            return false;
        }

        public override bool IsCovered(Rectangle area)
        {
            return area.Contains(GetSelectionRectangle());
        }

        public override bool IsSelectable(Rectangle area)
        {
            return area.IntersectsWith(GetSelectionRectangle()) && !rect.Contains(area);
        }

        public override bool IsSelectablePoint(Point position)
        {
            return GetSelectionRectangle().Contains(position);
        }

        public override void Move(Point offset)
        {
            rect.Offset(offset);
        }

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            if(!att.IsShadow) g.FillRectangle(new SolidBrush(BackColor), rect);
            g.DrawRectangle(att.Pen, rect);
        }

        protected override StringFormat TextFormat { get; } = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
        protected override void DrawText(Graphics g, Brush brush)
        {
            g.DrawString(Description, font, brush, rect, TextFormat);
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            Text text = (Text)obj;
            text.rect = rect;
        }

        public override object Clone()
        {
            var obj = (Text)MemberwiseClone();
            CopyTo(obj);
            return obj;
        }

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>();
            //Add parameters
            data.AddRange(SerializeObjectId());
            data.AddRange(Serialization.SerializeParameter(rect));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if (!Serialization.DeserializeParameter(data, ref index, out rect)) return false;
            return true;
        }
    }
}
