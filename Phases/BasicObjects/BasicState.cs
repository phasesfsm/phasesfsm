using Cottle;
using Phases.DrawableObjects;
using Phases.Expresions;
using Phases.Simulation;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    class BasicState : BasicObject
    {
        public override string Name => State.Name;
        public override string Alias => State.Name;

        public List<BasicTransition> Transitions { get; protected set; }      //Transitions to get out the state
        public List<BasicOutput> EnterOutputs { get; private set; }
        public List<BasicOutput> ExitOutputs { get; private set; }
        public override List<DrawableObject> ObjectList => new List<DrawableObject>();

        public override SimulationMark SimulationMark { get => State.SimulationMark; set => State.SimulationMark = value; }

        //Primitive fields
        public IState State { get; private set; }

        public BasicState(IState state)
        {
            Transitions = new List<BasicTransition>();
            EnterOutputs = new List<BasicOutput>();
            ExitOutputs = new List<BasicOutput>();
            State = state;
            foreach (string outputAction in State.EnterOutputsList)
            {
                string outputName = LexicalRules.GetOutputId(outputAction);
                OperationType operation = LexicalRules.GetOutputOperation(outputAction);
                if (State.OwnerDraw.OwnerSheet.OwnerBook.Variables.InternalOutputs.FirstOrDefault(output => output.Name == outputName) is IInternalOutput ioutput)
                {
                    EnterOutputs.Add(new BasicOutput(operation, ioutput));
                }
            }
            foreach (string outputAction in State.ExitOutputsList)
            {
                string outputName = LexicalRules.GetOutputId(outputAction);
                OperationType operation = LexicalRules.GetOutputOperation(outputAction);
                if (State.OwnerDraw.OwnerSheet.OwnerBook.Variables.InternalOutputs.FirstOrDefault(output => output.Name == outputName) is IInternalOutput ioutput)
                {
                    ExitOutputs.Add(new BasicOutput(operation, ioutput));
                }
            }
        }

        public void SetFather(IMachine thisFather)
        {
            if (Father != null) throw new Exception("Error: state already has a father state.");
            Father = thisFather;
        }

        public bool HasInputs()
        {
            return State.InTransitions.Length > 0;
        }

        public bool HasEntryOutputs()
        {
            return EnterOutputs.Count > 0;
        }

        public bool HasExitOutputs()
        {
            return ExitOutputs.Count > 0;
        }

        public virtual Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                { "Name", Name },
                { "Description", State.Description },
                { "Father", Father.Name },
                { "EnterOutputs", EnterOutputs.ConvertAll(output => (Value)output.GetDictionary()) },
                { "ExitOutputs", ExitOutputs.ConvertAll(output => (Value)output.GetDictionary()) },
                { "Transitions", Transitions.ConvertAll(trans => (Value)trans.Name) }
            };
        }
    }
}
