using Phases.DrawableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    interface IBasicGlobal
    {
        IGlobal RootObject { get; }
        string Name { get; }
    }
}
