using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class SimpleState : State
    {
        public SimpleState(DrawableCollection ownerDraw, Rectangle startRect)
            : base(ownerDraw, startRect)
        {
            rect = startRect;
        }

        public SimpleState(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        [Description("Forces the state to be drawn as a circle.")]
        public bool ForceCircle 
        {
            get => forceCircle;
            set
            {
                forceCircle = value;
                if (value)
                {
                    rect = Util.SquareRectangle(rect);
                    AdjustSize();
                }
            }
        }
        private bool forceCircle = false;

        protected override void DrawText(Graphics g, Brush brush)
        {
            if(SimulationMark == SimulationMark.ExecutingObjectExitOutputs)
            {
                if (EnterOutputsList.Count == 0)
                {
                    g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(Center, Text, font, TextFormat, 2));
                }
                else
                {
                    g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(Center, Text, font, TextFormat, 3));
                }
            }
            else if(SimulationMark == SimulationMark.ExecutingObjectEnterOutputs)
            {
                g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(Center, Text, font, TextFormat, 2));
            }
            base.DrawText(g, brush);
        }

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            if(!att.IsShadow) g.FillEllipse(Brushes.White, rect);
            g.DrawEllipse(att.Pen, rect);
        }

        public override void ResizeCheck(ref Point offset, MouseTool.ResizingTypes dir)
        {
            if (ForceCircle)
            {
                switch (dir)
                {
                    case MouseTool.ResizingTypes.Left_Top:
                        if (offset.X > offset.Y) offset.Y = offset.X;
                        else offset.X = offset.Y;
                        break;
                    case MouseTool.ResizingTypes.Right_Top:
                        if (offset.X < -offset.Y) offset.Y = -offset.X;
                        else offset.X = -offset.Y;
                        break;
                    case MouseTool.ResizingTypes.Left_Bottom:
                        if (offset.X > -offset.Y) offset.Y = -offset.X;
                        else offset.X = -offset.Y;
                        break;
                    case MouseTool.ResizingTypes.Right_Bottom:
                        if (offset.X < offset.Y) offset.Y = offset.X;
                        else offset.X = offset.Y;
                        break;
                }
            }
            base.ResizeCheck(ref offset, dir);
            if (ForceCircle)
            {
                switch (dir)
                {
                    case MouseTool.ResizingTypes.Left_Top:
                        if (offset.X < offset.Y) offset.Y = offset.X;
                        else offset.X = offset.Y;
                        break;
                    case MouseTool.ResizingTypes.Right_Top:
                        if (offset.X > -offset.Y) offset.Y = -offset.X;
                        else offset.X = -offset.Y;
                        break;
                    case MouseTool.ResizingTypes.Left_Bottom:
                        if (offset.X < -offset.Y) offset.Y = -offset.X;
                        else offset.X = -offset.Y;
                        break;
                    case MouseTool.ResizingTypes.Right_Bottom:
                        if (offset.X > offset.Y) offset.Y = offset.X;
                        else offset.X = offset.Y;
                        break;
                }
            }
        }

        public override List<MouseTool.SelectionRectangle> DrawSelection(Graphics g, bool focused)
        {
            Rectangle sel = rect;
            sel.Inflate(5, 5);
            List<MouseTool.SelectionRectangle> srs = new List<MouseTool.SelectionRectangle>();
            if (!ForceCircle)
            {
                srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Top, new Point(sel.X + sel.Width / 2, sel.Y), focused));
                srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Left, new Point(sel.X, sel.Y + sel.Height / 2), focused));
                srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Right, new Point(sel.Right, sel.Y + sel.Height / 2), focused));
                srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Bottom, new Point(sel.X + sel.Width / 2, sel.Bottom), focused));
            }
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Left_Top, sel.Location, focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Right_Top, new Point(sel.Right, sel.Y), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Left_Bottom, new Point(sel.X, sel.Bottom), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.Right_Bottom, new Point(sel.Right, sel.Bottom), focused));
            return srs;
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
            Rectangle r = rect;
            r.Inflate(5, 5);
            g.FillEllipse(brush, r);
        }

        public override string Name
        {
            set
            {
                base.Name = value;
                AdjustSize();
            }
        }

        public override string EnterOutput
        {
            set
            {
                base.EnterOutput = value;
                AdjustSize();
            }
        }

        public override string ExitOutput
        {
            set
            {
                base.ExitOutput = value;
                AdjustSize();
            }
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            var state = obj as SimpleState;
            state.ForceCircle = ForceCircle;
        }

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>(base.SerializeSpecifics());
            data.AddRange(Serialization.SerializeParameter(ForceCircle));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if (!base.DeserializeObjectSpecifics(data, ref index)) return false;
            if (Serialization.DeserializeParameter(data, ref index, out bool circle)) ForceCircle = circle;
            return true;
        }
    }
}
