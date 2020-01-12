using Phases.BasicObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Simulation
{
    class SubMachineStatus : MachineStatus
    {
        public SubMachineStatus(MachineStatus owner, IMachine mach, BasicObjectsTree tree, bool markStart = true)
            : base(owner.Data, mach, tree, markStart)
        {
            Owner = owner;
        }

        public override BasicTransition CurrentTransition { get => base.Owner.CurrentTransition; set => base.Owner.CurrentTransition = value; }
        public override BasicTransition PreviousTransition { get => base.Owner.PreviousTransition; set => base.Owner.PreviousTransition = value; }
        public override BasicObject PreviousState { get => base.Owner.PreviousState; set => base.Owner.PreviousState = value; }
    }
}
