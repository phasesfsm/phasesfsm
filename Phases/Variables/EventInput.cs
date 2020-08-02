using Cottle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Variables
{
    class EventInput : Input, IIndirectInput
    {
        public EventInput(string name)
            : base(name)
        {

        }

        public override Value Evaluate(OperationType operation, Value currentValue)
        {
            switch (operation)
            {
                case OperationType.Send:
                    return true;
                default:
                    return false;
            }
        }
    }
}
