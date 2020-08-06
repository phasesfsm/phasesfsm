using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class StateAlias : State
    {
        public StateAlias(DrawableCollection ownerDraw, Rectangle startRect)
            : base(ownerDraw, startRect)
        {
            rect = startRect;
        }

        public StateAlias(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        public override string Text
        {
            get
            {
                if (pointing == null) return name;
                else return pointing.Text;
            }
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
            g.FillEllipse(Brushes.White, rect);
            g.DrawEllipse(att.Pen, rect);
            Rectangle irect = rect;
            irect.Inflate(-3, -3);
            g.DrawEllipse(att.Pen, irect);
        }

        //public override void DrawSimulationMark(Graphics g)
        //{
        //    Brush brush;
        //    switch (SimulationMark)
        //    {
        //        case SimulationMark.ExecutingObjectEnterOutputs:
        //            brush = Marks.ExecutingObjectBrush;
        //            break;
        //        case SimulationMark.ExecutingObjectExitOutputs:
        //            brush = Marks.ExecutingObjectBrush;
        //            break;
        //        case SimulationMark.ExecutingObject:
        //            brush = Marks.ExecutingObjectBrush;
        //            break;
        //        case SimulationMark.TestingObject:
        //            brush = Marks.TestingObjectBrush;
        //            break;
        //        case SimulationMark.LeavingObject:
        //            brush = Marks.LeavingObjectBrush;
        //            break;
        //        default:
        //            return;
        //    }
        //    Rectangle r = rect;
        //    r.Inflate(5, 5);
        //    g.FillEllipse(brush, r);
        //}

        public override string Name
        {
            set
            {
                base.Name = value;
                AdjustSize();
            }
        }

        [Browsable(false)]
        public override string EnterOutput => null;

        [Browsable(false)]
        public override string ExitOutput => null;

        [Browsable(false)]
        public override List<Alias> Aliases => null;

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            var state = (StateAlias)obj;
        }

        private DrawableObject pointing;
        [Description("The object being pointed by this alias."), Category("General"), Browsable(true), TypeConverter(typeof(Phases.PropertiesCoverters.ObjectsListConverter))]
        public string PointingTo
        {
            get
            {
                if (pointing == null) return null;
                return pointing.Name;
            }
            set
            {
                pointing = OwnerDraw.Objects.Find(obj => obj.Name == value);
                AdjustSize();
            }
        }

        [Browsable(false)]
        public State Pointing => pointing as State;

        internal override List<Transition> outTransitions => pointing == null ? base.outTransitions : pointing.outTransitions;
        internal List<Transition> AliasOutTransitions => base.outTransitions;
#if DEBUG
        public Transition[] aliasOutTransitions => base.outTransitions.ToArray();
#endif
        protected override void AdjustSize()
        {
            Point sizeRef = Point.Empty;
            ResizeCheck(ref sizeRef, MouseTool.ResizingTypes.Right_Bottom);
            Resize(sizeRef, MouseTool.ResizingTypes.Right_Bottom);
            foreach (Transition oTransition in InTransitions)
            {
                oTransition.MoveEndTo(PointFromAngle(oTransition.EndAngle));
            }
            foreach (Transition oTransition in AliasOutTransitions)
            {
                oTransition.MoveStartTo(PointFromAngle(oTransition.StartAngle));
            }
        }

        public virtual byte[] SerializeRelations()
        {
            var data = new List<byte>();
            //Add relations
            data.AddRange(SerializeObjectId());
            data.AddRange(SerializeRelation(OwnerDraw.Objects.IndexOf(pointing)));
            return data.ToArray();
        }

        public virtual bool DeserializeRelations(Dictionary<int, DrawableObject> dictionary, byte[] data, ref int index)
        {
            DrawableObject stateRef;
            if (DeserializeRelation(dictionary, data, ref index, out stateRef)) pointing = stateRef;
            return true;
        }
    }
}
