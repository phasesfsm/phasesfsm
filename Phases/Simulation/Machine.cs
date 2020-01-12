using Phases.BasicObjects;
using Phases.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Simulation
{
    abstract class Machine
    {
        public SimulationState StepState { get; internal set; } = SimulationState.EndOfCycle;
        public MachineStatus SubMachine { get; internal set; } = null;
        public GeneratorData Data { get; private set; }
        public IBasicGlobal Global { get; private set; }
        public string Name => Global.Name;

        public Machine(IBasicGlobal global, GeneratorData data)
        {
            Data = data;
            Global = global;
        }

        public abstract SimulationState MoveToNextStepState();
    }
}
