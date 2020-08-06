using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Phases.DrawableObjects;

namespace Phases.Actions
{
    abstract class RecordableAction
    {
        public enum ActionTypes
        {
            Create,
            Move,
            MoveText,
            Resize,
            Rename,
            Edit,
            Remove,
            Cut,
            Paste,
            PropertyChanged,
            VariablesChanged,
            ModelVariablesChanged,
            AddSheet,
            DeleteSheet,
            AddModel,
            DeleteModel,
            SheetParameterChanged
        }

        public ActionTypes ActionType;

        public RecordableAction(ActionTypes actionType)
        {
            ActionType = actionType;
        }

        public static string ActionName(ActionTypes action)
        {
            switch (action)
            {
                case ActionTypes.Create:
                    return "create object.";
                case ActionTypes.Cut:
                    return "cut.";
                case ActionTypes.Edit:
                    return "edit.";
                case ActionTypes.Move:
                    return "move object.";
                case ActionTypes.MoveText:
                    return "move text.";
                case ActionTypes.Paste:
                    return "paste.";
                case ActionTypes.PropertyChanged:
                    return "property changed.";
                case ActionTypes.Remove:
                    return "remove.";
                case ActionTypes.Rename:
                    return "rename";
                case ActionTypes.Resize:
                    return "resize.";
                default:
                    return "unknown.";
            }
        }
    }
}
