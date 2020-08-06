using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualText
{
    class ActionsManager
    {
        private Stack<TextAction> UndoActions { get; set; }
        private Stack<TextAction> RedoActions { get; set; }

        public ActionsManager()
        {
            UndoActions = new Stack<TextAction>();
            RedoActions = new Stack<TextAction>();
        }

        public TextAction Add(TextCursor cursor, TextAction.ActionType type)
        {
            TextAction action = new TextAction(cursor, type);
            UndoActions.Push(action);
            if (RedoActions.Any()) RedoActions.Clear();
            return action;
        }
        public TextAction PeekOrAdd(TextCursor cursor, TextAction.ActionType type)
        {
            if (UndoActions.Any() && UndoActions.Peek().Match(cursor, type))
            {
                return UndoActions.Peek();
            }
            else
            {
                return Add(cursor, type);
            }
        }
        public TextAction PeekIf(TextCursor cursor, TextAction.ActionType type, Func<TextAction, bool> condition)
        {
            if (UndoActions.Any() && UndoActions.Peek().Match(cursor, type) && condition(UndoActions.Peek()))
            {
                return UndoActions.Peek();
            }
            else
            {
                return Add(cursor, type);
            }
        }
        private string GetText(Stack<TextAction> actions) => actions.Any() ? actions.Peek().Type.ToString().Replace("__", "/").Replace("_", " ") : "";
        public string UndoText => GetText(UndoActions);
        public string RedoText => GetText(RedoActions);
        public bool Undo(out TextCursor cursor)
        {
            cursor = null;
            if (!UndoActions.Any()) return false;
            var action = UndoActions.Pop();
            cursor = action.Cursor;
            cursor.Undo(action);
            RedoActions.Push(action);
            return UndoActions.Any();
        }
        public bool Redo(out TextCursor cursor)
        {
            cursor = null;
            if (!RedoActions.Any()) return false;
            var action = RedoActions.Pop();
            cursor = action.Cursor;
            cursor.Redo(action);
            UndoActions.Push(action);
            return RedoActions.Any();
        }
    }
}
