using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phases.Simulation
{
    class SignalsDraw
    {
        PictureBox Canvas { get; }
        public VariablesStatusLog VariablesStatus { get; set; }
        public VariablesStatusLog VariablesShadow { get; set; }

        public SignalsDraw(PictureBox canvas)
        {
            Canvas = canvas;
        }

        public void Paint(Graphics g)
        {
            if (VariablesShadow != null)
            {
                VariablesShadow.Draw(g, Pens.LightSalmon);
            }
            if (VariablesStatus != null)
            {
                VariablesStatus.Draw(g, Pens.Black);
            }
        }
    }
}
