using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    abstract class State : DrawableObject, IState
    {
        public static readonly int transitionVector = 50;
        public Rectangle rect;
        public List<string> enterOutput = new List<string>();
        public List<string> exitOutput = new List<string>();

        public State(DrawableCollection ownerDraw, Rectangle startRect)
            : base(ownerDraw)
        {
            rect = startRect;
        }

        public State(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        public static State Create(ObjectType objectType, DrawableCollection ownerDraw, Rectangle startRect)
        {
            switch (objectType)
            {
                case ObjectType.SimpleState:
                    return new SimpleState(ownerDraw, startRect);
                case ObjectType.StateAlias:
                    return new StateAlias(ownerDraw, startRect);
                case ObjectType.SuperState:
                    return new SuperState(ownerDraw, startRect);
                case ObjectType.Nested:
                    return new Nested(ownerDraw, startRect);
                default:
                    throw new Exception("Invalid State type.");
            }
        }

        public override string Name {
            set
            {
                if (!(this is StateAlias) && value == GetFormName())
                {
                    MessageBox.Show(string.Format("'{0}' is a reserved name.", value), "Value error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                base.Name = value;
            }
        }

        [Category("Simulation")]
        public bool Track { get; set; } = false;

        protected void AdjustSize()
        {
            Point sizeRef = Point.Empty;
            ResizeCheck(ref sizeRef, MouseTool.ResizingTypes.Right_Bottom);
            Resize(sizeRef, MouseTool.ResizingTypes.Right_Bottom);
            foreach (Transition oTransition in InTransitions)
            {
                oTransition.MoveEndTo(PointFromAngle(oTransition.EndAngle));
            }
            foreach (Transition oTransition in OutTransitions)
            {
                oTransition.MoveStartTo(PointFromAngle(oTransition.StartAngle));
            }
        }

        [Description("Output when the state is activated."), Category("Logics")]
        [Editor(typeof(Phases.PropertiesCoverters.OutputsEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(PropertiesCoverters.NullCoverter))]
        public virtual string EnterOutput
        {
            get
            {
                return string.Join(", ", enterOutput.Select(item => item));
            }
            set
            {
                if (value == null) enterOutput = new List<string>();
                else enterOutput = new List<string>(value.Split(','));
            }
        }

        [Browsable(false)]
        public virtual List<Alias> Aliases
        {
            get
            {
                return OwnerDraw.Aliases.FindAll(alias => alias.PointingTo == Name);
            }
        }

        [Category("Transitions")]
        public Transition[] AllOutTransitions
        {
            get
            {
                if (Aliases == null) return null;
                List<Transition> list = new List<Transition>(outTransitions);
                Aliases.ForEach(alias => list.AddRange(alias.OutTransitions));
                return list.ToArray();
            }
        }

        [Browsable(false)]
        public List<string> EnterOutputsList => enterOutput;

        [Description("Output when the state is leaved."), Category("Logics")]
        [Editor(typeof(Phases.PropertiesCoverters.OutputsEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(PropertiesCoverters.NullCoverter))]
        public virtual string ExitOutput
        {
            get
            {
                return string.Join(", ", exitOutput.Select(item => item));
            }
            set
            {
                if (value == null) exitOutput = new List<string>();
                else exitOutput = new List<string>(value.Split(','));
            }
        }

        [Browsable(false)]
        public List<string> ExitOutputsList => exitOutput;

#if !DEBUG
        [Browsable(false)]
#endif
        public override Point Center => Util.Center(rect);
        
        public override string Text
        {
            get
            {
                if(EnterOutputsList.Count == 0 && ExitOutputsList.Count == 0)
                {
                    return Name;
                }
                else if(EnterOutputsList.Count == 0)
                {
                    return Name + Environment.NewLine + "(" + ExitOutput + ")⇗";
                }
                else if(ExitOutputsList.Count == 0)
                {
                    return Name + Environment.NewLine + "⇘(" + EnterOutput + ")";
                }
                else
                {
                    return Name + Environment.NewLine + "⇘(" + EnterOutput + ")" + Environment.NewLine + "(" + ExitOutput + ")⇗";
                }
            }
        }

        protected override void DrawText(Graphics g, Brush brush)
        {
            if (Text.Contains(Environment.NewLine))
            {
                Size size = TextRenderer.MeasureText(Text, font);
                int lines = Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length;
                int lineHeight = lines * size.Height / (2 * lines) - size.Height / lines + 2;
                g.DrawLine(new Pen(brush), Center.X - size.Width / 2, Center.Y - lineHeight, Center.X + size.Width / 2, Center.Y - lineHeight);
            }
            g.DrawString(Text, font, brush, Center, TextFormat);
        }

        public override List<MouseTool.SelectionRectangle> DrawSelection(Graphics g, bool focused)
        {
            Rectangle sel = rect;
            sel.Inflate(5, 5);
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

        public override void DrawingRectangle(Point startPoint, Point endPoint)
        {
            rect = MouseTool.GetRectangle(startPoint, endPoint);
        }

        public override Rectangle GetSelectionRectangle()
        {
            return rect;
        }

        public virtual bool IsBorderPoint(Point position)
        {
            float x, y, h, k, rx, ry;
            double d;
            x = position.X;
            y = position.Y;
            rx = rect.Width / 2f + 5f;
            ry = rect.Height / 2f + 5f;
            h = rect.X + rx;
            k = rect.Y + ry;
            d = Math.Pow(x - h, 2) / Math.Pow(rx, 2) + Math.Pow(y - k, 2) / Math.Pow(ry, 2);
            return d >= 0.5d && d <= 1d;
        }

        public override SuperState Father
        {
            get
            {
                SuperState father = null;
                foreach (SuperState superState in OwnerDraw.SuperStates)
                {
                    if (superState != this && superState.Contains(this) && !superState.Contains(father)) father = superState;
                }
                return father;
            }
        }

        public override bool HasFather(SuperState father)
        {
            SuperState afather = Father;
            while (!(afather is null) && afather != father)
            {
                afather = afather.Father;
            }
            return father == afather;
        }

        public virtual bool Contains(State other)
        {
            if (other == null) return false;
            return rect.Contains(other.rect);
        }

        public virtual bool Contains(Link other)
        {
            return rect.Contains(other.Location);
        }

        public override bool IsCovered(Rectangle area)
        {
            return area.Contains(rect);
        }

        public override bool IsSelectable(Rectangle area)
        {
            return area.IntersectsWith(rect);
        }

        public override bool IsSelectablePoint(Point position)
        {
            float x, y, h, k, rx, ry;
            x = position.X;
            y = position.Y;
            rx = rect.Width / 2f + 5f;
            ry = rect.Height / 2f + 5f;
            h = rect.X + rx;
            k = rect.Y + ry;
            return Math.Pow(x - h, 2) / Math.Pow(rx, 2) + Math.Pow(y - k, 2) / Math.Pow(ry, 2) <= 1;
        }

        public override void Move(Point offset)
        {
            rect.Offset(offset);
        }

        public override void Resize(Point offset, MouseTool.ResizingTypes dir)
        {
            if((dir & MouseTool.ResizingTypes.Left) != 0)
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
            Size textOffset = new Size(15, 15 * (Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length));
            Size textSize = TextRenderer.MeasureText(Text, font);
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

        public override void Intersect(Point position, ref Point point, ref double angle)
        {
            Point res;
            float ox, oy, rx, ry;
            double r, px, py;
            rx = rect.Width / 2f;
            ry = rect.Height / 2f;
            ox = rect.X + rx;
            oy = rect.Y + ry;
            angle = Util.GetAngle(Util.Center(rect), position);
            r = 1 / Math.Sqrt(Math.Pow(Math.Cos(angle), 2) / Math.Pow(rx, 2) + Math.Pow(Math.Sin(angle), 2) / Math.Pow(ry, 2));
            px = r * Math.Cos(angle);
            py = r * Math.Sin(angle);
            res = Point.Round(new PointF((float)px, (float)py));
            res.Offset((int)ox, (int)oy);
            point = res;
        }

        public Point PointFromAngle(double angle)
        {
            Point res;
            float ox, oy, rx, ry;
            double r, px, py;
            rx = rect.Width / 2f;
            ry = rect.Height / 2f;
            ox = rect.X + rx;
            oy = rect.Y + ry;
            r = 1 / Math.Sqrt(Math.Pow(Math.Cos(angle), 2) / Math.Pow(rx, 2) + Math.Pow(Math.Sin(angle), 2) / Math.Pow(ry, 2));
            px = r * Math.Cos(angle);
            py = r * Math.Sin(angle);
            res = Point.Round(new PointF((float)px, (float)py));
            res.Offset((int)ox, (int)oy);
            return res;
        }

        public override Point OutDir(Point position, out double angle)
        {
            Point center = Util.Center(rect);
            angle = Util.GetAngle(center, position);
            return Util.GetPoint(center, Util.Distance(center, position) + transitionVector, Util.GetAngle(center, position));
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            State state = (State)obj;
            state.rect = rect;
            state.inTransitions = new List<Transition>(inTransitions);
            state.enterOutput = enterOutput;
            state.exitOutput = exitOutput;
        }

        public override object Clone()
        {
            var obj = (State)MemberwiseClone();
            CopyTo(obj);
            return obj;
        }

        #region "Serialization"

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>();
            //Add parameters
            data.AddRange(SerializeObjectId());
            data.AddRange(Serialization.SerializeParameter(rect));
            data.AddRange(Serialization.SerializeParameter(enterOutput.ToArray()));
            data.AddRange(Serialization.SerializeParameter(exitOutput.ToArray()));
            data.AddRange(Serialization.SerializeParameter(Track));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if (!Serialization.DeserializeParameter(data, ref index, out rect)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, out string[] enterOutputs)) return false;
            enterOutput = new List<string>(enterOutputs);
            if (!Serialization.DeserializeParameter(data, ref index, out string[] exitOutputs)) return false;
            exitOutput = new List<string>(exitOutputs);
            if (Serialization.DeserializeParameter(data, ref index, out bool track)) Track = track;
            return true;
        }
        #endregion
    }
}
