using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases
{
    interface IMachineModel
    {
        VariableCollection Variables { get; }
        bool ExistsName(string name);
    }
}
