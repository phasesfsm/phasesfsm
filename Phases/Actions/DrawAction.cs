using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Phases.DrawableObjects;

namespace Phases.Actions
{
    class DrawAction : RecordableAction
    {
        public DrawingSheet Sheet;
        public List<DrawableObject> DrawRef;        //Reference in draw
        public List<DrawableObject> ShadowState;    //Previous shadow state
        public List<DrawableObject> AfterAction;    //New shadow state
        public List<DrawableObject> Selection;
        public int FocusSelectionIndex;

        public DrawAction(ActionTypes actionType, DrawingSheet sheet, List<DrawableObject> objects,
            List<DrawableObject> selection, int focusIndex) : base(actionType)
        {
            Sheet = sheet;
            DrawRef = new List<DrawableObject>(objects);
            Selection = new List<DrawableObject>(selection);
            FocusSelectionIndex = focusIndex;
        }
    }
}
