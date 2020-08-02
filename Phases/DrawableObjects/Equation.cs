using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Phases.Simulation;

namespace Phases.DrawableObjects
{
    class Equation : DrawableObject, IGlobal
    {
        public Rectangle rect;
        private string result = "", operation = "";

        public Equation(DrawableCollection ownerDraw, Rectangle startRect)
            : base(ownerDraw)
        {
            rect = startRect;
        }

        public Equation(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        [Category("General")]
        public int Priority
        {
            get
            {
                return OwnerDraw.OwnerSheet.Globals.IndexOf(this);
            }
            set
            {
                if (value < 0) value = 0;
                else if (value >= OwnerDraw.OwnerSheet.Globals.Count) value = OwnerDraw.OwnerSheet.Globals.Count - 1;
                OwnerDraw.OwnerSheet.Globals.Remove(this);
                OwnerDraw.OwnerSheet.Globals.Insert(value, this);
            }
        }

        [Description("Output variable to execute the action."), Category("Logics")]
        [Browsable(true), TypeConverter(typeof(PropertiesCoverters.IndirectOutputsList))]
        public string AssignTo {
            get
            {
                return result;
            }
            set
            {
                result = value;
                AdjustSize();
            }
        }

        [Description("Condition for the equation."), Category("Logics")]
        [Editor(typeof(Phases.PropertiesCoverters.ConditionalEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(PropertiesCoverters.NullCoverter))]
        public string Operation
        {
            get
            {
                return operation;
            }
            set
            {
                operation = value;
                AdjustSize();
            }
        }

        public override Point Center => Util.Center(rect);
        public override SuperState Father => null;

        public override string Description
        {
            set
            {
                base.Description = value;
            }
        }

        public override void DrawingRectangle(Point startPoint, Point endPoint)
        {
            rect = MouseTool.GetRectangle(startPoint, endPoint);
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
            Size textSize = TextRenderer.MeasureText(Text, font);
            Size textOffset = new Size(10, 15);
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
            if(!att.IsShadow) g.FillRectangle(Brushes.LightYellow, rect);
            g.DrawRectangle(att.Pen, rect);
        }

        public override void DrawSimulationMark(Graphics g)
        {
            Brush brush;
            switch (SimulationMark)
            {
                case SimulationMark.ExecutingObject:
                    brush = Marks.ExecutingObjectBrush;
                    break;
                default:
                    return;
            }
            Rectangle r = rect;
            r.Inflate(5, 5);
            g.FillRectangle(brush, r);
        }

        public override string Text
        {
            get
            {
                if (string.IsNullOrEmpty(Operation) && string.IsNullOrEmpty(AssignTo)) return "";
                else return AssignTo + " = " + Operation;
            }
        }

        protected override StringFormat TextFormat { get; } = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        protected override void DrawText(Graphics g, Brush brush)
        {
            if (SimulationMark == SimulationMark.ExecutingObject)
            {
                g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(Center, Text, font, TextFormat));
            }
            g.DrawString(Text, font, brush, rect, TextFormat);
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            Equation eq = (Equation)obj;
            eq.rect = rect;
            eq.result = result;
            eq.operation = operation;
        }

        public override object Clone()
        {
            var obj = (Equation)MemberwiseClone();
            CopyTo(obj);
            return obj;
        }

        #region "Serialization"

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>();
            data.AddRange(SerializeObjectId());
            data.AddRange(Serialization.SerializeParameter(rect));
            data.AddRange(Serialization.SerializeParameter(result));
            data.AddRange(Serialization.SerializeParameter(operation));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if (!Serialization.DeserializeParameter(data, ref index, out rect)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, out result)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, out operation)) return false;
            return true;
        }

        #endregion
    }
}
