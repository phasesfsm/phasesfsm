using Phases.Variables;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Phases.Simulation;

namespace Phases.DrawableObjects
{
    class Relation : Link, IGlobal
    {
        public static readonly int radio = 4;
        public static readonly int selectionRadio = 8;
        public static readonly int large = 40;
        private readonly StringFormat leftTextFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        private readonly StringFormat rightTextFormat = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

        private string TriggerText => string.IsNullOrEmpty(Action) ? "" : Trigger;
        private Point TriggerTextLocation => new Point(location.X - large / 2 - radio - 3, location.Y);
        private string OutputText => string.IsNullOrEmpty(Action) ? "" : Output;
        private Point OutputTextLocation => new Point(location.X + large / 2 + 3, location.Y);
        public override string Text => string.IsNullOrEmpty(Action) ? Name : Action;

        public override int Radio { get { return radio; } }
        public override int SelectionRadio { get { return selectionRadio; } }

        public Relation(DrawableCollection ownerDraw, Point location)
            : base(ownerDraw, location)
        {
            this.location = location;
        }

        public Relation(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        [Category("General")]
        public int Priority
        {
            get
            {
                return OwnerDraw.OwnerSheet.OwnerBook.Globals.IndexOf(this);
            }
            set
            {
                if (value < 0) value = 0;
                else if (value >= OwnerDraw.OwnerSheet.OwnerBook.Globals.Count) value = OwnerDraw.OwnerSheet.OwnerBook.Globals.Count - 1;
                OwnerDraw.OwnerSheet.OwnerBook.Globals.Remove(this);
                OwnerDraw.OwnerSheet.OwnerBook.Globals.Insert(value, this);
            }
        }

        [Description("Input event to trigger an action over an input."), Category("Implementation")]
        [Browsable(true), TypeConverter(typeof(PropertiesCoverters.IndirectInputsList))]
        public string Trigger { get; set; } = "";

        private string output = "";
        [Description("Output variable to execute the action."), Category("Implementation")]
        [Browsable(true), TypeConverter(typeof(PropertiesCoverters.IndirectOutputsList))]
        public string Output
        {
            get
            {
                return output;
            }
            set
            {
                output = value;
                if(!OwnerDraw.OwnerSheet.OwnerBook.Variables.IndirectOutputs.Exists(obj => obj.Name == Output && obj.Operations.Contains(Action))) Action = null;
            }
        }

        [Description("Action over the output."), Category("Implementation")]
        [Browsable(true), TypeConverter(typeof(PropertiesCoverters.IndirectActionsList))]
        public string Action { get; set; } = "";

        internal string[] AvailableActions()
        {
            if (OwnerDraw.OwnerSheet.OwnerBook.Variables.IndirectOutputs.Exists(obj => obj.Name == Output))
            {
                return OwnerDraw.OwnerSheet.OwnerBook.Variables.IndirectOutputs.First(obj => obj.Name == Output).Operations;
            }
            else
            {
                return null;
            }
        }

        public override void DrawSimulationMark(Graphics g)
        {
            Brush brush;
            switch (SimulationMark)
            {
                case SimulationMark.TestingObject:
                case SimulationMark.ExecutingObject:
                    brush = Marks.ExecutingObjectBrush;
                    break;
                case SimulationMark.LeavingObject:
                    brush = Marks.LeavingObjectBrush;
                    break;
                default:
                    return;
            }
            g.FillRectangle(brush, Util.GetRectangle(Location, new Size(large + radio + 6, radio * 2 + 6)));
        }

        protected override void DrawText(Graphics g, Brush brush)
        {
            if (SimulationMark == SimulationMark.TestingObject)
            {
                g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(TriggerTextLocation, TriggerText, font, leftTextFormat));
            }
            else if (SimulationMark == SimulationMark.ExecutingObject)
            {
                g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(TextPoint, Text, font, TextFormat));
                g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(OutputTextLocation, OutputText, font, rightTextFormat));
            }
            base.DrawText(g, brush);
            g.DrawString(TriggerText, font, brush, TriggerTextLocation, leftTextFormat);
            g.DrawString(OutputText, font, brush, OutputTextLocation, rightTextFormat);
        }

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            g.FillEllipse(new SolidBrush(att.Pen.Color), location.X - radio - large / 2, location.Y - radio, radio * 2, radio * 2);
            g.DrawLine(att.Pen, location.X - large / 2f, location.Y, location.X + large / 2f + 0.4f, location.Y);
            g.DrawLine(att.Pen, location.X + large / 2 - radio, location.Y - radio, location.X + large / 2, location.Y);
            g.DrawLine(att.Pen, location.X + large / 2 - radio, location.Y + radio, location.X + large / 2, location.Y);
        }

        public override void DrawSelectionBack(Graphics g)
        {
            g.DrawRectangle(Pens.Gray, location.X - radio - large / 2 - 1, location.Y - radio - 1, large + radio + 2, radio * 2 + 2);
        }

        public override bool IsTextSelectable(Point position)
        {
            Size leftTextSize = TextRenderer.MeasureText(TriggerText, font);
            Size rightTextSize = TextRenderer.MeasureText(OutputText, font);
            Rectangle leftTextRect = new Rectangle(new Point(location.X - large / 2 - radio - 3 - leftTextSize.Width, location.Y - leftTextSize.Height / 2), leftTextSize);
            Rectangle rightTextRect = new Rectangle(new Point(location.X + large / 2 + 3, location.Y - leftTextSize.Height / 2), rightTextSize);
            return leftTextRect.Contains(position) || rightTextRect.Contains(position) || base.IsTextSelectable(position);
        }

        public override bool IsSelectablePoint(Point position)
        {
            return GetSelectionRectangle().Contains(position);
        }

        public override Rectangle GetSelectionRectangle()
        {
            return new Rectangle(location.X - radio - large / 2 - 1, location.Y - radio - 1, large + radio + 2, radio * 2 + 2);
        }

        public override List<MouseTool.SelectionRectangle> DrawSelection(Graphics g, bool focused)
        {
            Rectangle rect = GetSelectionRectangle();
            rect.Offset(-MouseTool.SelectionRectangle.size.Width-10, MouseTool.SelectionRectangle.size.Height / 2);
            int radiox = rect.Width / 2;
            int radioy = rect.Height / 2;
            List<MouseTool.SelectionRectangle> srs = new List<MouseTool.SelectionRectangle>();
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.None, new Point(location.X - radiox - MouseTool.SelectionRectangle.size.Width / 2, location.Y - radioy), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.None, new Point(location.X + radiox - MouseTool.SelectionRectangle.size.Width / 2, location.Y - radioy), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.None, new Point(location.X - radiox - MouseTool.SelectionRectangle.size.Width / 2, location.Y + radioy), focused));
            srs.Add(new MouseTool.SelectionRectangle(this, MouseTool.ResizingTypes.None, new Point(location.X + radiox - MouseTool.SelectionRectangle.size.Width / 2, location.Y + radioy), focused));
            return srs;
        }

        public override void MoveText(Point offset)
        {
            
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            Relation indir = obj as Relation;
            indir.Location = Location;
            indir.Trigger = Trigger;
            indir.Output = Output;
            indir.Action = Action;
        }

        public override object Clone()
        {
            var obj = (Relation)MemberwiseClone();
            CopyTo(obj);
            return obj;
        }

        #region "Serialization"

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>(base.SerializeSpecifics());
            data.AddRange(Serialization.SerializeParameter(Trigger));
            data.AddRange(Serialization.SerializeParameter(Output));
            data.AddRange(Serialization.SerializeParameter(Action));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            string str = "";
            if (!base.DeserializeObjectSpecifics(data, ref index)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref str)) return false;
            Trigger = str;
            if (!Serialization.DeserializeParameter(data, ref index, ref str)) return false;
            Output = str;
            if (!Serialization.DeserializeParameter(data, ref index, ref str)) return false;
            Action = str;
            return true;
        }

        #endregion
    }
}
