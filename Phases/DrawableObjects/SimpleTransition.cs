using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class SimpleTransition : Transition
    {
        private List<string> output = new List<string>(); //Outputs if the transition happen
        private int timeout = 0;

        public SimpleTransition(DrawableCollection ownerDraw, Point[] splinePoints, DrawableObject startObject)
            : base(ownerDraw, splinePoints, startObject)
        {

        }

        public SimpleTransition(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        public override string Text
        {
            get
            {
                string up;
                if(Condition == "" && timeout == 0)
                {
                    up = StartObject is Origin && Output == "" ? "" : name;
                }
                else if (Condition == "")
                {
                    up = "t=" + timeout.ToString();
                }
                else if(timeout == 0)
                {
                    up = Condition;
                }
                else
                {
                    string cond = trigger == TransitionTriggerType.ConditionAndTimeout ? "&" : "|";
                    up = string.Format("t≥{0} {1} ({2})", timeout.ToString(), cond, Condition);
                }
                return up + (Output != "" ? Environment.NewLine + Output : "");
            }
        }

        protected override void DrawText(Graphics g, Brush brush)
        {
            Size size = g.MeasureString(Text, font).ToSize();
            switch (SimulationMark)
            {
                case SimulationMark.ExecutingObjectExitOutputs:
                    g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(TextPoint, Text, font, TextFormat, 2));
                    break;
                case SimulationMark.TestingObject:
                    if (Text.Contains(Environment.NewLine))
                    {
                        g.FillRectangle(Marks.TestingObjectBrush, Util.GetTextRectangle(TextPoint, Text, font, TextFormat, 1));
                    }
                    else
                    {
                        g.FillRectangle(Marks.TestingObjectBrush, Util.GetRectangle(TextPoint, size));
                    }
                    break;
            }
            base.DrawText(g, brush);
        }

        [Description("Condition for the transition."), Category("Logics")]
        [Editor(typeof(Phases.PropertiesCoverters.ConditionalEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(PropertiesCoverters.NullCoverter))]
        public virtual string Condition { get; set; } = "";

        [Description("Output when the state is activated."), Category("Logics")]
        [Editor(typeof(Phases.PropertiesCoverters.OutputsEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(PropertiesCoverters.NullCoverter))]
        public string Output
        {
            get
            {
                return string.Join(", ", output.Select(item => item));
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    output = new List<string>();
                }
                else
                {
                    output = new List<string>(value.Split(','));
                }
            }
        }

        [Browsable(false)]
        public List<string> OutputsList => output;

        [Description("Timeout in machine ticks."), Category("Logics")]
        public virtual int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = Math.Max(value, 0);
            }
        }

        public enum TransitionTriggerType
        {
            ConditionAndTimeout,
            ConditionOrTimeout
        }

        private TransitionTriggerType trigger = TransitionTriggerType.ConditionAndTimeout;
        [Description("Timeout behavior when the count expires."), Category("Logics")]
        public TransitionTriggerType TransitionTrigger
        {
            get
            {
                return trigger;
            }
            set
            {
                trigger = value;
            }
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            var trans = (SimpleTransition)obj;
            trans.Condition = Condition;
            trans.output = output;
        }

        public override object Clone()
        {
            var obj = (Transition)MemberwiseClone();
            CopyTo(obj);
            return obj;
        }

        public override byte[] SerializeSpecifics()
        {
            var data = new List<byte>(base.SerializeSpecifics());
            data.AddRange(Serialization.SerializeParameter(Condition));
            data.AddRange(Serialization.SerializeParameter(output.ToArray()));
            data.AddRange(Serialization.SerializeParameter(timeout));
            data.AddRange(Serialization.SerializeParameter((byte)trigger));
            return data.ToArray();
        }

        public override bool DeserializeObjectSpecifics(byte[] data, ref int index)
        {
            if (!base.DeserializeObjectSpecifics(data, ref index)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, out string condition)) return false;
            Condition = condition;
            if (!Serialization.DeserializeParameter(data, ref index, out string[] outputs)) return false;
            output = new List<string>(outputs);
            if (!Serialization.DeserializeParameter(data, ref index, out timeout)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, out byte bt)) return false;
            trigger = (TransitionTriggerType)bt;
            return true;
        }
    }
}
