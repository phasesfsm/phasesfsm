using Cottle;
using Phases.DrawableObjects;
using Phases.Expresions;
using Phases.Simulation;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    class BasicRoot : BasicState
    {
        public Origin Origin { get; private set; }
        public IMachine Owner { get; private set; }
        public override List<DrawableObject> ObjectList => new List<DrawableObject> { Origin };

        public override SimulationMark SimulationMark { get => Origin.SimulationMark; set => Origin.SimulationMark = value; }

        public BasicRoot(IMachine owner, Origin origin)
            : base(origin)
        {
            Origin = origin;
            Owner = owner;
        }

        public BasicTransition Transition { get; set; }

        public override string Name => Origin.Name;
        public override string Alias => Owner is BasicObjectsTree ? Origin.Name : Owner.Name;

        public override IMachine Father => Owner;

        public bool HasFirstPriority() => false;

        public bool HasLastPriority() => false;
    }
}
