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
            var state = (SimpleState)obj;
        }
    }
}
