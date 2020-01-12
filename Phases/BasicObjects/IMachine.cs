using Phases.DrawableObjects;
using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    interface IMachine : IBasicGlobal
    {
        List<BasicState> States { get; }
        Origin Origin { get; }
        BasicTransition Transition { get; }
        BasicRoot Root { get; }
        //string Name { get; }
        List<BasicTransition> Transitions { get; }
        SimulationMark SimulationMark { get; set; }
        IMachine Father { get; }

        bool HasFirstPriority();

        bool HasLastPriority();

        bool HasEntryOutputs();

        bool HasExitOutputs();

    }
}
