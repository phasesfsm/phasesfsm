using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class SuperState : State, INestedState
    {
        public static readonly int SelectionBorderMargin = 5;
        Size textSize;

        public SuperState(DrawableCollection ownerDraw, Rectangle startRect)
            : base(ownerDraw, startRect)
        {
            rect = startRect;
        }

        public SuperState(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        public override string Text => Name;

        private string EnterOutputsText
        {
            get
            {
                return string.Join(Environment.NewLine, enterOutput.Select(item => item));
            }
        }

        private string ExitOutputsText
        {
            get
            {
                return string.Join(Environment.NewLine, exitOutput.Select(item => item));
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

        private NestedPriority priority;
        public NestedPriority Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
            }
        }

        public Origin Origin
        {
            get
            {
                return OwnerDraw.Origins.FirstOrDefault(origin => origin.Father == this);
            }
        }

        [Browsable(false)]
        public List<DrawableObject> ContainedObjects
        {
            get
            {
                return OwnerDraw.Objects.FindAll(obj => obj.IsSelectable(rect));
            }
        }

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            g.DrawRectangle(att.Pen, rect);
        }

        protected override StringFormat TextFormat { get; } = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far };
        protected StringFormat EnterOutputsTextFormat { get; } = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
        protected StringFormat ExitOutputsTextFormat { get; } = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };

        protected override void DrawText(Graphics g, Brush brush)
        {
            g.DrawString(Text, font, brush, rect.Location, TextFormat);
            if(EnterOutputsList.Count > 0)
            {
                Point point = new Point(rect.Left, rect.Bottom);
                g.DrawString("⇘", font, brush, point, EnterOutputsTextFormat);
                point.Offset(15, 0);
                if (SimulationMark == SimulationMark.ExecutingObjectEnterOutputs)
                {
                    g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(point, Text, font, EnterOutputsTextFormat));
                }
                g.DrawString(EnterOutputsText, font, brush, point, EnterOutputsTextFormat);
            }
            if (ExitOutputsList.Count > 0)
            {
                Point point = new Point(rect.Right, rect.Bottom);
                g.DrawString("⇗", font, brush, point, ExitOutputsTextFormat);
                point.Offset(-15, 0);
                if (SimulationMark == SimulationMark.ExecutingObjectExitOutputs)
                {
                    g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(point, Text, font, ExitOutputsTextFormat));
                }
                g.DrawString(ExitOutputsText, font, brush, point, ExitOutputsTextFormat);
            }
        }

        public override void DrawSimulationMark(Graphics g)
        {
            Brush brush;
            switch (SimulationMark)
            {
                case SimulationMark.ExecutingObjectEnterOutputs:
                    brush = Marks.ExecutingObjectBrush;
                    break;
                case SimulationMark.ExecutingObjectExitOutputs:
                    brush = Marks.ExecutingObjectBrush;
                    break;
                case SimulationMark.ExecutingObject:
                    brush = Marks.ExecutingObjectBrush;
                    break;
                case SimulationMark.TestingObject:
                    brush = Marks.TestingObjectBrush;
                    break;
                case SimulationMark.LeavingObject:
                    brush = Marks.LeavingObjectBrush;
                    break;
                default:
                    return;
            }
            Pen pen = new Pen(brush, 5f);
            Rectangle r = rect;
            r.Inflate(5, 5);
            g.DrawRectangle(pen, rect);
        }

        public override void DrawingRectangle(Point startPoint, Point endPoint)
        {
            rect = Util.GetRectangle(startPoint, endPoint);
        }

        public override Rectangle GetSelectionRectangle()
        {
            return rect;
        }

        public override bool IsBorderPoint(Point position)
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

        public override bool IsCovered(Rectangle area)
        {
            return area.Contains(GetSelectionRectangle());
        }

        public override bool IsSelectable(Rectangle area)
        {
            return area.IntersectsWith(GetSelectionRectangle()) && !rect.Contains(area);
        }

        public override bool IsTextSelectable(Point position)
        {
            Point realTextPoint = new Point(rect.X, rect.Y - textSize.Height);
            //realTextPoint.Offset(textPointOffset);
            Rectangle textRect = new Rectangle(realTextPoint, textSize);
            return textRect.Contains(position);
        }

        public override bool IsSelectablePoint(Point position)
        {
            if (IsTextSelectable(position)) return true;
            return (position.X >= rect.Left - SelectionBorderMargin && position.X <= rect.Left + SelectionBorderMargin * 2 && position.Y >= rect.Top && position.Y <= rect.Bottom)
                || (position.X >= rect.Right - SelectionBorderMargin * 2 && position.X <= rect.Right + SelectionBorderMargin && position.Y >= rect.Top && position.Y <= rect.Bottom)
                || (position.Y >= rect.Top - SelectionBorderMargin && position.Y <= rect.Top + SelectionBorderMargin * 2 && position.X >= rect.Left && position.X <= rect.Right)
                || (position.Y >= rect.Bottom - SelectionBorderMargin * 2 && position.Y <= rect.Bottom + SelectionBorderMargin && position.X >= rect.Left && position.X <= rect.Right);
        }

        public override void Intersect(Point position, ref Point point, ref double angle)
        {
            if (position.X >= rect.Left - SelectionBorderMargin && position.X <= rect.Left + SelectionBorderMargin * 2 && position.Y >= rect.Top && position.Y <= rect.Bottom)
            {
                point = new Point(rect.Left, position.Y);
            }
            else if (position.X >= rect.Right - SelectionBorderMargin * 2 && position.X <= rect.Right + SelectionBorderMargin && position.Y >= rect.Top && position.Y <= rect.Bottom)
            {
                point = new Point(rect.Right, position.Y);
            }
            else if (position.Y >= rect.Top - SelectionBorderMargin && position.Y <= rect.Top + SelectionBorderMargin * 2 && position.X >= rect.Left && position.X <= rect.Right)
            {
                point = new Point(position.X, rect.Top);
            }
            else if (position.Y >= rect.Bottom - SelectionBorderMargin * 2 && position.Y <= rect.Bottom + SelectionBorderMargin && position.X >= rect.Left && position.X <= rect.Right)
            {
                point = new Point(position.X, rect.Bottom);
            }
            OutDir(point, out angle);
        }

        public Point PointFromOffset(MouseTool.ResizingTypes resizingType, Point location, double angle)
        {
            MouseTool.ResizingTypes pointSide = (MouseTool.ResizingTypes)Math.Floor(angle);
            double dec;
            if(pointSide.HasFlag(MouseTool.ResizingTypes.Left) && pointSide.HasFlag(MouseTool.ResizingTypes.Right)
                || pointSide.HasFlag(MouseTool.ResizingTypes.Top) && pointSide.HasFlag(MouseTool.ResizingTypes.Bottom))
            {
                pointSide--;
                dec = 1d;
            }
            else
            {
                dec = angle - (double)pointSide;
            }
            Point res = location;
            if (pointSide.HasFlag(MouseTool.ResizingTypes.Left))
            {
                if (resizingType.HasFlag(MouseTool.ResizingTypes.Left))
                {
                    res.X = rect.Left;
                }
                if ((resizingType.HasFlag(MouseTool.ResizingTypes.Top) || resizingType.HasFlag(MouseTool.ResizingTypes.Bottom)) && !pointSide.HasFlag(MouseTool.ResizingTypes.Bottom))
                {
                    res.Y = rect.Y + (int)(dec * rect.Height);
                }
            }
            else if (pointSide.HasFlag(MouseTool.ResizingTypes.Right))
            {
                if (resizingType.HasFlag(MouseTool.ResizingTypes.Right))
                {
                    res.X = rect.Right;
                }
                if ((resizingType.HasFlag(MouseTool.ResizingTypes.Top) || resizingType.HasFlag(MouseTool.ResizingTypes.Bottom)) && !pointSide.HasFlag(MouseTool.ResizingTypes.Top))
                {
                    res.Y = rect.Y + (int)(dec * rect.Height);
                }
            }
            if (pointSide.HasFlag(MouseTool.ResizingTypes.Top))
            {
                if (resizingType.HasFlag(MouseTool.ResizingTypes.Top))
                {
                    res.Y = rect.Top;
                }
                if ((resizingType.HasFlag(MouseTool.ResizingTypes.Left) || resizingType.HasFlag(MouseTool.ResizingTypes.Right)) && !pointSide.HasFlag(MouseTool.ResizingTypes.Right))
                {
                    res.X = rect.X + (int)(dec * rect.Width);
                }
            }
            else if (pointSide.HasFlag(MouseTool.ResizingTypes.Bottom))
            {
                if (resizingType.HasFlag(MouseTool.ResizingTypes.Bottom))
                {
                    res.Y = rect.Bottom;
                }
                if ((resizingType.HasFlag(MouseTool.ResizingTypes.Left) || resizingType.HasFlag(MouseTool.ResizingTypes.Right)) && !pointSide.HasFlag(MouseTool.ResizingTypes.Left))
                {
                    res.X = rect.X + (int)(dec * rect.Width);
                }
            }
            return res;
        }

        public override Point OutDir(Point position, out double angle)
        {
            double dec = 0d;
            MouseTool.ResizingTypes resizing = MouseTool.ResizingTypes.None;
            if (position.X <= rect.Left)
            {
                resizing |= MouseTool.ResizingTypes.Left;
                dec = Math.Abs((double)(position.Y - rect.Y) / rect.Height);
            }
            else if (position.X == rect.Right)
            {
                resizing |= MouseTool.ResizingTypes.Right;
                dec = Math.Abs((double)(position.Y - rect.Y) / rect.Height);
            }
            if (position.Y == rect.Top)
            {
                resizing |= MouseTool.ResizingTypes.Top;
                dec = Math.Abs((double)(position.X - rect.X) / rect.Width);
            }
            else if (position.Y == rect.Bottom)
            {
                resizing |= MouseTool.ResizingTypes.Bottom;
                dec = Math.Abs((double)(position.X - rect.X) / rect.Width);
            }
            angle = (double)resizing + dec;
            Point center = Util.Center(rect);
            return Util.GetPoint(center, Util.Distance(center, position) + transitionVector, Util.GetAngle(center, position));
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            SuperState state = (SuperState)obj;
        }

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>(base.SerializeSpecifics());
            //Add parameters
            data.AddRange(Serialization.SerializeParameter((byte)priority));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if (!base.DeserializeObjectSpecifics(data, ref index)) return false;
            //Get parameters
            if (!Serialization.DeserializeParameter(data, ref index, out byte bt)) return false;
            priority = (NestedPriority)bt;
            return true;
        }

        public List<Link> ContainedLinks()
        {
            return OwnerDraw.Objects.FindAll(obj => obj is Link && rect.Contains(obj.Center)).ConvertAll(link => (Link)link);
        }

        public List<Link> ContainedTransitionLinks()
        {
            return OwnerDraw.Objects.FindAll(obj => (obj is End || obj is Abort) && rect.Contains(obj.Center)).ConvertAll(link => (Link)link);
        }

        public string[] ContainedTransitionLinksNames()
        {
            return ContainedTransitionLinks().ConvertAll(obj => obj.Name).Distinct().ToArray();
        }
    }
}
