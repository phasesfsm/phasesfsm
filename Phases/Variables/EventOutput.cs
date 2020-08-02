using Cottle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Variables
{
    class EventOutput : Output, IInternalOutput
    {
        public EventOutput(string name)
            :base(name)
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
