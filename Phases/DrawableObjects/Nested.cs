using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class Nested : State, INestedState
    {
        public static readonly int SelectionBorderMargin = 5;
        public string pointing = "";

        public Nested(DrawableCollection ownerDraw, Rectangle startRect)
            : base(ownerDraw, startRect)
        {
            rect = startRect;
        }

        public Nested(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

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
                if(PointedSheet != null)
                {
                    if (PointedSheet.draw.Origins.Count > 0)
                        return PointedSheet.draw.Origins.First();
                }
                return null;
            }
        }

        public List<DrawableObject> ContainedObjects
        {
            get
            {
                return null;
            }
        }

        public override string Text
        {
            get
            {
                if (pointing == "") return name;
                else return name + " -> " + pointing;
            }
        }

        [Description("The object that activates this transition."), Category("General"), Browsable(true), TypeConverter(typeof(PropertiesCoverters.SheetsInBookConverter))]
        public string PointingTo
        {
            get
            {
                return pointing;
            }
            set
            {
                pointing = value;
            }
        }

        public DrawingSheet PointedSheet
        {
            get
            {
                return OwnerDraw.OwnerSheet.OwnerBook.Sheets.FirstOrDefault(sh => sh.Name == pointing);
            }
        }

        public override void DrawBack(Graphics g)
        {
            base.DrawBack(g);
            g.FillRectangle(Brushes.LightYellow, rect);
        }

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            g.DrawRectangle(att.Pen, rect);
        }

        public override void DrawingRectangle(Point startPoint, Point endPoint)
        {
            rect = MouseTool.GetRectangle(startPoint, endPoint);
        }

        public override Rectangle GetSelectionRectangle()
        {
            return rect;
        }

        public override bool Contains(State other)
        {
            var sheet = OwnerDraw.OwnerSheet.OwnerBook.Sheets.FirstOrDefault(sh => sh.Name == pointing);
            if (sheet != null)
            {
                return sheet.draw.objects.Contains(other);
            }
            else
            {
                return false;
            }
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
            //if (IsTextSelectable(position)) return true;
            return rect.Contains(position);
        }

        public override void Intersect(Point position, ref Point point, ref double angle)
        {
            Rectangle exterior = rect, interior = rect;
            exterior.Inflate(3, 3);
            interior.Inflate(-Math.Max(10, rect.Width/2), -Math.Max(10, rect.Width / 2));
            if (!exterior.Contains(position) || interior.Contains(position)) return;
            int left, right, top, bottom;
            left = position.X - rect.Left;
            right = rect.Right - position.X;
            top = position.Y - rect.Top;
            bottom = rect.Bottom - position.Y;
            if(left < right)
            {
                if(top < bottom)
                {
                    if(left < top)
                    {
                        point = new Point(rect.Left, position.Y);
                    }
                    else if (top < left)
                    {
                        point = new Point(position.X, rect.Top);
                    }
                    else
                    {
                        point = new Point(rect.Left, rect.Top);
                    }
                }
                else
                {
                    if (left < bottom)
                    {
                        point = new Point(rect.Left, position.Y);
                    }
                    else if (bottom < left)
                    {
                        point = new Point(position.X, rect.Bottom);
                    }
                    else
                    {
                        point = new Point(rect.Left, rect.Bottom);
                    }
                }
            }
            else
            {
                if (top < bottom)
                {
                    if (right < top)
                    {
                        point = new Point(rect.Right, position.Y);
                    }
                    else if (top < right)
                    {
                        point = new Point(position.X, rect.Top);
                    }
                    else
                    {
                        point = new Point(rect.Right, rect.Top);
                    }
                }
                else
                {
                    if (right < bottom)
                    {
                        point = new Point(rect.Right, position.Y);
                    }
                    else if (bottom < right)
                    {
                        point = new Point(position.X, rect.Bottom);
                    }
                    else
                    {
                        point = new Point(rect.Right, rect.Bottom);
                    }
                }
            }
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
            var nested = (Nested)obj;
            nested.enterOutput = enterOutput;
            nested.exitOutput = exitOutput;
        }

        public List<Link> ContainedTransitionLinks()
        {
            var list = new List<Link>();
            DrawingSheet sheet = OwnerDraw.OwnerSheet.OwnerBook.Sheets.FirstOrDefault(sh => sh.Name == pointing);
            if (sheet == null) return list;

            return sheet.draw.Objects.FindAll(obj => obj is End || obj is Abort).ConvertAll(link => (Link)link);
        }

        public string[] ContainedTransitionLinksNames()
        {
            return ContainedTransitionLinks().ConvertAll(obj => obj.Name).Distinct().ToArray();
        }

        #region "Serialization"

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>(base.SerializeSpecifics());
            //Add pointing sheet
            data.AddRange(Serialization.SerializeParameter(pointing));
            data.AddRange(Serialization.SerializeParameter((byte)priority));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            byte bt = 0;
            if (!base.DeserializeObjectSpecifics(data, ref index)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref pointing)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref bt)) return false;
            priority = (NestedPriority)bt;
            return true;
        }
        #endregion
    }
}
