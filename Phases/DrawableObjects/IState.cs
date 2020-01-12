using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.DrawableObjects
{
    interface IState
    {
        DrawableCollection OwnerDraw { get; }
        string Name { get; }
        string Description { get; }
        SimulationMark SimulationMark { get; set; }
        List<string> EnterOutputsList { get; }
        List<string> ExitOutputsList { get; }

        Transition[] InTransitions { get; }
        Transition[] OutTransitions { get; }

        bool HasFather(SuperState father);
    }
}
