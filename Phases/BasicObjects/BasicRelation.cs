using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cottle;
using Phases.DrawableObjects;
using Phases.Simulation;
using Phases.Variables;

namespace Phases.BasicObjects
{
    class BasicRelation : BasicObject, IBasicGlobal
    {
        public override string Name => Relation.Name;
        public override string Alias => Relation.Name;

        public Relation Relation { get; private set; }
        public IIndirectInput Input { get; private set; }
        public BasicOutput Action { get; private set; }
        public override List<DrawableObject> ObjectList => new List<DrawableObject> { Relation };

        public override SimulationMark SimulationMark { get => Relation.SimulationMark; set => Relation.SimulationMark = value; }

        public BasicRelation(Relation Relation, IIndirectInput input, BasicOutput action)
        {
            this.Relation = Relation;
            Input = input;
            Action = action;
        }

        public IGlobal RootObject => Relation;

        public Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                { "Name", Name },
                { "Input", Input.Name },
                { "Action", Action.Operation.ToString() },
                { "Output", Action.Output.Name }
            };
        }

        public bool Evaluate(IStore store)
        {
            return store[Relation.Trigger].AsBoolean;
        }

        public void Execute(IStore store)
        {
            store[Action.Output.Name] = Action.Output.Evaluate(Action.Operation, store[Action.Output.Name]);
        }
    }
}
