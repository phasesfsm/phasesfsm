using Cottle;
using Phases.DrawableObjects;
using Phases.Simulation;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    class BasicOutput : BasicObject
    {
        public override string Name => Output.Name;
        public override string Alias => Output.Name;

        public OperationType Operation { get; private set; }
        public Variable Output { get; private set; }

        public BasicOutput(OperationType operation, Variable output)
        {
            Operation = operation;
            Output = output;
        }

        public override string ToString()
        {
            switch (Operation)
            {
                case OperationType.Clear:
                    return "!" + Output.Name;
                case OperationType.Increment:
                    return Output.Name + "+";
                case OperationType.Decrement:
                    return Output.Name + "-";
                case OperationType.Maximum:
                    return "'" + Output.Name;
                case OperationType.Minimum:
                    return "." + Output.Name;
                case OperationType.Toggle:
                    return "~" + Output.Name;
            }
            return Output.Name;
        }

        public Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                { "Name", Name },
                { "Action", Operation.ToString() }
            };
        }
    }
}
