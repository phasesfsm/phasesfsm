using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DualText
{
    public partial class DualTextBox : UserControl
    {
        private TextView leftView, rightView, activeView, lastView;
        public TextFormatter SourceFormat { get; private set; }
        public TextFormatter ResultFormat { get; private set; }
        private TextView ActiveView
        {
            get => activeView;
            set
            {
                if (activeView != null) activeView.IsActive = false;
                activeView = value;
                if (activeView != null) activeView.IsActive = true;
            }
        }

        #region "Control initializations"
        public DualTextBox(IContainer container)
        {
            container.Add(this);

            SourceFormat = new TextFormatter();
            ResultFormat = new TextFormatter();
            leftView = new TextView(SourceFormat);
            leftView.Text.TextChanged += Source_TextChanged;
            rightView = new TextView(ResultFormat);
            rightView.Text.ReadOnly = true;

            // flipped cursor
            Bitmap bitmap = new Bitmap(Cursors.Arrow.Size.Width, Cursors.Arrow.Size.Height * 2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gBmp = Graphics.FromImage(bitmap))
            {
                Cursors.Arrow.Draw(gBmp, new Rectangle(15, 32, bitmap.Size.Width, bitmap.Size.Height));
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                FlippedArrow = new Cursor(bitmap.GetHicon());
            }

            InitializeComponent();
            DoubleBuffered = true;

            leftView.SetScrollsAndLabel(new Rectangle(0, 0, vScrollL.Left, hScrollL.Top), hScrollL, vScrollL, true, leftLabel);
            rightView.SetScrollsAndLabel(new Rectangle(vScrollL.Right, 0, vScrollR.Left - vScrollL.Right, hScrollR.Top), hScrollR, vScrollR, false, rightLabel);
        }
        #endregion

        #region "Resizing control"
        private void leftLabel_Resize(object sender, EventArgs e)
        {
            leftLabel.Left = vScrollL.Right - leftLabel.Width;
            hScrollL.Width = leftLabel.Left;
        }

        private void rightLabel_Resize(object sender, EventArgs e)
        {
            rightLabel.Left = vScrollR.Right - rightLabel.Width;
            hScrollR.Width = rightLabel.Left - hScrollR.Left;
        }

        private void CalcScrollRanges()
        {
            var leftSize = leftView.Text.GetTextBoundaries();
            var rightSize = rightView.Text.GetTextBoundaries();

            var maxSize = new Size(Math.Max(leftSize.Width, rightSize.Width), Math.Max(leftSize.Height, rightSize.Height));
            leftView.Text.Size = maxSize;
            rightView.Text.Size = maxSize;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

#if NETCOREAPP3_1
            //sub-controls location
            hScrollL.Top = Height - hScrollL.Height;
            hScrollR.Top = hScrollL.Top;
            vScrollL.Left = Width / 2 - vScrollL.Width;
            vScrollL.Height = hScrollL.Top;
            vScrollR.Left = Width - vScrollR.Width;
            vScrollR.Height = hScrollL.Top;
            leftLabel.Top = hScrollL.Top + 1;
            rightLabel.Top = hScrollL.Top + 1;
            rightLabel.Left = Width - rightLabel.Width;
#endif
            // Scrolls resizing
            leftLabel.Left = vScrollL.Right - leftLabel.Width;
            hScrollL.Width = leftLabel.Left;
            hScrollR.Left = vScrollL.Right;
            hScrollR.Width = rightLabel.Left - hScrollR.Left;

            // Resize TextViews
            leftView.Resize(new Rectangle(0, 0, vScrollL.Left, hScrollL.Top));
            rightView.Resize(new Rectangle(vScrollL.Right, 0, vScrollR.Left - vScrollL.Right, hScrollR.Top));

            Refresh();
        }
#endregion

#region "Control drawing"
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            leftView.Draw(e.Graphics);
            rightView.Draw(e.Graphics);
        }
#endregion

#region "Text configuration"
        public event Func<string, string> ProcessText;
        public void SetText(string sourceText)
        {
            SourceFormat.FormatString(sourceText, leftView.Text);
            string linedText = leftView.Text.GetLinedText();
            string result;
            if (ProcessText == null)
            {
                result = linedText;
            }
            else
            {
                result = ProcessText(linedText);
            }
            if (string.IsNullOrWhiteSpace(result)) result = "0\t";
            ResultFormat.FormatString(result, rightView.Text);

            leftView.Text.Build();
            rightView.Text.BuildUnline();

            RelateTexts(leftView.Text, rightView.Text);
            CalcScrollRanges();

            OnResize(null);
        }
        private void Source_TextChanged()
        {
            SetText(leftView.Text.Builder.ToString());
        }
        private void RelateTexts(Text input, Text output)
        {
            int oLine = 0;
            foreach (TextLine lineL in input.Lines)
            {
                bool repeat = false;
                foreach (TextLine lineR in output.Lines.Skip(oLine))
                {
                    if (lineR.LineRelated == -1)
                    {
                        lineL.AppendLine();
                        oLine++;
                    }
                    else if (lineL.LineIndex == lineR.LineRelated)
                    {
                        if (oLine < output.Lines.Count - 1) oLine++;
                        if (repeat) lineL.AppendLine();
                        else repeat = true;
                    }
                    else if (lineL.LineIndex > lineR.LineRelated)
                    {
                        if (!repeat)
                        {
                            oLine++;
                            break;
                        }
                        lineL.AppendLine();
                        oLine++;
                    }
                    else 
                    {
                        if (!repeat) lineR.InsertLine();
                        break;
                    }
                }
            }
            int acc = 0;
            foreach (TextLine line in input.Lines)
            {
                line.LinePosition += acc;
                if (line.LinesBelow >= 0)
                {
                    acc += line.LinesBelow;
                    input.DrawLines.Add(line);
                    for (int i = 0; i < line.LinesBelow; i++)
                    {
                        input.DrawLines.Add(null);
                    }
                }
                else
                {
                    line.LinePosition -= line.LinesBelow;
                    acc -= line.LinesBelow;
                    input.DrawLines.Add(line);
                    for (int i = 0; i > line.LinesBelow; i--)
                    {
                        input.DrawLines.Add(null);
                    }
                }
            }
            acc = 0;
            foreach (TextLine line in output.Lines)
            {
                line.LinePosition += acc;
                if (line.LinesBelow >= 0)
                {
                    acc += line.LinesBelow;
                    output.DrawLines.Add(line);
                    for (int i = 0; i < line.LinesBelow; i++)
                    {
                        output.DrawLines.Add(null);
                    }
                }
                else
                {
                    line.LinePosition -= line.LinesBelow;
                    acc -= line.LinesBelow;
                    for (int i = 0; i > line.LinesBelow; i--)
                    {
                        output.DrawLines.Add(null);
                    }
                    output.DrawLines.Add(line);
                }
            }
        }
        public string GetSourceText()
        {
            return leftView.Text.ToString();
        }
#endregion

#region "Scrolling"
        private void vScrollL_ValueChanged(object sender, EventArgs e)
        {
            vScrollR.Value = vScrollL.Value;
        }

        private void hScrollR_ValueChanged(object sender, EventArgs e)
        {
            hScrollL.Value = hScrollR.Value;
        }

        private void hScrollL_ValueChanged(object sender, EventArgs e)
        {
            hScrollR.Value = hScrollL.Value;
            leftView.RecalcTextArea();
            rightView.RecalcTextArea();
            Invalidate();
        }

        private void vScrollR_ValueChanged(object sender, EventArgs e)
        {
            vScrollL.Value = vScrollR.Value;
            leftView.RecalcTextArea();
            rightView.RecalcTextArea();
            Invalidate();
        }
        private void MoveVertScroll(int offset)
        {
            var vert = vScrollR.Value + offset;
            if (vert < 0) vScrollR.Value = 0;
            else if (vert > vScrollR.Maximum - vScrollR.LargeChange) vScrollR.Value = vScrollR.Maximum - vScrollR.LargeChange;
            else vScrollR.Value = vert;
        }
#endregion

#region "Editions and actions"
        [Browsable(false)] public string UndoText => leftView.Cursor.Actions.UndoText;
        public bool Undo()
        {
            bool more = leftView.Cursor.Actions.Undo(out TextCursor cursor);
            ActiveView = leftView;
            return more;
        }
        [Browsable(false)] public string RedoText => leftView.Cursor.Actions.RedoText;
        public bool Redo()
        {
            bool more = leftView.Cursor.Actions.Redo(out TextCursor cursor);
            ActiveView = leftView;
            return more;
        }
#endregion

#region "Keyboard events"
        private void Scrolls_KeyPress(object sender, KeyPressEventArgs e) => OnKeyPress(e);
        private void Scrolls_KeyUp(object sender, KeyEventArgs e) => OnKeyUp(e);
        private void Scrolls_KeyDown(object sender, KeyEventArgs e) => OnKeyDown(e);
        private void Scrolls_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) => OnPreviewKeyDown(e);
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            e.IsInputKey = true;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            e.SuppressKeyPress = true;
            ActiveView.Cursor.Selecting = e.Modifiers.HasFlag(Keys.Shift);
            if (e.Modifiers.HasFlag(Keys.Control))
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        ActiveView.Cursor.MoveWord(-1);
                        break;
                    case Keys.Right:
                        ActiveView.Cursor.MoveWord(+1);
                        break;
                    case Keys.Up:
                        MoveVertScroll(-FontHeight);
                        break;
                    case Keys.Down:
                        MoveVertScroll(+FontHeight);
                        break;
                    case Keys.Home:
                        ActiveView.Cursor.MoveTo(0, 0);
                        break;
                    case Keys.End:
                        ActiveView.Cursor.MoveToEnd();
                        break;
                    case Keys.PageUp:
                        ActiveView.Cursor.MoveTo(ActiveView.Cursor.X, ActiveView.VisibleText.Y);
                        break;
                    case Keys.PageDown:
                        ActiveView.Cursor.MoveTo(ActiveView.Cursor.X, ActiveView.VisibleText.Bottom - 1);
                        break;
                    case Keys.Z:    // Undo
                        Undo();
                        break;
                    case Keys.Y:    // Redo
                        Redo();
                        break;
                    case Keys.C:    // Copy
                        ActiveView.Cursor.CopySelection();
                        break;
                    case Keys.V:    // Paste
                        ActiveView.Cursor.Paste();
                        break;
                    case Keys.X:    // Cut
                        ActiveView.Cursor.CutSelection();
                        break;
                    case Keys.A:    // Select all
                        ActiveView.Cursor.SelectAll();
                        break;
                    default:
                        e.SuppressKeyPress = false;
                        break;
                }
            }
            else if (e.Modifiers.HasFlag(Keys.Alt))
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        ActiveView.Cursor.MoveLineUp();
                        break;
                    case Keys.Down:
                        ActiveView.Cursor.MoveLineDown();
                        break;
                    default:
                        e.SuppressKeyPress = false;
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        ActiveView.Cursor.MoveX(-1);
                        break;
                    case Keys.Right:
                        ActiveView.Cursor.MoveX(+1);
                        break;
                    case Keys.Up:
                        ActiveView.Cursor.MoveY(-1);
                        break;
                    case Keys.Down:
                        ActiveView.Cursor.MoveY(+1);
                        break;
                    case Keys.Home:
                        ActiveView.Cursor.MoveTo(0);
                        break;
                    case Keys.End:
                        ActiveView.Cursor.MoveTo(ActiveView.Cursor.LineEnd);
                        break;
                    case Keys.PageUp:
                        ActiveView.Cursor.MoveY(1 - ActiveView.VisibleText.Height);
                        break;
                    case Keys.PageDown:
                        ActiveView.Cursor.MoveY(ActiveView.VisibleText.Height - 1);
                        break;
                    case Keys.Delete:
                        ActiveView.Cursor.Delete();
                        break;
                    case Keys.Tab:
                        ActiveView.Cursor.Tab(ActiveView.Cursor.Selecting);
                        break;
                    case Keys.Escape:
                        ActiveView.Cursor.Unselect();
                        break;
                    default:
                        e.SuppressKeyPress = false;
                        break;
                }
            }
            ActiveView.Cursor.Selecting = false;
            if (e.SuppressKeyPress) Invalidate();
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (!e.Handled)
            {
                switch (e.KeyChar)
                {
                    case '\b':
                        ActiveView.Cursor.Delete(true);
                        break;
                    case '\r':
                        ActiveView.Cursor.Enter();
                        break;
                    default:
                        ActiveView.Cursor.Insert(e.KeyChar);
                        break;
                }
                Invalidate();
            }
        }

