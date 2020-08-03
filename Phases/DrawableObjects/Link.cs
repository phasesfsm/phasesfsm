using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Phases.Simulation;

namespace Phases.DrawableObjects
{
    abstract class Link : DrawableObject
    {
        public static readonly int transitionVector = 50;
        [Browsable(false)]
        public abstract int Radio { get; }
        [Browsable(false)]
        public abstract int SelectionRadio { get; }
        public Point location;
        private Point textPointOffset;

        public Link(DrawableCollection ownerDraw, Point _location)
            : base(ownerDraw)
        {
            location = _location;
        }

        public Link(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        public static Link Create(ObjectType objectType, DrawableCollection ownerDraw, Point _location)
        {
            switch (objectType)
            {
                case ObjectType.Abort:
                    return new Abort(ownerDraw, _location);
                case ObjectType.Alias:
                    return new Alias(ownerDraw, _location);
                case ObjectType.End:
                    return new End(ownerDraw, _location);
                case ObjectType.Origin:
                    return new Origin(ownerDraw, _location);
                case ObjectType.Relation:
                    return new Relation(ownerDraw, _location);
                default:
                    throw new Exception("Invalid State type.");
            }
        }

        public override SuperState Father
        {
            get
            {
                SuperState father = null;
                foreach (SuperState superState in OwnerDraw.SuperStates)
                {
                    if (superState.Contains(this) && !superState.Contains(father)) father = superState;
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

#if !DEBUG
        [Browsable(false)]
#endif
        public override Point Center
        {
            get { return location; }
        }

        public Point Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        protected Point TextPoint
        {
            get
            {
                Point textPoint = Center;
                textPoint.Offset(textPointOffset);
                textPoint.Offset(0, -SelectionRadio*2 - 5);
                return textPoint;
            }
        }

        protected override void DrawText(Graphics g, Brush brush)
        {
            g.DrawString(Text, font, brush, TextPoint, TextFormat);
        }

        public override void DrawSelectionBack(Graphics g)
        {
            g.DrawEllipse(Pens.Gray, location.X - SelectionRadio, location.Y - SelectionRadio, SelectionRadio * 2, SelectionRadio * 2);
        }

        public override void DrawSimulationMark(Graphics g)
        {
            Brush brush;
            switch (SimulationMark)
            {
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
            g.FillRectangle(brush, Util.GetRectangle(TextPoint, Size.Round(g.MeasureString(Text, font))));
            g.FillEllipse(brush, Util.GetRectangle(Location, new Size(SelectionRadio + 12, SelectionRadio + 12)));
        }

        public override List<MouseTool.SelectionRectangle> DrawSelection(Graphics g, bool focused)
        {
            int radio = SelectionRadio;
            List<MouseTool.SelectionRectangle> srs = new List<MouseTool.SelectionRectangle>();
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.None, new Point(location.X - radio, location.Y - radio), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.None, new Point(location.X + radio, location.Y - radio), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.None, new Point(location.X - radio, location.Y + radio), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.None, new Point(location.X + radio, location.Y + radio), focused));
            return srs;
        }

        public override void DrawShadow(Graphics g, DrawAttributes att)
        {
            DrawForm(g, att);
        }

        public override void DrawingRectangle(Point startPoint, Point endPoint)
        {
            location = endPoint;
        }

        public override void Move(Point offset)
        {
            location.Offset(offset);
        }

        public override void MoveText(Point offset)
        {
            textPointOffset.Offset(offset);
        }

        public override void Intersect(Point position, ref Point point, ref double angle)
        {
            Point res;
            float x, y;
            double px, py;
            x = position.X - location.X;
            y = position.Y - location.Y;
            if (x == 0 && y == 0) return;
            angle = (x >= 0f ? 0 : Math.PI) + Math.Atan(y / x);
            px = Radio * Math.Cos(angle);
            py = Radio * Math.Sin(angle);
            res = Point.Round(new PointF((float)px, (float)py));
            res.Offset(location);
            point = res;
        }

        public virtual Point PointFromAngle(double angle)
        {
            Point res;
            double px, py;
            px = Radio * Math.Cos(angle);
            py = Radio * Math.Sin(angle);
            res = Point.Round(new PointF((float)px, (float)py));
            res.Offset(location);
            return res;
        }

        public override Point OutDir(Point position, out double angle)
        {
            angle = Util.GetAngle(location, position);
            return Util.GetPoint(location, Util.Distance(location, position) + transitionVector, Util.GetAngle(location, position));
        }

        public override bool IsTextSelectable(Point position)
        {
            Rectangle textRect = Util.GetRectangle(TextPoint, TextRenderer.MeasureText(Text, font));
            return textRect.Contains(position);
        }

        public override bool IsSelectablePoint(Point position)
        {
            if (IsTextSelectable(position)) return true;
            return Math.Sqrt(Math.Pow(location.X - position.X, 2) + Math.Pow(location.Y - position.Y, 2)) <= Radio + 2;
        }

        public override bool IsCovered(Rectangle area)
        {
            return area.Contains(GetSelectionRectangle());
        }

        public override bool IsSelectable(Rectangle area)
        {
            return area.IntersectsWith(GetSelectionRectangle());
        }

        public override Rectangle GetSelectionRectangle()
        {
            return new Rectangle(location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            var link = (Link)obj;
            link.location = location;
            link.textPointOffset = textPointOffset;
            if (inTransitions != null) link.inTransitions = new List<Transition>(inTransitions);
        }

        public override object Clone()
        {
            var obj = (Link)MemberwiseClone();
            CopyTo(obj);
            return obj;
        }

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>();
            //Add parameters
            data.AddRange(SerializeObjectId());
            //location and text
            data.AddRange(Serialization.SerializeParameter(location));
            data.AddRange(Serialization.SerializeParameter(textPointOffset));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if (!Serialization.DeserializeParameter(data, ref index, out location)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, out textPointOffset)) return false;
            return true;
        }

        public static string[] LinksObjects(SuperTransition @object)
        {
            if(@object.StartObject is SuperState)
            {
                return ((SuperState)@object.StartObject).ContainedTransitionLinksNames();
            }
            else if (@object.StartObject is Nested)
            {
                return ((Nested)@object.StartObject).ContainedTransitionLinksNames();
            }
            else
            {
                throw new Exception("Invalid object type.");
            }
        }
    }
}
