using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualText
{
    class TextAction
    {
        public enum ActionType
        {
            Delete_text,
            Erase_backwards,
            Delete_chars,
            Replace_text,
            Write_text,
            Move_text,
            Insert_new_line,
            Cut_text,
            Paste_text,
            Paste__replace_text,
            Replace_with_Tab,
            Insert_Tab,
            Remove_Tab,
            Ident_text,
            Move_text_up,
            Move_text_down
        }
        public TextCursor Cursor { get; private set; }
        public ActionType Type { get; private set; }
        public StringBuilder Text { get; set; }
        public StringBuilder AfterText { get; set; }
        public Point Selection { get; private set; }
        public Point Location { get; private set; }
        public Point AfterSelection { get; private set; }
        public Point AfterLocation { get; private set; }
        public TextAction(TextCursor cursor, ActionType type)
        {
            Cursor = cursor;
            Type = type;
            Text = new StringBuilder();
            AfterText = new StringBuilder();
            Selection = Cursor.Selection;
            Location = Cursor.Location;
        }
        public void SaveAfter()
        {
            AfterSelection = Cursor.Selection;
            AfterLocation = Cursor.Location;
        }
        public bool Match(TextCursor cursor, ActionType type)
        {
            return cursor == Cursor && type == Type && AfterSelection == cursor.Selection && AfterLocation == cursor.Location;
        }
    }
}
