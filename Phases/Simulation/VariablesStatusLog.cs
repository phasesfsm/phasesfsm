using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phases.Variables;

namespace Phases.Simulation
{
    class VariablesStatusLog
    {
        private readonly Font variablesNamesFont = new Font("Arial", 8f);
        private readonly Brush variablesNamesBrush = Brushes.Black;
        private readonly StringFormat variablesNameStringFormat = new StringFormat() { LineAlignment = StringAlignment.Center };

        public const float VerticalSeparation = 4f;
        public List<VariableHistory> Histories { get; }
        public int MaxTimeDraw { get; set; } = 0;

        public VariablesStatusLog()
        {
            Histories = new List<VariableHistory>();
        }

        public int GetTimeMax()
        {
            int max = 0;

            // Get the maximum time
            foreach (VariableHistory variable in Histories)
            {
                if (variable.History.Last() > max) max = variable.History.Last();
            }

            return max;
        }

        public void Draw(Graphics g, Pen linePen)
        {
            float posY = VariableHistory.DrawHeight / 2f + VerticalSeparation;

            // Get the maximum time and draw variables names
            foreach (VariableHistory variable in Histories)
            {
                g.DrawString(variable.Variable, variablesNamesFont, variablesNamesBrush, 0, posY, variablesNameStringFormat);
                posY += VariableHistory.DrawHeight + VerticalSeparation;
            }
            // Draw time lines
            posY = VerticalSeparation;
            foreach (VariableHistory variable in Histories)
            {
                variable.LinePen = linePen;
                variable.Draw(g, posY, Math.Max(MaxTimeDraw, GetTimeMax()));
                posY += VariableHistory.DrawHeight + VerticalSeparation;
            }

            // Draw separation with flags
            
        }

        public void ChangeVariable(String variable, int time)
        {
            VariableHistory variableHistory = Histories.Find(vh => vh.Variable == variable);

            if (variableHistory != null)
            {
                variableHistory.AddChange(time);
            }
        }

        public bool HadHistoryChanged(string variable, int time)
        {
            VariableHistory history = Histories.Find(vh => vh.Variable == variable);
            if (history == null) return false;
            return history.History.Contains(time);
        }
    }
}
