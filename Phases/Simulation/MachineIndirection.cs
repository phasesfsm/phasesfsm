using Phases.BasicObjects;
using Phases.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Simulation
{
    class MachineRelation : Machine
    {
        BasicRelation Root;

        public MachineRelation(GeneratorData data, BasicRelation bindir)
            : base(bindir as IBasicGlobal, data)
        {
            Root = bindir;
        }

        public override SimulationState MoveToNextStepState()
        {
            switch (StepState)
            {
                case SimulationState.EndOfCycle:
                    StepState = SimulationState.TestingRelation;
                    Root.SimulationMark = SimulationMark.TestingObject;
                    break;
                case SimulationState.TestingRelation:
                    if (Root.Evaluate(Data.Store))
                    {
                        StepState = SimulationState.ExecutingRelation;
                        Root.SimulationMark = SimulationMark.ExecutingObject;
                    }
                    else
                    {
                        StepState = SimulationState.EndOfCycle;
                        Root.SimulationMark = SimulationMark.None;
                    }
                    break;
                case SimulationState.ExecutingRelation:
                    Root.Execute(Data.Store);
                    StepState = SimulationState.EndOfCycle;
                    Root.SimulationMark = SimulationMark.LeavingObject;
                    break;
            }
            return StepState;
        }
    }
}
