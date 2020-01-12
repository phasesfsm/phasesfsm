using Phases.BasicObjects;
using Phases.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Simulation
{
    class MachineEquation : Machine
    {
        BasicEquation Root;

        public MachineEquation(GeneratorData data, BasicEquation beq)
            : base(beq as IBasicGlobal, data)
        {
            Root = beq;
        }

        public override SimulationState MoveToNextStepState()
        {
            switch (StepState)
            {
                case SimulationState.EndOfCycle:
                    StepState = SimulationState.ExecutingEcuation;
                    Root.SimulationMark = SimulationMark.ExecutingObject;
                    break;
                case SimulationState.ExecutingEcuation:
                    Root.Evaluate(Data.Store);
                    StepState = SimulationState.EndOfCycle;
                    Root.SimulationMark = SimulationMark.None;
                    break;
            }
            return StepState;
        }
    }
}
