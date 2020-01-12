using Cottle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Variables
{
    abstract class Flag : Variable, IConditional
    {
        public const string DefaultFlagName = "Flag";

        public Flag(string name)
            : base(name)
        {

        }
    }
}
