using Cottle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Variables
{
    abstract class Output : Variable
    {
        public const string DefaultOutputName = "Output";

        public Output(string name)
            : base(name)
        {

        }
    }
}
