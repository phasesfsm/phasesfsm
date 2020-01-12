using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.DrawableObjects
{
    interface IGlobal
    {
        int Priority { get; set; }
        string Name { get; set; }
    }
}
