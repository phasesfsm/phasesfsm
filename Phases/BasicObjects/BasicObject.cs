using Phases.DrawableObjects;
using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    abstract class BasicObject
    {
        public abstract string Name { get; }
        public abstract string Alias { get; }

        public virtual List<DrawableObject> ObjectList => new List<DrawableObject>();
        public virtual SimulationMark SimulationMark { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IMachine Father { get; protected set; } = null;

        public bool HasFather(IMachine machine)
        {
            IMachine afather = Father;
            while (!(afather is BasicObjectsTree) && afather != machine)
            {
                afather = afather.Father;
            }
            return machine == afather;
        }

        public IMachine NextFather(IMachine machine)
        {
            IMachine afather = Father, pFather = null;
            while (!(afather is BasicObjectsTree) && afather != machine)
            {
                pFather = afather;
                afather = afather.Father;
            }
            return pFather;
        }

        public override string ToString() => Name;
    }
}
