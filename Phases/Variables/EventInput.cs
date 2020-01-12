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
    }
}
