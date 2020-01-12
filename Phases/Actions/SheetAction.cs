using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Phases.DrawableObjects;

namespace Phases.Actions
{
    class SheetAction : RecordableAction
    {
        public byte[] Data;

        public SheetAction(ActionTypes actionType, byte[] data)
            : base(actionType)
        {
            Data = data;
        }
    }
}
