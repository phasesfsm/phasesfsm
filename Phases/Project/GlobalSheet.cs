using Phases.DrawableObjects;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases
{
    class GlobalSheet : DrawingSheet
    {
        public override VariableCollection Variables => OwnerBook.Variables;
        public override List<IGlobal> Globals => OwnerBook.Globals;
        internal GlobalSheet(PhasesBook ownerBook, string sheetName, Size size, int imageIndex = Constants.ImageIndex.SubSheet)
            : base(ownerBook, sheetName, size, imageIndex)
        {
            
        }
    }
}
