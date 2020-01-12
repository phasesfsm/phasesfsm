using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Simulation
{
    public enum SimulationMark
    {
        None,
        LeavingObject,
        TestingObject,
        ExecutingObject,
        ExecutingObjectEnterOutputs,
        ExecutingObjectExitOutputs
    }

    sealed class Marks
    {
        public static Brush LeavingObjectBrush = Brushes.LightGray;
        public static Brush TestingObjectBrush = Brushes.Aqua;
        public static Brush ExecutingObjectBrush = Brushes.Yellow;

    }
}
