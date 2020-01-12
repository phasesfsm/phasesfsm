using Cottle;
using Phases.DrawableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    class BasicMachine : BasicState, IMachine
    {
        List<BasicState> states;    //Includes BasicStates and BasicMachines
        public BasicRoot Root { get; set; }
        public Origin Origin => Root == null ? null : Root.Origin;
        public IGlobal RootObject => Origin as IGlobal;
        public BasicTransition Transition { get; set; }

        public BasicMachine(State state)
            : base(state)
        {
            states = new List<BasicState>();
        }

        public List<BasicState> States => states;

        public IReadOnlyList<BasicState> StatesList()
        {
            return States.FindAll(st => !(st is BasicMachine));
        }

        public IReadOnlyList<BasicMachine> SuperStatesList()
        {
            return States.FindAll(st => st is BasicMachine).ConvertAll(st => st as BasicMachine);
        }

        public bool HasFirstPriority() => (State as SuperState).Priority == NestedPriority.FirstPriority;

        public bool HasLastPriority() => (State as SuperState).Priority == NestedPriority.LastPriority;

        public bool HasOrigin() => Origin != null;

        public override Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                { "Name", Name },
                { "Description", State.Description },
                { "Father", Father.Name },
                { "EnterOutputs", EnterOutputs.ConvertAll(output => (Value)output.GetDictionary()) },
                { "ExitOutputs", ExitOutputs.ConvertAll(output => (Value)output.GetDictionary()) },
                { "Transitions", Transitions.ConvertAll(trans => (Value)trans.Name) },
                { "SuperStates", SuperStatesList().ToList().ConvertAll(state => (Value)state.Name) },
                { "States", StatesList().ToList().ConvertAll(state => (Value)state.Name) }
            };
        }
    }
}
