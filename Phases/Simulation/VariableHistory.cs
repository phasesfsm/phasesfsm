using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phases.Variables;

namespace Phases.Simulation
{
    class VariableHistory
    {
        public const float DrawHeight = 15f;
        public const float LineWidth = 0.2f;
        public const float ProcessWidth = 5f;
        public const float VariableNameWidth = 100f;

        public string Variable { get; }
        public List<int> History { get; }
        public bool InitialValue { get; }
        public Pen LinePen { get; set; }
        public bool IsEvent { get; set; }

        public VariableHistory(string eventVariable)
        {
            Variable = eventVariable;
            InitialValue = false;
            IsEvent = true;
            History = new List<int>
            {
                { 0 }
            };
            ChangeLineColor(Color.Black);
        }

        public VariableHistory(string boolVariable, bool initialValue)
        {
            Variable = boolVariable;
            InitialValue = initialValue;
            IsEvent = false;
            History = new List<int>
            {
                { 0 }
            };
            ChangeLineColor(Color.Black);
        }

        public void ChangeLineColor(Color color)
        {
            LinePen = new Pen(color, LineWidth);
        }

        public void AddChange(int time)
        {
            if (History.Last() >= time)
            {
                throw new Exception("Value must be major than the latest time value in history.");
            }
            History.Add(time);
        }

        public void AddIncrement(int time)
        {
            History.Add(History.Last() + time);
        }

        public void Draw(Graphics g, float posY, int max)
        {
            PointF startPoint, endPoint;
            bool value = InitialValue;
            float posX, maxX = VariableNameWidth + max * ProcessWidth;
            startPoint = new PointF(VariableNameWidth, posY + (value ? 0f : DrawHeight));

            foreach (int time in History)
            {
                if (time != 0)
                {
                    posX = VariableNameWidth + time * ProcessWidth;
                    endPoint = new PointF(posX, posY + (value ? 0f : DrawHeight));
                    g.DrawLine(LinePen, startPoint, endPoint);
                    if (!IsEvent)
                    {
                        value = !value;
                        startPoint = new PointF(posX, posY + (value ? 0f : DrawHeight));
                        g.DrawLine(LinePen, startPoint, endPoint);
                    }
                    else
                    {
                        startPoint = new PointF(posX, posY);
                        g.DrawLine(LinePen, startPoint, endPoint);
                        endPoint = new PointF(posX + ProcessWidth, posY);
                        g.DrawLine(LinePen, startPoint, endPoint);
                        startPoint = new PointF(posX + ProcessWidth, posY + DrawHeight);
                        g.DrawLine(LinePen, startPoint, endPoint);
                    }
                }
            }
            endPoint = new PointF(maxX, posY + (value ? 0f : DrawHeight));
            g.DrawLine(LinePen, startPoint, endPoint);
        }
    }
}
