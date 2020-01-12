using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    abstract class Transition : DrawableObject
    {
        static Pen guides = new Pen(Color.LightGray);
        
        private Point[] points;
        private Point textPointOffset;

#if !DEBUG
        [Browsable(false)]
#endif
        public double AngleS => StartAngle;
#if !DEBUG
        [Browsable(false)]
#endif
        public double AngleE => EndAngle;

        public double StartAngle, EndAngle;
        private double[] maxDist;

        public void SetTextPoint(Point point)
        {
            textPointOffset = Point.Subtract(point, new Size(Center));
        }

#if !DEBUG
        [Browsable(false)]
#endif
        public Point StartPoint
        {
            get
            {
                return points[0];
            }
        }

#if !DEBUG
        [Browsable(false)]
#endif
        public Point EndPoint
        {
            get
            {
                return points[3];
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

        [Category("General")]
        public int Priority
        {
            get
            {
                if(startObject == null)
                {
                    return 0;
                }
                else
                {
                    return startObject.outTransitions.IndexOf(this);
                }
            }
            set
            {
                if (startObject != null)
                {
                    if (value < 0) value = 0;
                    else if (value >= startObject.outTransitions.Count) value = startObject.outTransitions.Count - 1;
                    startObject.outTransitions.Remove(this);
                    startObject.outTransitions.Insert(value, this);
                }
            }
        }

        public Transition(DrawableCollection ownerDraw, Point[] splinePoints, DrawableObject startObject)
            : base(ownerDraw)
        {
            guides.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            points = (Point[])splinePoints.Clone();
            textPointOffset = Point.Empty;
            StartObject = startObject;
            maxDist = new double[2];
            maxDist[0] = points[0] == points[1] ? 50d : Math.Sqrt(Math.Pow(points[0].X - points[1].X, 2) + Math.Pow(points[0].Y - points[1].Y, 2));
            maxDist[1] = points[2] == points[3] ? 50d : Math.Sqrt(Math.Pow(points[2].X - points[3].X, 2) + Math.Pow(points[2].Y - points[3].Y, 2));
        }

        public Transition(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {
            points = new Point[4];
            maxDist = new double[2];
        }

        public static Transition Create(ObjectType objectType, DrawableCollection ownerDraw, Point[] splinePoints, DrawableObject startObject)
        {
            switch (objectType)
            {
                case ObjectType.SimpleTransition:
                    return new SimpleTransition(ownerDraw, splinePoints, startObject);
                case ObjectType.SuperTransition:
                    return new SuperTransition(ownerDraw, splinePoints, startObject);
                default:
                    throw new Exception("Invalid Transition type.");
            }
        }

#if !DEBUG
        [Browsable(false)]
#endif
        public override Point Center
        {
            get { return Util.Center(points); }
        }

        private DrawableObject startObject;
        [DisplayName("From"), Description("Output when the state is activated."), Category("Links")]
        public DrawableObject StartObject
        {
            get
            {
                return startObject;
            }
            set
            {
                ChangeStartObject(this, value);
                startObject = value;
            }
        }

        public override SuperState Father => null;
        public override bool HasFather(SuperState machine) => false;

        private DrawableObject endObject;
        [DisplayName("To"), Description("Output when the state is activated."), Category("Links")]
        public DrawableObject EndObject
        {
            get
            {
                return endObject;
            }
            set
            {
                ChangeEndObject(this, value);
                endObject = value;
            }
        }

        protected Point TextPoint
        {
            get
            {
                Point textPoint = Center;
                textPoint.Offset(textPointOffset);
                return textPoint;
            }
        }

        protected override void DrawText(Graphics g, Brush brush)
        {
            if (Text.Contains(Environment.NewLine))
            {
                int width = TextRenderer.MeasureText(Text, font).Width / 2;
                g.DrawLine(new Pen(brush), TextPoint.X - width, TextPoint.Y - 1, TextPoint.X + width, TextPoint.Y - 1);
            }
            g.DrawString(Text, font, brush, TextPoint, TextFormat);
        }

        public override void DrawSimulationMark(Graphics g)
        {
            Brush brush;
            switch (SimulationMark)
            {
                case SimulationMark.ExecutingObject:
                case SimulationMark.ExecutingObjectExitOutputs:
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
            Pen pen = new Pen(brush, 8f);
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(2, 2);
            pen.CustomEndCap = bigArrow;
            g.DrawBezier(pen, points[0], points[1], points[2], points[3]);
        }

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            Pen pen2 = new Pen(att.Pen.Color, att.Pen.Width);
            AdjustableArrowCap bigArrow;
            if (att.IsShadow || att.Scale < 0.9f)
            {
                bigArrow = new AdjustableArrowCap(2, 4);
            }
            else
            {
                bigArrow = new AdjustableArrowCap(4, 8);
            }
            pen2.CustomEndCap = bigArrow;
            g.DrawBezier(pen2, points[0], points[1], points[2], points[3]);
            //draw arrow
            //DrawArrow(g, pen);
        }

        //protected virtual void DrawArrow(Graphics g, Pen pen)
        //{
        //    Point arrow1 = points[3], arrow2 = points[3];
        //    double ang, dx, dy, arrowHeight = 14f, arrowAngle = 0.3d;
        //    bool drawArrow = false;
        //    dx = points[2].X - points[3].X;
        //    dy = points[2].Y - points[3].Y;
        //    if (Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)) >= 10d)
        //    {
        //        ang = (dx >= 0d ? Math.PI : 0) + Math.Atan(dy / dx);
        //        dx = arrowHeight * Math.Cos(ang - arrowAngle);
        //        dy = arrowHeight * Math.Sin(ang - arrowAngle);
        //        arrow1.Offset((int)-dx, (int)-dy);
        //        dx = arrowHeight * Math.Cos(ang + arrowAngle);
        //        dy = arrowHeight * Math.Sin(ang + arrowAngle);
        //        arrow2.Offset((int)-dx, (int)-dy);
        //        drawArrow = true;
        //    }
        //    else
        //    {
        //        dx = points[0].X - points[3].X;
        //        dy = points[0].Y - points[3].Y;
        //        if (Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)) >= 10d)
        //        {
        //            ang = (dx >= 0 ? Math.PI : 0) + Math.Atan(dy / dx);
        //            dx = arrowHeight * Math.Cos(ang - arrowAngle);
        //            dy = arrowHeight * Math.Sin(ang - arrowAngle);
        //            arrow1.Offset((int)-dx, (int)-dy);
        //            dx = arrowHeight * Math.Cos(ang + arrowAngle);
        //            dy = arrowHeight * Math.Sin(ang + arrowAngle);
        //            arrow2.Offset((int)-dx, (int)-dy);
        //            drawArrow = true;
        //        }
        //    }
        //    if (drawArrow)
        //    {
        //        PointF[] arrow = { points[3], arrow1, arrow2 };
        //        g.FillPolygon(new SolidBrush(pen.Color), arrow);
        //    }
        //}

        public override void DrawSelectionBack(Graphics g)
        {
            //draw curve guide lines
            g.DrawLine(guides, points[0], points[1]);
            g.DrawLine(guides, points[2], points[3]);
            //draw text rectangle
            g.DrawRectangle(guides, Util.GetRectangle(TextPoint, g.MeasureString(Text, font, Center, TextFormat).ToSize()));
        }

        public override List<MouseTool.SelectionRectangle> DrawSelection(Graphics g, bool focused)
        {
            List<MouseTool.SelectionRectangle> srs = new List<MouseTool.SelectionRectangle>();
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Spline0, points[0], focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Spline1, points[1], focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Spline2, points[2], focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Spline3, points[3], focused));
            return srs;
        }

        public void DrawInDir(Point newPoint, int movingIndex)
        {
            //Get the next point index
            int curvedIndex = movingIndex == 3 ? 2 : 1;
            //get the maxDist index
            int mdIndex = movingIndex == 3 ? 1 : 0;
            double maxDistance = maxDist[mdIndex];
            //distances between the current curved point and the newPoint
            double dx, dy;
            dx = newPoint.X - points[curvedIndex].X;
            dy = newPoint.Y - points[curvedIndex].Y;
            //distance between newPoint and point[index]
            double distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            //if distance > maxDistance then move points[idx] in newPoint direction until be at maxDistance from newPoint
            if ((int)distance > maxDistance)
            {
                points[curvedIndex] = newPoint;
                points[curvedIndex].Offset((int)(-dx * maxDistance / distance), (int)(-dy * maxDistance / distance));
            }
            points[movingIndex] = newPoint;
        }

        public override void DrawingRectangle(Point startPoint, Point endPoint)
        {
            maxDist[1] = Math.Sqrt(Math.Pow(startPoint.X - endPoint.X, 2) + Math.Pow(startPoint.Y - endPoint.Y, 2)) / 2;
            DrawInDir(endPoint, 3);
        }

        public override Rectangle GetSelectionRectangle()
        {
            int x1, y1, x2, y2;
            x1 = points[0].X;
            y1 = points[0].Y;
            x2 = x1;
            y2 = y1;
            for (int i = 1; i < 4; i++)
            {
                if (points[i].X < x1) x1 = points[i].X;
                else if (points[i].X > x2) x2 = points[i].X;
                if (points[i].Y < y1) y1 = points[i].Y;
                else if (points[i].Y > y2) y2 = points[i].Y;
            }
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        public void OutDir(Point position, int index)
        {
            points[index] = position;
        }

        public override void Move(System.Drawing.Point offset)
        {
            for (int i = 0; i < 4; i++)
            {
                points[i].Offset(offset);
            }
        }

        public void MoveStart(Point offset)
        {
            points[0].Offset(offset);
            points[1].Offset(offset);
        }

        public void MoveEnd(Point offset)
        {
            points[2].Offset(offset);
            points[3].Offset(offset);
        }

        public void MoveStartTo(Point dest)
        {
            Point offset = new Point(dest.X - points[0].X, dest.Y - points[0].Y);
            MoveStart(offset);
        }

        public void MoveEndTo(Point dest)
        {
            Point offset = new Point(dest.X - points[3].X, dest.Y - points[3].Y);
            MoveEnd(offset);
        }

        public override void MoveText(Point offset)
        {
            textPointOffset.Offset(offset);
        }

        public void SizeCheckAndFix()
        {
            if(startObject != null && startObject == endObject)
            {
                double diffAngle;
                switch (startObject)
                {
                    case SimpleState oState:
                        diffAngle = 20d / (oState.rect.Width + oState.rect.Height);
                        if (Math.Abs(StartAngle - EndAngle) < diffAngle)
                        {
                            diffAngle = StartAngle < EndAngle ? diffAngle : -diffAngle;
                            MoveEndTo(oState.PointFromAngle(EndAngle + diffAngle));
                            textPointOffset = Util.GetPoint(textPointOffset, 50, EndAngle + diffAngle / 2);
                        }
                        break;
                    case StateAlias oState:
                        diffAngle = 20d / (oState.rect.Width + oState.rect.Height);
                        if (Math.Abs(StartAngle - EndAngle) < diffAngle)
                        {
                            diffAngle = StartAngle < EndAngle ? diffAngle : -diffAngle;
                            MoveEndTo(oState.PointFromAngle(EndAngle + diffAngle));
                            textPointOffset = Util.GetPoint(textPointOffset, 50, EndAngle + diffAngle / 2);
                        }
                        break;
                }
            }
            else if(Util.Distance(points[0], points[3]) <= 10)
            {
                points[3].X += 100;
            }
        }

        public override void ResizeCheck(ref System.Drawing.Point offset, MouseTool.ResizingTypes dir)
        {
        }

        public override void Resize(System.Drawing.Point offset, MouseTool.ResizingTypes dir)
        {
            int idx = (int)Math.Log((double)dir, 2d) - 4;
            if (idx == 0 || idx == 3)
            {
                Point newPoint = points[idx];
                newPoint.Offset(offset);
                DrawInDir(newPoint, idx);
            }
            else
            {
                int idx2 = idx == 2 ? 3 : 0;
                points[idx].Offset(offset);
                maxDist[idx - 1] = Math.Sqrt(Math.Pow(points[idx].X - points[idx2].X, 2) + Math.Pow(points[idx].Y - points[idx2].Y, 2));
            }
        }

        public override bool IsTextSelectable(Point position)
        {
            Rectangle textRect = Util.GetRectangle(TextPoint, TextRenderer.MeasureText(Text, font));
            return textRect.Contains(position);
        }

        public override bool IsSelectablePoint(System.Drawing.Point position)
        {
            if (IsTextSelectable(position)) return true;
            Pen wpen = new Pen(Brushes.Black, 10);
            GraphicsPath gp = new GraphicsPath();
            gp.AddBezier(points[0], points[1], points[2], points[3]);
            return gp.IsOutlineVisible(position, wpen);
        }

        public override bool IsCovered(System.Drawing.Rectangle area)
        {
            if (area.Contains(points[0]) && area.Contains(points[3])) return true;
            return false;
        }

        public override bool IsSelectable(System.Drawing.Rectangle area)
        {
            if (area.Contains(points[0]) || area.Contains(points[3])) return true;
            return false;
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            Transition trans = (Transition)obj;
            trans.points = (Point[])points.Clone();
            trans.maxDist = (double[])maxDist;
            trans.startObject = startObject;
            trans.endObject = endObject;
            trans.EndAngle = EndAngle;
            trans.StartAngle = StartAngle;
            trans.textPointOffset = textPointOffset;
        }

        public override object Clone()
        {
            var obj = (Transition)MemberwiseClone();
            CopyTo(obj);
            return obj;
        }

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>();
            //Add parameters
            data.AddRange(SerializeObjectId());
            //Spline points
            data.AddRange(Serialization.SerializeParameter(points[0]));
            data.AddRange(Serialization.SerializeParameter(points[1]));
            data.AddRange(Serialization.SerializeParameter(points[2]));
            data.AddRange(Serialization.SerializeParameter(points[3]));
            //other
            data.AddRange(Serialization.SerializeParameter(textPointOffset));
            data.AddRange(Serialization.SerializeParameter(maxDist[0]));
            data.AddRange(Serialization.SerializeParameter(maxDist[1]));
            data.AddRange(Serialization.SerializeParameter(StartAngle));
            data.AddRange(Serialization.SerializeParameter(EndAngle));
            data.AddRange(Serialization.SerializeParameter(Priority));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if(!Serialization.DeserializeParameter(data, ref index, ref points[0])) return false;
            if(!Serialization.DeserializeParameter(data, ref index, ref points[1])) return false;
            if(!Serialization.DeserializeParameter(data, ref index, ref points[2])) return false;
            if(!Serialization.DeserializeParameter(data, ref index, ref points[3])) return false;
            if(!Serialization.DeserializeParameter(data, ref index, ref textPointOffset)) return false;
            if(!Serialization.DeserializeParameter(data, ref index, ref maxDist[0])) return false;
            if(!Serialization.DeserializeParameter(data, ref index, ref maxDist[1])) return false;
            if(!Serialization.DeserializeParameter(data, ref index, ref StartAngle)) return false;
            if(!Serialization.DeserializeParameter(data, ref index, ref EndAngle)) return false;
            int priority = 0;
            if (!Serialization.DeserializeParameter(data, ref index, ref priority)) return false;
            return true;
        }

        public virtual byte[] SerializeRelations()
        {
            var data = new List<byte>();
            //Add relations
            data.AddRange(SerializeObjectId());
            data.AddRange(SerializeRelation(OwnerDraw.Objects.IndexOf(startObject)));
            data.AddRange(SerializeRelation(OwnerDraw.Objects.IndexOf(endObject)));
            return data.ToArray();
        }

        public virtual bool DeserializeRelations(Dictionary<int, DrawableObject> dictionary, byte[] data, ref int index)
        {
            DrawableObject startRef, endRef;
            if (DeserializeRelation(dictionary, data, ref index, out startRef)) StartObject = startRef;
            if (DeserializeRelation(dictionary, data, ref index, out endRef)) EndObject = endRef;
            return true;
        }
    }
}
