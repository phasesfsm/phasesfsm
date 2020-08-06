using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DualText
{
    class TextCursor
    {
        public Text Text { get; private set; }
        public ActionsManager Actions { get; private set; }
        public Point Location;
        public Point Selection;
        public int X
        {
            get => Location.X;
            set
            {
                Location.X = Math.Max(value, 0);
                if (!Selecting && !SideSelecting) Selection = Location;
            }
        }
        public int Y
        {
            get => Location.Y;
            set
            {
                if (SideSelecting)
                {
                    if (value >= sideSelectionStartLine)
                    {
                        Selection.X = 0;
                        Selection.Y = sideSelectionStartLine;
                        Location.Y = Math.Min(value + 1, Text.LastLine);
                    }
                    else
                    {
                        if (sideSelectionStartLine == Text.LastLine)
                        {
                            Selection.X = Text.Lines[Text.LastLine].Length;
                            Selection.Y = sideSelectionStartLine;
                        }
                        else
                        {
                            Selection.X = 0;
                            Selection.Y = sideSelectionStartLine + 1;
                        }
                        Location.Y = Math.Max(value, 0);
                    }
                }
                else
                {
                    Location.Y = Math.Min(Math.Max(value, 0), Text.LastLine);
                    if (!Selecting) Selection = Location;
                }
            }
        }
        public bool Selecting;
        public bool SideSelecting { get; private set; }
        private int sideSelectionStartLine = 0;
        public Point? MovingText;

        private const string TOKENS_GROUP = "tokens";
        private const string UNGROUPED_OTHER = "other";
        private const string SPACES_GROUP = "other";
        private static readonly Regex wordsRegex = new Regex(@"(?<" + TOKENS_GROUP + @">\w+)|(?<" + SPACES_GROUP + @">\s+)|(?<" + UNGROUPED_OTHER + @">[^\s\w]+)");

        public TextCursor(Text text, ActionsManager actionsManager)
        {
            Text = text;
            Actions = actionsManager;
        }

        public void Draw(Graphics g)
        {
            int line = Math.Max(0, Math.Min(Text.LastDrawLine, Location.Y));
            int x = Math.Min(Location.X, Text.Lines[line].Length) * BaseFormat.FontWidth + (int)BaseFormat.CursorPen.Width + BaseFormat.LeftMargin;
            int y = Text.Lines[line].LinePosition * BaseFormat.FontHeight;
            g.DrawLine(BaseFormat.CursorPen, x, y, x, y + BaseFormat.FontHeight);
        }

        #region "Text info"
        public bool IsLocationOverText(Point location) => IsLocationOverText(location.Y, location.X);
        public bool IsLocationOverText(int line, int column) => line >= 0 && line < Text.Lines.Count && Text.Lines[line] != null && column >= 0 && column <= Text.Lines[line].Length;
        public int GetIndex(Point location) => GetIndex(location.Y, location.X);
        public int GetIndex(int line, int column = 0)
        {
            if (line > Text.LastLine) line = Text.LastLine;
            return Text.Lines.Take(line).Sum(ln => ln.Length + BaseFormat.NewLine.Length) + Math.Min(column, Text.Lines[line].Length);
        }
        public Point GetLocationFromIndex(int index)
        {
            int y = Text.ToString(0, index).Split(new string[] { BaseFormat.NewLine }, StringSplitOptions.None).Length - 1;
            int lineIndex = GetIndex(y, 0);
            return new Point(index - lineIndex, y);
        }
        #endregion

        #region "Cursor info"
        public int LineEnd => CurrentLine.Length;
        public string CurrentLine => Text.Lines[Y].ToString();
        #endregion

        #region "Cursor moves"
        public void MoveWord(int dw)
        {
            int column = Math.Min(X, LineEnd);
            if (dw >= 0)    // Move one word to the right
            {
                if (column == LineEnd)
                {
                    MoveX(+1);
                }
                else
                {
                    var tokens = wordsRegex.Matches(CurrentLine.Substring(X));
                    int index = 1;
                    foreach (Match match in tokens)
                    {
                        if (index == 1)
                            index = LineEnd - column;
                        else if (match.Groups[TOKENS_GROUP].Success || (match.Groups[UNGROUPED_OTHER].Success && !match.Groups[SPACES_GROUP].Success))
                        {
                            index = match.Index;
                            break;
                        }
                    }
                    X += index;
                }
            }
            else            // Move one word to the left
            {
                if (column == 0)
                {
                    MoveX(-1);
                }
                else
                {
                    string reversed = new string(CurrentLine.Substring(0, column).Reverse().ToArray());
                    var tokens = wordsRegex.Matches(reversed);
                    int index = column;
                    foreach (Match match in tokens)
                    {
                        if (match.Groups[TOKENS_GROUP].Success || (match.Groups[UNGROUPED_OTHER].Success && !match.Groups[SPACES_GROUP].Success))
                        {
                            index = match.Index + match.Length;
                            break;
                        }
                    }
                    X = column - index;
                }
            }
            Text.View.AdjustVisibleArea();
        }
        public void MoveX(int dx)
        {
            if (dx == 0) return;
            X = Math.Min(X, LineEnd);
            int movx = X + dx;
            if (movx >= 0 && movx <= LineEnd)
            {
                X = movx;
            }
            else if (dx > 0)
            {
                if (MoveY(1))
                {
                    X = 0;
                }
            }
            else if (dx < 0)
            {
                if (MoveY(-1))
                {
                    X = LineEnd;
                }
            }
            Text.View.AdjustVisibleArea();
        }
        public bool MoveY(int dy)
        {
            if (dy == 0) return false;
            int movy = Y + dy;
            if (movy >= 0 && movy < Text.Lines.Count)
            {
                if (Selecting) X = Math.Min(X, LineEnd);
                Y = movy;
            }
            else if (dy > 0)
            {
                dy = Text.LastLine - Y;
                if (dy == 0) return false;
                movy = Y + dy;
                Y = movy;
            }
            else
            {
                dy = -Y;
                if (dy == 0) return false;
                movy = Y + dy;
                Y = movy;
            }
            Text.View.AdjustVisibleArea();
            return true;
        }
        public void MoveTo(Point location) => MoveTo(location.X, location.Y);
        public void MoveTo(int column, int line = -1)
        {
            if (line >= 0) Y = line;
            if (SideSelecting && line >= Text.LastLine) X = LineEnd;
            else X = Math.Min(column, LineEnd);
            Text.View.AdjustVisibleArea();
        }
        public Point ValidateLocation(Point location)
        {
            int x, y;
            y = Math.Min(Math.Max(location.Y, 0), Text.LastLine);
            x = Math.Min(Math.Max(location.X, 0), Text.Lines[y].Length);
            return new Point(x, y);
        }
        public void MoveToEnd()
        {
            Y = Text.LastLine;
            X = LineEnd;
            Text.View.AdjustVisibleArea();
        }

        #endregion

        #region "Selections"
        public int SelectionStart => GetIndex(Selection.Y, Selection.X);
        public int SelectionEnd => GetIndex(Location.Y, Location.X);
        public int SelectionLength => SelectionStart - SelectionEnd;
        public int SelectionIndex => Math.Min(SelectionStart, SelectionEnd);
        public int SelectionCount => Math.Abs(SelectionLength);
        public string SelectedText => Text.ToString(SelectionIndex, SelectionCount);
        public Rectangle SelectionRectangle => new Rectangle(Location, new Size(Location) - new Size(Selection));

        public bool IsSelected(Point location) => IsSelected(location.Y, location.X);
        public bool IsSelected(int line, int column)
        {
            if (!IsLocationOverText(line, column)) return false;
            int index = GetIndex(line, column);
            if (SelectionStart < SelectionEnd)
            {
                return index >= SelectionStart && index < SelectionEnd;
            }
            else
            {
                return index >= SelectionEnd && index < SelectionStart;
            }
        }
        public bool IsSelected(int line)
        {
            return line >= Math.Min(Location.Y, Selection.Y) && line <= Math.Max(Location.Y, Selection.Y);
        }
        public bool GetLineSelection(int line, out int start, out int count)
        {
            start = 0;
            count = 0;
            if (Location.Y == Selection.Y)
            {
                if (line != Location.Y || Location.X == Selection.X) return false;
                if (Location.X > Selection.X)
                {
                    start = Selection.X;
                    count = Location.X - Selection.X;
                }
                else
                {
                    start = Location.X;
                    count = Selection.X - Location.X;
                }
                return true;
            }
            else if (Location.Y > Selection.Y)
            {
                if (line < Selection.Y || line > Location.Y) return false;
                if (line == Selection.Y)
                {
                    start = Selection.X;
                    count = Text.Lines[line].Length - start;
                }
                else if (line == Location.Y)
                {
                    start = 0;
                    count = Location.X;
                }
                else
                {
                    start = 0;
                    count = Text.Lines[line].Length;
                }
                return true;
            }
            else
            {
                if (line < Location.Y || line > Selection.Y) return false;
                if (line == Location.Y)
                {
                    start = Location.X;
                    count = Text.Lines[line].Length - start;
                }
                else if (line == Selection.Y)
                {
                    start = 0;
                    count = Selection.X;
                }
                else
                {
                    start = 0;
                    count = Text.Lines[line].Length;
                }
                return true;
            }
        }
        public void SelectWord(Point location) => SelectWord(location.Y, location.X);
        public void SelectWord(int line, int column)
        {
            if (!IsLocationOverText(line, column)) return;
            string fromIndex = Text.Lines[line].Substring(column);
            string reversed = new string(Text.Lines[line].Substring(0, column).Reverse().ToArray());
            var tokens = wordsRegex.Matches(fromIndex);
            int right = 0;
            Match match = tokens[0];
            if (match.Groups[TOKENS_GROUP].Success || (match.Groups[UNGROUPED_OTHER].Success && !match.Groups[SPACES_GROUP].Success))
            {
                right = match.Length;
            }
            tokens = wordsRegex.Matches(reversed);
            int left = 0;
            match = tokens[0];
            if (match.Groups[TOKENS_GROUP].Success || (match.Groups[UNGROUPED_OTHER].Success && !match.Groups[SPACES_GROUP].Success))
            {
                left = match.Length;
            }
            Selection = new Point(column - left, line);
            Location = new Point(column + right, line);
        }
        private void GetSelectedLinesRange(out int start_line, out int end_line)
        {
            if (Selection.Y == Location.Y)
            {
                start_line = Location.Y;
                end_line = start_line;
            }
            else if (Location.Y > Selection.Y)
            {
                start_line = Selection.Y;
                end_line = Location.Y;
                if (Location.X == 0) end_line--;
            }
            else
            {
                start_line = Location.Y;
                end_line = Selection.Y;
                if (Selection.X == 0) end_line--;
            }
        }
        public void CopySelection()
        {
            try
            {
                Clipboard.SetText(SelectedText);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error copying text: " + ex.Message, "DualTextBox", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void SelectAll()
        {
            Selection = new Point(0, 0);
            Selecting = true;
            MoveToEnd();
            Selecting = false;
        }
        public void Unselect()
        {
            Selecting = false;
            SideSelecting = false;
            MovingText = null;
            Selection = Location;
            Text.View.AdjustVisibleArea();
        }
        public void StartSideSelection(int line)
        {
            sideSelectionStartLine = Math.Min(line, Text.LastLine);
            MoveTo(0, line);
            SideSelecting = true;
            MoveTo(0, line);
            Y = line;
        }
        public void StopSideSelection()
        {
            SideSelecting = false;
        }
        #endregion

        #region "Editions"
        public void Delete(bool backspace = false)
        {
            if (Text.ReadOnly || (SelectionCount == 0 && (backspace && SelectionIndex == 0)
                || (!backspace && SelectionIndex == Text.Length))) return;
            TextAction action;
            if (SelectionCount > 0)
            {
                action = Actions.Add(this, TextAction.ActionType.Delete_text);
                action.Text.Append(Text.Builder.ToString(SelectionIndex, SelectionCount));
                Text.Builder.Remove(SelectionIndex, SelectionCount);
            }
            else
            {
                int offset;
                if (backspace)
                {
                    action = Actions.PeekOrAdd(this, TextAction.ActionType.Erase_backwards);
                    offset = X == 0 ? 2 : 1;
                    action.Text.Insert(0, Text.Builder.ToString(SelectionIndex - offset, offset));
                    MoveX(-offset);
                }
                else
                {
                    action = Actions.PeekOrAdd(this, TextAction.ActionType.Delete_chars);
                    offset = X == LineEnd ? 2 : 1;
                    action.Text.Append(Text.Builder.ToString(SelectionIndex, offset));
                }
                Text.Builder.Remove(SelectionIndex, offset);
            }
            Location = GetLocationFromIndex(SelectionIndex);
            Text.Rebuild_From_Builder();
            //Box.recalcBoundaries();
            action.SaveAfter();
        }
        public void MoveSelectedText(Point newLocation)
        {
            if (Text.ReadOnly) return;
            var index = GetIndex(newLocation);
            var start = SelectionIndex;
            var count = SelectionCount;
            TextAction action = Actions.Add(this, TextAction.ActionType.Move_text);
            action.Text.Append(Text.Builder.ToString(start, count));
            if (index < start)
            {
                Text.Builder.Remove(start, count);
                Text.Builder.Insert(index, SelectedText);
                Location = GetLocationFromIndex(SelectionEnd - (start - index));
                Selection = GetLocationFromIndex(SelectionStart - (start - index));
            }
            else
            {
                Text.Builder.Insert(index, SelectedText);
                Text.Builder.Remove(start, count);
                Location = GetLocationFromIndex(SelectionEnd + index - start - count);
                Selection = GetLocationFromIndex(SelectionStart + index - start - count);
            }
            Text.Rebuild_From_Builder();
            action.SaveAfter();
        }
        public void Insert(char ch)
        {
            if (Text.ReadOnly) return;
            TextAction action;
            if (SelectionCount > 0)
            {
                action = Actions.Add(this, TextAction.ActionType.Replace_text);
                action.Text.Append(Text.Builder.ToString(SelectionIndex, SelectionCount));
                Text.Builder.Remove(SelectionIndex, SelectionCount);
                Location = GetLocationFromIndex(SelectionIndex);
            }
            else
            {
                action = Actions.PeekIf(this, TextAction.ActionType.Write_text, a =>
                {
                    string text = a.AfterText.ToString();
                    if (ch == ' ')  // space
                    {
                        if (text.Length > 1 && text[1] == ' ') return true;
                        else if (text.Length == 1 && text[0] == ' ') return true;
                    }
                    else    // any other character
                    {
                        if (text.Length > 1 && text[1] == ' ') return false;
                        return true;
                    }
                    return false;
                });
            }
            Text.Builder.Insert(SelectionIndex, ch);
            action.AfterText.Append(ch);
            Text.Rebuild_From_Builder();
            //Box.recalcBoundaries();
            Location.X++;
            Selection = Location;
            action.SaveAfter();
            Text.View.AdjustVisibleArea();
        }
        public void Enter()
        {
            if (Text.ReadOnly) return;
            TextAction action;
            if (SelectionCount > 0)
            {
                action = Actions.Add(this, TextAction.ActionType.Replace_text);
                action.Text.Append(Text.Builder.ToString(SelectionIndex, SelectionCount));
                Text.Builder.Remove(SelectionIndex, SelectionCount);
            }
            else
            {
                action = Actions.PeekOrAdd(this, TextAction.ActionType.Insert_new_line);
            }
            Text.Builder.Insert(SelectionIndex, BaseFormat.NewLine);
            action.AfterText.Append(BaseFormat.NewLine);
            Text.Rebuild_From_Builder();
            //Box.recalcBoundaries();
            MoveX(+1);
            Selection = Location;
            action.SaveAfter();
        }
        public void MoveLineUp()
        {
            if (Text.ReadOnly) return;
            GetSelectedLinesRange(out int start_line, out int end_line);
            if (start_line == 0) return;
            TextAction action = Actions.Add(this, TextAction.ActionType.Move_text_up);
            start_line--;
            var line = Text.Lines[start_line];
            Text.Lines.RemoveAt(start_line);
            Text.Lines.Insert(end_line, line);
            Selection.Y--;
            Location.Y--;
            Text.Rebuild_From_Lines();
            //Box.recalcBoundaries();
            action.SaveAfter();
            Text.View.AdjustVisibleArea();
        }
        public void MoveLineDown()
        {
            if (Text.ReadOnly) return;
            GetSelectedLinesRange(out int start_line, out int end_line);
            if (end_line == Text.LastLine) return;
            TextAction action = Actions.Add(this, TextAction.ActionType.Move_text_down);
            end_line++;
            var line = Text.Lines[end_line];
            Text.Lines.RemoveAt(end_line);
            Text.Lines.Insert(start_line, line);
            if (Selection.Y > Location.Y && Selection.Y == Text.LastLine)
            {
                Selection.X = Text.Lines[Text.LastLine].Length;
                Location.Y++;
            }
            else if (Location.Y > Selection.Y && Location.Y == Text.LastLine)
            {
                Location.X = Text.Lines[Text.LastLine].Length;
                Selection.Y++;
            }
            else
            {
                Selection.Y++;
                Location.Y++;
            }
            Text.Rebuild_From_Lines();
            //Box.recalcBoundaries();
            action.SaveAfter();
            Text.View.AdjustVisibleArea();
        }
        public void Tab(bool shifted)
        {
            if (Text.ReadOnly) return;
            TextAction action;
            if (SelectionCount > 0)
            {
                if (!shifted && Selection.Y == Location.Y && Location.Y < Text.LastLine)
                {
                    // one line selection
                    action = Actions.Add(this, TextAction.ActionType.Replace_with_Tab);
                    action.Text.Append(Text.Builder.ToString(SelectionIndex, SelectionCount));
                    Text.Builder.Remove(SelectionIndex, SelectionCount);
                    int tab_adjust = BaseFormat.TabSpaces - X % BaseFormat.TabSpaces;
                    string tab = new string(' ', tab_adjust);
                    Text.Builder.Insert(SelectionIndex, tab);
                    action.AfterText.Append(tab);
                    X += tab.Length;
                    Selection = Location;
                }
                else
                {
                    // multiline selection
                    GetSelectedLinesRange(out int start_line, out int end_line);
                    if (shifted)    // un-ident
                    {
                        if (!Text.Lines.Skip(start_line).Take(end_line - start_line).Any(ln => ln.Chars.TakeWhile(ch => ch.Char == ' ').Any())) return;
                        action = Actions.Add(this, TextAction.ActionType.Ident_text);
                        for (int line = end_line; line >= start_line; line--)
                        {
                            action.Text.Insert(0, Text.Lines[line] + (line == Text.LastLine ? "" : BaseFormat.NewLine));
                            int spaces_back = Text.Lines[line].Chars.TakeWhile(ch => ch.Char == ' ').Count();
                            if (spaces_back == 0)
                            {
                                action.AfterText.Insert(0, Text.Lines[line] + (line == Text.LastLine ? "" : BaseFormat.NewLine));
                                continue;
                            }
                            int tab_adjust = spaces_back % BaseFormat.TabSpaces;
                            if (tab_adjust == 0) tab_adjust = 4;
                            if (tab_adjust > spaces_back) tab_adjust = spaces_back;
                            Text.Builder.Remove(GetIndex(line), tab_adjust);
                            action.AfterText.Insert(0, Text.Lines[line].Substring(tab_adjust) + (line == Text.LastLine ? "" : BaseFormat.NewLine));
                            if (Location.Y == line)
                            {
                                if (Location.X == spaces_back) Location.X -= tab_adjust;
                                else if (spaces_back - tab_adjust < Location.X) Location.X = spaces_back - tab_adjust;
                            }
                            if (Selection.Y == line)
                            {
                                if (Selection.X == spaces_back) Selection.X -= tab_adjust;
                                else if (spaces_back - tab_adjust < Selection.X) Selection.X = spaces_back - tab_adjust;
                            }
                        }
                    }
                    else    // ident
                    {
                        action = Actions.Add(this, TextAction.ActionType.Ident_text);
                        for (int line = end_line; line >= start_line; line--)
                        {
                            action.Text.Insert(0, Text.Lines[line] + (line == Text.LastLine ? "" : BaseFormat.NewLine));
                            if (Text.Lines[line].Length > 0)
                            {
                                string tab = new string(' ', BaseFormat.TabSpaces);
                                Text.Builder.Insert(GetIndex(line), tab);
                                action.AfterText.Insert(0, tab + Text.Lines[line] + (line == Text.LastLine ? "" : BaseFormat.NewLine));
                                if (Location.Y == line && Location.X > 0) Location.X += BaseFormat.TabSpaces;
                                if (Selection.Y == line && Selection.X > 0) Selection.X += BaseFormat.TabSpaces;
                            }
                            else
                            {
                                action.AfterText.Insert(0, Text.Lines[line] + (line == Text.LastLine ? "" : BaseFormat.NewLine));
                            }
                        }
                    }
                }
            }
            else if (shifted)
            {
                // shifted tab
                int spaces_back = CurrentLine.Take(X).Reverse().TakeWhile(ch => ch == ' ').Count();
                if (spaces_back == 0) return;
                action = Actions.Add(this, TextAction.ActionType.Remove_Tab);
                int tab_adjust = X % BaseFormat.TabSpaces;
                if (tab_adjust == 0) tab_adjust = 4;
                if (tab_adjust > spaces_back) tab_adjust = spaces_back;
                action.Text.Append(Text.Builder.ToString(SelectionIndex - tab_adjust, tab_adjust));
                Text.Builder.Remove(SelectionIndex - tab_adjust, tab_adjust);
                X -= tab_adjust;
                Selection = Location;
            }
            else
            {
                // normal tab
                action = Actions.Add(this, TextAction.ActionType.Insert_Tab);
                int tab_adjust = BaseFormat.TabSpaces - X % BaseFormat.TabSpaces;
                string tab = new string(' ', tab_adjust);
                Text.Builder.Insert(SelectionIndex, tab);
                action.AfterText.Append(tab);
                X += tab.Length;
                Selection = Location;
            }
            Text.Rebuild_From_Builder();
            //Box.recalcBoundaries();
            action.SaveAfter();
        }

        public void Paste()
        {
            if (Text.ReadOnly || !Clipboard.ContainsText()) return;
            TextAction action;
            if (SelectionCount > 0)
            {
                action = Actions.Add(this, TextAction.ActionType.Paste__replace_text);
                action.Text.Append(Text.Builder.ToString(SelectionIndex, SelectionCount));
                Text.Builder.Remove(SelectionIndex, SelectionCount);
                Location = GetLocationFromIndex(SelectionIndex);
            }
            else
            {
                action = Actions.Add(this, TextAction.ActionType.Paste_text);
            }
            string text = Clipboard.GetText();
            Text.Builder.Insert(SelectionIndex, text);
            action.AfterText.Append(text);
            Text.Rebuild_From_Builder();
            //Box.recalcBoundaries();
            MoveX(text.Length);
            Selection = Location;
            action.SaveAfter();
        }
        public void CutSelection()
        {
            if (Text.ReadOnly || SelectionCount == 0) return;
            TextAction action = Actions.Add(this, TextAction.ActionType.Cut_text);
            string text = Text.Builder.ToString(SelectionIndex, SelectionCount);
            action.Text.Append(text);
            Clipboard.SetText(text);
            Text.Builder.Remove(SelectionIndex, SelectionCount);
            Location = GetLocationFromIndex(SelectionIndex);
            Text.Rebuild_From_Builder();
            //Box.recalcBoundaries();
            action.SaveAfter();
        }
        #endregion

        #region "Actions"
        public void Undo(TextAction action)
        {
            Selection = action.AfterSelection;
            Location = action.AfterLocation;
            switch (action.Type)
            {
                case TextAction.ActionType.Remove_Tab:
                case TextAction.ActionType.Cut_text:
                case TextAction.ActionType.Delete_text:
                case TextAction.ActionType.Delete_chars:
                case TextAction.ActionType.Erase_backwards:
                    Text.Builder.Insert(SelectionIndex, action.Text);
                    break;
                case TextAction.ActionType.Move_text:
                    Text.Builder.Remove(SelectionIndex, SelectionCount);
                    break;
                case TextAction.ActionType.Ident_text:
                {
                    GetSelectedLinesRange(out int start_line, out int end_line);
                    int index = GetIndex(start_line);
                    int count;
                    if (end_line == Text.LastLine) count = Text.Builder.Length - index;
                    else count = GetIndex(end_line + 1) - index;
                    Text.Builder.Remove(index, count);
                    Text.Builder.Insert(index, action.Text);
                    break;
                }
                case TextAction.ActionType.Move_text_up:
                {
                    GetSelectedLinesRange(out int start_line, out int end_line);
                    end_line++;
                    var line = Text.Lines[end_line];
                    Text.Lines.RemoveAt(end_line);
                    Text.Lines.Insert(start_line, line);
                    break;
                }
                case TextAction.ActionType.Move_text_down:
                {
                    GetSelectedLinesRange(out int start_line, out int end_line);
                    start_line--;
                    var line = Text.Lines[start_line];
                    Text.Lines.RemoveAt(start_line);
                    Text.Lines.Insert(end_line, line);
                    break;
                }
            }
            Selection = action.Selection;
            Location = action.Location;
            switch (action.Type)
            {
                case TextAction.ActionType.Replace_with_Tab:
                case TextAction.ActionType.Insert_Tab:
                case TextAction.ActionType.Paste__replace_text:
                case TextAction.ActionType.Insert_new_line:
                case TextAction.ActionType.Write_text:
                case TextAction.ActionType.Replace_text:
                    Text.Builder.Remove(SelectionIndex, action.AfterText.Length);
                    Text.Builder.Insert(SelectionIndex, action.Text);
                    break;
                case TextAction.ActionType.Move_text:
                    Text.Builder.Insert(SelectionIndex, action.Text);
                    break;
                case TextAction.ActionType.Paste_text:
                    Text.Builder.Remove(SelectionIndex, action.AfterText.Length);
                    break;
                case TextAction.ActionType.Move_text_up:
                case TextAction.ActionType.Move_text_down:
                    Text.Rebuild_From_Lines();
                    return;
            }
            Text.Rebuild_From_Builder();
            //Box.recalcBoundaries();
            Text.View.AdjustVisibleArea();
        }
        public void Redo(TextAction action)
        {
            Selection = action.Selection;
            Location = action.Location;
            switch (action.Type)
            {
                case TextAction.ActionType.Remove_Tab:
                case TextAction.ActionType.Cut_text:
                case TextAction.ActionType.Delete_text:
                case TextAction.ActionType.Delete_chars:
                case TextAction.ActionType.Erase_backwards:
                case TextAction.ActionType.Move_text:
                    Text.Builder.Remove(SelectionIndex, SelectionCount);
                    break;
                case TextAction.ActionType.Replace_with_Tab:
                case TextAction.ActionType.Insert_Tab:
                case TextAction.ActionType.Paste__replace_text:
                case TextAction.ActionType.Insert_new_line:
                case TextAction.ActionType.Write_text:
                case TextAction.ActionType.Replace_text:
                    Text.Builder.Remove(SelectionIndex, action.Text.Length);
                    Text.Builder.Insert(SelectionIndex, action.AfterText);
                    break;
                case TextAction.ActionType.Paste_text:
                    Text.Builder.Insert(SelectionIndex, action.AfterText);
                    break;
                case TextAction.ActionType.Ident_text:
                {
                    GetSelectedLinesRange(out int start_line, out int end_line);
                    int index = GetIndex(start_line);
                    int count;
                    if (end_line == Text.LastLine) count = Text.Builder.Length - index;
                    else count = GetIndex(end_line + 1) - index;
                    Text.Builder.Remove(index, count);
                    Text.Builder.Insert(index, action.AfterText);
                    break;
                }
                case TextAction.ActionType.Move_text_up:
                {
                    GetSelectedLinesRange(out int start_line, out int end_line);
                    start_line--;
                    var line = Text.Lines[start_line];
                    Text.Lines.RemoveAt(start_line);
                    Text.Lines.Insert(end_line, line);
                    break;
                }
                case TextAction.ActionType.Move_text_down:
                {
                    GetSelectedLinesRange(out int start_line, out int end_line);
                    end_line++;
                    var line = Text.Lines[end_line];
                    Text.Lines.RemoveAt(end_line);
                    Text.Lines.Insert(start_line, line);
                    break;
                }
            }
            Selection = action.AfterSelection;
            Location = action.AfterLocation;
            switch (action.Type)
            {
                case TextAction.ActionType.Move_text:
                    Text.Builder.Insert(SelectionIndex, action.Text);
                    break;
                case TextAction.ActionType.Move_text_up:
                case TextAction.ActionType.Move_text_down:
                    Text.Rebuild_From_Lines();
                    return;
            }
            Text.Rebuild_From_Builder();
            //Box.recalcBoundaries();
            Text.View.AdjustVisibleArea();
        }

        #endregion

    }
}