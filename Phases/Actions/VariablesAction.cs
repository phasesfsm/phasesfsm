using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Phases.Variables;

namespace Phases.Actions
{
    class VariablesAction : RecordableAction
    {
        public byte[] Before;
        public byte[] After;

        public VariablesAction(ActionTypes actionType, byte[] before, byte[] after)
            : base(actionType)
        {
            Before = before;
            After = after;
        }
    }
}
