using Cottle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Variables
{
    class MessageFlag : Flag, IIndirectInput, IInternalOutput
    {
        public MessageFlag(string name)
            : base(name)
        {

        }

        public Value Evaluate(OperationType operation, Value currentValue)
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
