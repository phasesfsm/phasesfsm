﻿using System;
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

        public NestedPriority Priority { get; set; }

        public Origin Origin
        {
            get
            {
                if(PointedSheet != null)
                {
                    if (PointedSheet.Sketch.Origins.Count > 0)
                        return PointedSheet.Sketch.Origins.First();
                }
                return null;
            }
        }

        public List<DrawableObject> ContainedObjects => null;

        public override string Text
        {
            get
            {
                if (PointingTo == "") return name;
                else return name + " -> " + PointingTo;
            }
        }

        [Description("The Model that this object points."), Category("General"), Browsable(true), TypeConverter(typeof(PropertiesCoverters.SheetsInBookConverter))]
        public string PointingTo { get; set; } = "";

        public ModelSheet PointedSheet
        {
            get
            {
                return OwnerDraw.OwnerSheet.OwnerBook.Models.FirstOrDefault(sh => sh.Name == PointingTo);
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
            var sheet = OwnerDraw.OwnerSheet.OwnerBook.Sheets.FirstOrDefault(sh => sh.Name == PointingTo);
            if (sheet != null)
            {
                return sheet.Sketch.objects.Contains(other);
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
            var nested = (Nested)obj;
            nested.enterOutput = enterOutput;
            nested.exitOutput = exitOutput;
        }

        public List<Link> ContainedTransitionLinks()
        {
            var list = new List<Link>();
            DrawingSheet sheet = OwnerDraw.OwnerSheet.OwnerBook.Sheets.FirstOrDefault(sh => sh.Name == PointingTo);
            if (sheet == null) return list;

            return sheet.Sketch.Objects.FindAll(obj => obj is End || obj is Abort).ConvertAll(link => (Link)link);
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
            data.AddRange(Serialization.SerializeParameter(PointingTo));
            data.AddRange(Serialization.SerializeParameter((byte)Priority));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if (!base.DeserializeObjectSpecifics(data, ref index)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, out string pointingTo)) return false;
            PointingTo = pointingTo;
            if (!Serialization.DeserializeParameter(data, ref index, out byte bt)) return false;
            Priority = (NestedPriority)bt;
            return true;
        }
        #endregion
    }
}
