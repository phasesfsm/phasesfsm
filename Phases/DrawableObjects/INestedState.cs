using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.DrawableObjects
{
    public enum NestedPriority
    {
        FirstPriority,
        LastPriority
    }

    interface INestedState
    {
        NestedPriority Priority { get; set; }
        List<DrawableObject> ContainedObjects { get; }
        Origin Origin { get; }
    }
}
