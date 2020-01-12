using Phases.BasicObjects;
using Phases.CodeGeneration;
using Phases.DrawableObjects;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Simulation
{
    public enum SimulationState
    {
        EndOfCycle = 0x00,
        EndOfSubMachine,
        StateChanged,
        ChangedToSubMachine,

        Running = 0x70,
        ExecutingExitOutputs,
        ExecutingTransitionOutputs,
        ExecutingEnterOutputs,
        ExecutingSubMachine,
        ExecutingEcuation,
        ExecutingRelation,

        TestingTransition = 0x30,
        TestingRelation = 0x31,

        SwitchingStates = 0x50,
        SwitchingStatesUp,
        SwitchingTransitionOutputs,
        SwitchingStatesDown,
    }

    class MachineStatus : Machine
    {
        public virtual BasicObject PreviousState { get; set; } = null;
        public virtual BasicTransition CurrentTransition { get; set; } = null;
        public virtual BasicTransition PreviousTransition { get; set; } = null;
        public BasicObjectsTree Tree { get; private set; }
        public BasicState TargetState { get; private set; } = null;
        public virtual MachineStatus Owner { get; protected set; }
        public IMachine Node { get; private set; }

        public MachineStatus(GeneratorData data, IMachine mach, BasicObjectsTree tree, bool markStart = true)
            : base(mach as IBasicGlobal, data)
        {
            Owner = null;
            Node = mach;
            Tree = tree;
            if(markStart) mach.Origin.SimulationMark = SimulationMark.ExecutingObject;
            CurrentState = mach.Root;
            if (mach.Father is BasicMachine bmach)
            {
                foreach(BasicState bstate in bmach.States)
                {
                    if (bstate != mach)
                    {
                        bstate.SimulationMark = SimulationMark.None;
                    }
                }
            }
        }

        private BasicObject currentState;
        public BasicObject CurrentState
        {
            get
            {
                return currentState;
            }
            private set
            {
                if (currentState != null && !(currentState is BasicRoot)) Data.Store[currentState.Alias] = false;
                currentState = value;
                if (currentState != null && !(currentState is BasicRoot)) Data.Store[currentState.Alias] = true;
            }
        }

        public override SimulationState MoveToNextStepState()
        {
            switch (StepState)
            {
                case SimulationState.EndOfCycle:
                    //Clean messages
                    if(Node is BasicObjectsTree tree)
                    {
                        foreach(Variable var in Tree.OutputVariables)
                        {
                            if (var is MessageFlag msg) Data.Store[msg.Name] = false;
                        }
                    }

                    //Increment machine counter
                    Data.Store[Util.CounterName(Name)] = Data.Store[Util.CounterName(Name)].AsNumber + 1;

                    //Execute
                    switch (CurrentState)
                    {
                        case BasicRoot broot:
                            StepToTransition(broot.Transition);
                            break;
                        case IMachine imach:
                            if (SubMachine == null)
                            {
                                StepToTransition(imach.Transition);
                            }
                            else
                            {
                                EnterToSubMachine(CurrentState as BasicMachine);
                            }
                            break;
                        case BasicState state:
                            if (state.Transitions.Count > 0)
                            {
                                StepToTransition(state.Transitions.First());
                            }
                            break;
                    }
                    break;
                case SimulationState.ChangedToSubMachine:
                case SimulationState.ExecutingSubMachine:
                    ExecuteSubMachine(CurrentState as BasicMachine);
                    break;
                case SimulationState.TestingTransition:
                    if (TestTransition(CurrentTransition))
                    {
                        TargetState = CurrentTransition.Pointing;
                        ExitFromState();
                    }
                    else
                    {
                        if (CurrentState is BasicRoot root)
                        {
                            StepToTransition(null);
                        }
                        else if(CurrentState is BasicMachine bmach && CurrentTransition == bmach.Transitions.Last() && bmach.HasFirstPriority())
                        {
                            StepToTransition(null);
                            ExecuteSubMachine(bmach);
                        }
                        else if (CurrentState is BasicState bstate && bstate.Transitions.Contains(CurrentTransition) && CurrentTransition != bstate.Transitions.Last())
                        {
                            StepToTransition(bstate.Transitions[bstate.Transitions.IndexOf(CurrentTransition) + 1]);
                        }
                        else
                        {
                            StepToTransition(null);
                        }
                    }
                    break;
                case SimulationState.ExecutingExitOutputs:
                    foreach (BasicOutput output in (CurrentState as BasicState).ExitOutputs)
                    {
                        Data.Store[output.Name] = output.Output.Evaluate(output.Operation, Data.Store[output.Name]);
                    }
                    MakeTransition();
                    break;
                case SimulationState.ExecutingTransitionOutputs:
                    foreach (BasicOutput output in CurrentTransition.Outputs)
                    {
                        Data.Store[output.Name] = output.Output.Evaluate(output.Operation, Data.Store[output.Name]);
                    }
                    EnterToState();
                    break;
                case SimulationState.ExecutingEnterOutputs:
                    foreach (BasicOutput output in (CurrentState as BasicState).EnterOutputs)
                    {
                        Data.Store[output.Name] = output.Output.Evaluate(output.Operation, Data.Store[output.Name]);
                    }
                    CurrentState.SimulationMark = SimulationMark.ExecutingObject;
                    FlagStateChanged();
                    break;
                case SimulationState.SwitchingStatesUp:
                    foreach (BasicOutput output in (CurrentState as BasicState).ExitOutputs)
                    {
                        Data.Store[output.Name] = output.Output.Evaluate(output.Operation, Data.Store[output.Name]);
                    }
                    PreviousState.SimulationMark = SimulationMark.None;
                    CurrentState.SimulationMark = SimulationMark.LeavingObject;
                    SwitchStateUp();
                    break;
                case SimulationState.SwitchingStatesDown:
                    foreach (BasicOutput output in (CurrentState as BasicState).EnterOutputs)
                    {
                        Data.Store[output.Name] = output.Output.Evaluate(output.Operation, Data.Store[output.Name]);
                    }
                    CurrentState.SimulationMark = SimulationMark.ExecutingObject;
                    Data.Store[CurrentState.Name] = 0;
                    SubMachine = new SubMachineStatus(this, TargetState.NextFather(Node), Tree, false);
                    StepState = SimulationState.ExecutingSubMachine;
                    SwitchStateDown(SubMachine);
                    break;
                case SimulationState.SwitchingTransitionOutputs:
                    foreach (BasicOutput output in CurrentTransition.Outputs)
                    {
                        Data.Store[output.Name] = output.Output.Evaluate(output.Operation, Data.Store[output.Name]);
                    }
                    CurrentState.SimulationMark = SimulationMark.None;
                    CurrentTransition.SimulationMark = SimulationMark.ExecutingObject;
                    SwitchStateDown(this);
                    break;
            }
            return StepState;
        }

        public void FlagStateChanged()
        {
            if (CurrentState is BasicMachine bmach)
            {
                StepState = SimulationState.ChangedToSubMachine;
            }
            else
            {
                StepState = SimulationState.StateChanged;
            }
            TargetState = null;
            CurrentTransition = null;
        }

        public void AckStateChanged()
        {
            StepState = SimulationState.EndOfCycle;
        }

        private void EnterToSubMachine(BasicMachine mach)
        {
            if (mach.HasFirstPriority() && mach.Transitions.Count > 0)
            {
                StepToTransition(mach.Transitions.First());
            }
            else
            {
                StepState = SimulationState.ExecutingSubMachine;
                ExecuteSubMachine(mach);
            }
        }

        private void ExecuteSubMachine(BasicMachine mach)
        {
            SimulationState state = SubMachine.MoveToNextStepState();
            if (state == SimulationState.StateChanged)
            {
                Data.Store[Util.CounterName(mach.Name)] = 0;
                SubMachine.AckStateChanged();
                StepState = SimulationState.EndOfCycle;
                Data.LogStateChange(SubMachine.PreviousState.Alias, SubMachine.CurrentState.Alias);
            }
            else if (state == SimulationState.ChangedToSubMachine)
            {
                Data.Store[Util.CounterName(mach.Name)] = 0;
                Data.LogStateChange(SubMachine.PreviousState.Alias, SubMachine.CurrentState.Alias);
            }
            else if (state == SimulationState.EndOfCycle)
            {
                LeaveSubMachine(mach);
            }
            else if(!StepState.HasFlag(SimulationState.SwitchingStates))
            {
                StepState = SimulationState.ExecutingSubMachine;
            }
        }

        private void LeaveSubMachine(BasicMachine mach)
        {
            if (mach.HasLastPriority())
            {
                StepToTransition(mach.Transitions.First());
            }
            else
            {
                StepState = SimulationState.EndOfCycle;
            }
        }

        private void StepToTransition(BasicTransition nextTransition)
        {
            if (CurrentTransition != null)
            {
                CurrentTransition.SimulationMark = SimulationMark.None;
            }
            CurrentTransition = nextTransition;
            nextTransition = null;
            if (CurrentTransition != null)
            {
                CurrentTransition.SimulationMark = SimulationMark.TestingObject;
                StepState = SimulationState.TestingTransition;
            }
            else
            {
                StepState = SimulationState.EndOfCycle;
            }
        }

        private void ExitFromState()
        {
            if (CurrentState is BasicMachine bmach && SubMachine != null)
            {
                MachineStatus machineStatus = SubMachine;
                do
                {
                    machineStatus.CurrentState.SimulationMark = SimulationMark.None;
                    machineStatus.CurrentState = null;
                    machineStatus = machineStatus.SubMachine;
                } while (machineStatus != null);
                if (PreviousTransition != null) PreviousTransition.SimulationMark = SimulationMark.None;
            }
            if (CurrentState != Node && CurrentState is BasicState bstate && bstate.HasExitOutputs())
            {
                CurrentTransition.SimulationMark = SimulationMark.ExecutingObject;
                TargetState.SimulationMark = SimulationMark.ExecutingObject;
                bstate.SimulationMark = SimulationMark.ExecutingObjectExitOutputs;
                StepState = SimulationState.ExecutingExitOutputs;
            }
            else
            {
                MakeTransition();
            }
        }

        private void MakeTransition()
        {
            if (PreviousTransition != null) PreviousTransition.SimulationMark = SimulationMark.None;
            if (TargetState.Father != Node)
            {
                PreviousState = CurrentState;
                CurrentState.SimulationMark = SimulationMark.LeavingObject;
                CurrentTransition.SimulationMark = SimulationMark.ExecutingObject;
                SwitchStateUp();
            }
            else
            {
                if (CurrentTransition.HasOutputs())
                {
                    if (CurrentState is BasicMachine bmach)
                    {
                        bmach.SimulationMark = SimulationMark.LeavingObject;
                    }
                    else
                    {
                        CurrentState.SimulationMark = SimulationMark.LeavingObject;
                    }
                    TargetState.SimulationMark = SimulationMark.ExecutingObject;
                    CurrentTransition.SimulationMark = SimulationMark.ExecutingObjectExitOutputs;
                    StepState = SimulationState.ExecutingTransitionOutputs;
                }
                else
                {
                    EnterToState();
                }
            }
        }

        private void EnterToState()
        {
            ChangeState();
            if (TargetState.HasEntryOutputs())
            {
                PreviousState.SimulationMark = SimulationMark.None;
                CurrentState.SimulationMark = SimulationMark.ExecutingObjectEnterOutputs;
                StepState = SimulationState.ExecutingEnterOutputs;
            }
            else
            {
                FlagStateChanged();
            }
        }

        private void ChangeState()
        {
            if (CurrentState is BasicMachine bmach)
            {
                if (TargetState.Father != bmach)
                {
                    bmach.SimulationMark = SimulationMark.None;
                }
                if (SubMachine.CurrentState != null) SubMachine.CurrentState.SimulationMark = SimulationMark.None;
            }
            else
            {
                CurrentState.SimulationMark = SimulationMark.None;
            }
            PreviousState = CurrentState;
            CurrentState = TargetState;
            PreviousTransition = CurrentTransition;
            CurrentTransition = null;
            PreviousTransition.SimulationMark = SimulationMark.LeavingObject;
            CurrentState.SimulationMark = SimulationMark.ExecutingObject;
            if(CurrentState is BasicMachine bmach2)
            {
                SubMachine = new SubMachineStatus(this, bmach2, Tree);
            }
        }

        private void SwitchStateUp()
        {
            MachineStatus machine = this;
            bool hasFather = TargetState.HasFather(machine.Node);
            while (!hasFather && !machine.Node.HasExitOutputs())
            {
                machine.Node.SimulationMark = SimulationMark.None;
                machine = machine.Owner;
                if(PreviousTransition != null) PreviousTransition.SimulationMark = SimulationMark.None;
                hasFather = TargetState.HasFather(machine.Node);
            }
            if (hasFather)
            {
                if (CurrentTransition.HasOutputs())
                {
                    StepState = SimulationState.SwitchingStates;
                    machine.StepState = SimulationState.SwitchingTransitionOutputs;
                    machine.TargetState = TargetState;
                    machine.CurrentState.SimulationMark = SimulationMark.LeavingObject;
                    machine.CurrentTransition.SimulationMark = SimulationMark.ExecutingObjectExitOutputs;
                }
                else
                {
                    SwitchStateDown(machine);
                }
            }
            else if (machine.Node.HasExitOutputs())
            {
                StepState = SimulationState.SwitchingStates;
                machine = machine.Owner;
                machine.TargetState = TargetState;
                machine.CurrentState.SimulationMark = SimulationMark.ExecutingObjectExitOutputs;
                machine.StepState = SimulationState.SwitchingStatesUp;
            }
        }

        private void SwitchStateDown(MachineStatus machine)
        {
            PreviousState.SimulationMark = SimulationMark.None;
            IMachine nextFather = TargetState.NextFather(machine.Node);
            while (nextFather != null && !nextFather.HasEntryOutputs())
            {
                machine.SubMachine = new SubMachineStatus(machine, nextFather, Tree, false);
                machine.StepState = SimulationState.ExecutingSubMachine;
                machine.CurrentState = machine.SubMachine.Node as BasicMachine;
                machine = machine.SubMachine;
                Data.Store[Util.CounterName(machine.Name)] = 0;
                machine.Node.SimulationMark = SimulationMark.ExecutingObject;
                nextFather = TargetState.NextFather(machine.Node);
            }
            if (nextFather == null)
            {
                machine.CurrentState = TargetState;
                Data.Store[Util.CounterName(machine.Name)] = 0;
                PreviousTransition = CurrentTransition;
                CurrentTransition = null;
                PreviousTransition.SimulationMark = SimulationMark.LeavingObject;
                if (TargetState.HasEntryOutputs())
                {
                    machine.CurrentState.SimulationMark = SimulationMark.ExecutingObjectEnterOutputs;
                    machine.StepState = SimulationState.ExecutingEnterOutputs;
                }
                else
                {
                    machine.CurrentState.SimulationMark = SimulationMark.ExecutingObject;
                    FlagStateChanged();
                }
            }
            else if (nextFather.HasEntryOutputs())
            {
                machine.StepState = SimulationState.SwitchingStatesDown;
                machine.TargetState = TargetState;
                machine.CurrentState = nextFather as BasicMachine;
                machine.CurrentState.SimulationMark = SimulationMark.ExecutingObjectEnterOutputs;
            }
        }

        private bool TestTransition(BasicTransition transition)
        {
            return transition.Evaluate(Data.Store);
        }
    }
}
