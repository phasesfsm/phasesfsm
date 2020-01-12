using Phases.BasicObjects;
using Phases.CodeGeneration;
using Phases.DrawableObjects;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Simulation
{
    class VirtualMachine : Machine, IDisposable
    {
        private List<Machine> Machine { get; set; }
        private VariablesStatusLog variablesStatus;

        private Machine current;
        private Machine CurrentMachine {
            get
            {
                return current;
            }
            set
            {
                current = value;
                if (current is MachineStatus ms) SubMachine = ms;
                else SubMachine = null;
            }
        }

        public VirtualMachine(GeneratorData data, VariablesStatusLog variablesStatusLog)
            : base(null, data)
        {
            Machine = new List<Machine>();
            variablesStatus = variablesStatusLog;

            foreach (IBasicGlobal ibg in Data.GlobalObjects)
            {
                switch (ibg)
                {
                    case BasicObjectsTree tree:
                        Machine.Add(new MachineStatus(Data, tree, tree));
                        Data.Store[Util.CounterName(tree.Name)] = 0;
                        foreach (BasicMachine mach in tree.SuperStatesList())
                        {
                            Data.Store[Util.CounterName(mach.Name)] = 0;
                        }
                        break;
                    case BasicRelation bindir:
                        Machine.Add(new MachineRelation(Data, bindir));
                        break;
                    case BasicEquation beq:
                        Machine.Add(new MachineEquation(Data, beq));
                        break;
                }
            }
        }

        public void Dispose()
        {
            foreach (DrawableObject obj in Data.ObjectsTable)
            {
                obj.SimulationMark = SimulationMark.None;
            }
            foreach (Transition obj in Data.TransitionsList)
            {
                obj.SimulationMark = SimulationMark.None;
            }
            foreach (BasicEquation obj in Data.EquationsList)
            {
                obj.SimulationMark = SimulationMark.None;
            }
            foreach (BasicRelation obj in Data.RelationsList)
            {
                obj.SimulationMark = SimulationMark.None;
            }
        }

        public override SimulationState MoveToNextStepState()
        {
            if (CurrentMachine == null)
            {
                CurrentMachine = Machine[0];
            }
            StepState = CurrentMachine.MoveToNextStepState();
            if (StepState == SimulationState.StateChanged)
            {
                Data.Store[Util.CounterName(SubMachine.Name)] = 0;
                SubMachine.AckStateChanged();
                StepState = SimulationState.EndOfCycle;
                Data.LogStateChange(SubMachine.PreviousState.Alias, SubMachine.CurrentState.Alias);
            }
            else if (StepState == SimulationState.ChangedToSubMachine)
            {
                Data.Store[Util.CounterName(SubMachine.Name)] = 0;
                Data.LogStateChange(SubMachine.PreviousState.Alias, SubMachine.CurrentState.Alias);
            }
            if (StepState == SimulationState.EndOfCycle)
            {
                //Change to next machine
                if (CurrentMachine != Machine.Last())
                {
                    while (CurrentMachine != Machine.Last())
                    {
                        //Move to the next machine
                        CurrentMachine = Machine.ElementAt(Machine.IndexOf(CurrentMachine) + 1);

                        if (CurrentMachine.MoveToNextStepState() != SimulationState.EndOfCycle) break;
                    }
                }
                else
                {
                    //Erase events
                    foreach(EventInput var in Data.Variables.EventInputs)
                    {
                        Data.Store[var.Name] = false;
                    }
                    //End condition step
                    CurrentMachine = null;
                    Data.IncrementMasterCounter();
                    variablesStatus.MaxTimeDraw = Data.MasterCounter;
                    return SimulationState.EndOfCycle;
                }
            }
            return CurrentMachine.StepState == SimulationState.EndOfCycle ? SimulationState.EndOfSubMachine : CurrentMachine.StepState;
        }

        public bool EvaluateCondition(BasicTransition transition)
        {
            return transition.Transition.Condition == "" ? true : false;
        }

        public string GetStatus()
        {
            StringBuilder sb = new StringBuilder();
            if (CurrentMachine == null)
            {
                sb.AppendFormat("[{0}]: End of Cycle.", Data.MasterCounter);
            }
            else if(CurrentMachine is MachineEquation)
            {
                sb.AppendFormat("[{0}]: Executing ecuation.", Data.MasterCounter);
            }
            else if (CurrentMachine is MachineRelation)
            {
                sb.AppendFormat("[{0}]: Testing Relation.", Data.MasterCounter);
            }
            else
            {
                sb.AppendFormat("[{0}] {1}: {2}", Data.MasterCounter, SubMachine.Name, SubMachine.StepState.ToString());
                if(SubMachine.StepState == SimulationState.ExecutingSubMachine)
                {
                    MachineStatus ms = SubMachine.SubMachine;
                    do
                    {
                        sb.AppendFormat(" > {0}: {1}", SubMachine.SubMachine.Name, SubMachine.SubMachine.StepState);
                        ms = ms.SubMachine;
                    } while (ms != null && ms.StepState == SimulationState.ExecutingSubMachine);
                }
            }
            return sb.ToString();
        }

        public string GetCurrentState(string defaultText = "")
        {
            MachineStatus machine = SubMachine;
            while(machine != null && machine.SubMachine != null)
            {
                machine = machine.SubMachine;
            }
            if (machine != null && machine.CurrentState != null) return machine.CurrentState.Name;
            else if (machine != null) return machine.Name;
            else return defaultText;
        }

        public string GetCurrentTransition(string defaultText = "")
        {
            if (SubMachine != null && SubMachine.CurrentTransition != null) return SubMachine.CurrentTransition.Name;
            else return defaultText;
        }
    }
}
