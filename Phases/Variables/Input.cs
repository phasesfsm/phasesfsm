using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Variables
{
    abstract class Input : Variable, IConditional
    {
        public const string DefaultInputName = "Input";

        public Input(string name)
            : base(name)
        {

        }
    }
}