#endregion

#region "Mouse events"

        private Point? downClick = null;
        private Cursor FlippedArrow;

        private void ScrollLabel_MouseEnter(object sender, EventArgs e) => Cursor = Cursors.Arrow;
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Left && ActiveView.TextArea.Contains(e.Location))
            {
                ActiveView.Cursor.SelectWord(ActiveView.GetCursorLocationFromPoint(e.Location));
            }
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            downClick = e.Location;
            ActiveView = downClick.Value.X < vScrollL.Left ? leftView : rightView;
            if (e.Button == MouseButtons.Left && ActiveView.TextArea.Contains(e.Location))
            {
                if (ActiveView.Cursor.IsSelected(ActiveView.GetCursorLocationFromPoint(e.Location)))
                {
                    ActiveView.Cursor.MovingText = ActiveView.GetNextCursorLocationFromPoint(e.Location);
                }
                else
                {
                    ActiveView.Cursor.MoveTo(ActiveView.GetNextCursorLocationFromPoint(e.Location));
                    ActiveView.Cursor.Selecting = true;
                }
                Invalidate();
            }
            else if (e.Button == MouseButtons.Left && ActiveView.SideArea.Contains(e.Location))
            {
                ActiveView.Cursor.StartSideSelection(ActiveView.GetSideCursorLocationFromPoint(e.Location).Y);
                Invalidate();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (ActiveView.Cursor.MovingText != null)
            {
                if (!ActiveView.Cursor.IsSelected(ActiveView.Cursor.MovingText.Value))
                {
                    ActiveView.Cursor.MoveSelectedText(ActiveView.Cursor.MovingText.Value);
                }
                ActiveView.Cursor.MovingText = null;
                Invalidate();
            }
            else if (ActiveView.Cursor.Selecting)
            {
                ActiveView.Cursor.Selecting = false;
                Invalidate();
            }
            else if (ActiveView.Cursor.SideSelecting)
            {
                ActiveView.Cursor.StopSideSelection();
            }
            downClick = null;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (ActiveView.Cursor.MovingText != null)
            {
                Point tomove = ActiveView.Cursor.ValidateLocation(ActiveView.GetNextCursorLocationFromPoint(e.Location));
                if (tomove != ActiveView.Cursor.MovingText)
                {
                    ActiveView.Cursor.MovingText = tomove;
                    ActiveView.AdjustVisibleArea(ActiveView.Cursor.MovingText.Value.X, ActiveView.Cursor.MovingText.Value.Y);
                    Invalidate();
                }
            }
            else if (ActiveView.Cursor.SideSelecting)
            {
                var line = Math.Min(Math.Max(ActiveView.GetNextCursorLocationFromPoint(e.Location).Y, 0), ActiveView.Text.LastLine);
                ActiveView.Cursor.MoveTo(0, ActiveView.GetSideCursorLocationFromPoint(e.Location).Y);
                Invalidate();
            }
            else if (downClick != null && ActiveView.Cursor.Selecting)
            {
                ActiveView.Cursor.MoveTo(ActiveView.GetNextCursorLocationFromPoint(e.Location));
                Invalidate();
            }
            else if (leftView.TextArea.Contains(e.Location))
            {
                if (!leftView.Cursor.Selecting && leftView.Cursor.IsSelected(ActiveView.GetCursorLocationFromPoint(e.Location)))
                {
                    Cursor = Cursors.Arrow;
                }
                else
                {
                    Cursor = Cursors.IBeam;
                }
            }
            else if (rightView.TextArea.Contains(e.Location))
            {
                if (!rightView.Cursor.Selecting && rightView.Cursor.IsSelected(ActiveView.GetCursorLocationFromPoint(e.Location)))
                {
                    Cursor = Cursors.Arrow;
                }
                else
                {
                    Cursor = Cursors.IBeam;
                }
            }
            else if (leftView.SideArea.Contains(e.Location) || rightView.SideArea.Contains(e.Location))
            {
                Cursor = FlippedArrow;
            }
            else if (!ActiveView.Cursor.Selecting)
            {
                Cursor = Cursors.Arrow;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            MoveVertScroll(-e.Delta * FontHeight / 40);
        }

#endregion

#region "Control focus"
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            ActiveView = lastView ?? leftView;
        }
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            lastView = ActiveView;
            ActiveView = null;
        }
#endregion
    }
}
