using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DualText
{
    class TextView
    {
        public HScrollBar HScroll { get; private set; }
        public VScrollBar VScroll { get; private set; }
        public Label Label { get; private set; }
        public Rectangle VisibleText { get; private set; }    // Characters rectangle
        public Rectangle ToDrawText; // { get; private set; }    // Characters rectangle
        public Rectangle Area;          // Graphic rectangle
        public Point ReferencePoint;
        public Rectangle TextArea { get; private set; }         // Graphic rectangle
        public Rectangle SideArea { get; private set; }      // Graphic rectangle
        public Text Text { get; set; }
        public TextCursor Cursor { get; set; }
        public bool IsActive { get; set; }
        public TextView(TextFormatter formatter)
        {
            Text = new Text(this, formatter);
            Cursor = new TextCursor(Text, new ActionsManager());
        }

        #region "Text view area"
        public void Draw(Graphics g)
        {
            //g.DrawLine(new Pen(Color.Red), Area.X + SideArea.Width, SideArea.Top, Area.X + SideArea.Width, SideArea.Bottom);
            
            // draw line numbers rectangle
            g.FillRectangle(new SolidBrush(BaseFormat.LineNumbers.BackgroundColor), SideArea);
            g.SetClip(SideArea);

            g.TranslateTransform(SideArea.X, ReferencePoint.Y);
            int y = ToDrawText.Top * BaseFormat.FontHeight;
            // draw line numbers
            foreach (TextLine line in Text.DrawLines.Skip(ToDrawText.Top).Take(ToDrawText.Height))
            {
                if (line != null)
                {
                    g.DrawString(line.LineNumber, BaseFormat.LineNumbers.Font, new SolidBrush(BaseFormat.LineNumbers.ForeColor), new RectangleF(0, y, Text.LineNumbersWidth + BaseFormat.LineNumbers.LeftMargin, BaseFormat.FontHeight), BaseFormat.LineNumbers.Format);
                }
                y += BaseFormat.FontHeight;
            }
            g.ResetTransform();

            // delimite text area
            //g.DrawRectangle(Pens.Red, TextArea);
            g.SetClip(TextArea);

            // Translate transform to text zone
            g.TranslateTransform(TextArea.X + ReferencePoint.X, TextArea.Y + ReferencePoint.Y);

            // Draw text
            Text.Draw(g, ToDrawText, Cursor);

            // Draw Cursor
            if (IsActive) Cursor.Draw(g);

            g.ResetClip();
            g.ResetTransform();
        }
        public void RecalcTextArea()
        {
            SideArea = new Rectangle(Area.Location, new Size(Text.LineNumbersWidth + BaseFormat.LineNumbers.LeftMargin + BaseFormat.LineNumbers.RightMargin, Area.Height));
            TextArea = new Rectangle(Area.X + SideArea.Width, Area.Y, Area.Width - SideArea.Width, Area.Height);

            if (HScroll == null || VScroll == null) return;
            ReferencePoint = new Point(-HScroll.Value, -VScroll.Value);
            
            // Calc text area
            int rx = (int)Math.Floor((double)HScroll.Value / BaseFormat.FontWidth);
            int ry = (int)Math.Ceiling((double)VScroll.Value / BaseFormat.FontHeight);
            int rw = (int)Math.Ceiling(((double)HScroll.Value + TextArea.Width - rx * BaseFormat.FontWidth) / BaseFormat.FontWidth);
            int rh = (int)Math.Ceiling(((double)VScroll.Value + TextArea.Height - ry * BaseFormat.FontHeight) / BaseFormat.FontHeight);
            VisibleText = new Rectangle(rx, ry, rw, rh);

            if (rx > 0) { rx -= 1; rw += 2; }
            else rw++;
            if (ry > 0) { ry -= 1; rh += 2; }
            else rh++;
            if (rh > Text.DrawLines.Count - ry) rh = Text.DrawLines.Count - ry;
            ToDrawText = new Rectangle(rx, ry, rw, rh);
        }
        public void Resize(Rectangle area)
        {
            Area = area;

            RecalcTextArea();
            RecalcScrolls();
        }
        #endregion

        #region "Scrolls and Label"
        public void SetScrollsAndLabel(Rectangle area, HScrollBar hScroll, VScrollBar vScroll, bool smallChange, Label label)
        {
            Area = area;
            HScroll = hScroll;
            VScroll = vScroll;
            Label = label;
            ReferencePoint = new Point(-HScroll.Value, -VScroll.Value);
            if (!smallChange)
            {
                HScroll.SmallChange = BaseFormat.FontHeight;
                VScroll.SmallChange = BaseFormat.FontHeight;
            }
        }
        public void RecalcScrolls()
        {
            if (HScroll == null || VScroll == null) return;

            HScroll.Maximum = Math.Max(0, Text.Size.Width * BaseFormat.FontWidth) + TextArea.Width / 2;
            HScroll.LargeChange = Math.Max(0, TextArea.Width);
            HScroll.Enabled = Text.Size.Width * BaseFormat.FontWidth >= TextArea.Width;

            VScroll.Maximum = Math.Max(0, Text.Size.Height * BaseFormat.FontHeight);
            VScroll.LargeChange = Math.Max(0, TextArea.Height);
            VScroll.Enabled = Text.Size.Height * BaseFormat.FontHeight >= TextArea.Height;
        }

        public void AdjustVisibleArea()
        {
            VScroll.SuspendLayout();
            HScroll.SuspendLayout();
            Label.SuspendLayout();
            AdjustVisibleArea(Cursor.Selection.X, Cursor.GetRealY(Cursor.Selection.Y));
            AdjustVisibleArea(Cursor.Location.X, Cursor.GetRealY(Cursor.Location.Y));
            UpdateLabel(Cursor.Location.Y + 1, Math.Min(Text.Lines[Cursor.Location.Y].Length, Cursor.Location.X) + 1);
            VScroll.ResumeLayout();
            VScroll.ResumeLayout();
            HScroll.ResumeLayout();
            Label.ResumeLayout();
        }
        public void AdjustVisibleArea(int x, int y)
        {
            // Vertical adjust
            if (VScroll.Value < y * BaseFormat.FontHeight - TextArea.Height + BaseFormat.FontHeight + 1)
            {
                // Going down
                VScroll.Value = y * BaseFormat.FontHeight - TextArea.Height + BaseFormat.FontHeight + 1;
            }
            else if (VScroll.Value > y * BaseFormat.FontHeight)
            {
                // Going up
                VScroll.Value = y * BaseFormat.FontHeight;
            }
            else if (Cursor.SideSelecting && VScroll.Value >= y * BaseFormat.FontHeight)
            {
                VScroll.Value = Math.Max(0, y - 1) * BaseFormat.FontHeight;
            }
            // Horizontal adjust
            if (x - VisibleText.X >= VisibleText.Width)
            {
                // Going right
                HScroll.Value = x * BaseFormat.FontWidth - TextArea.Width + (int)BaseFormat.CursorPen.Width + BaseFormat.FontWidth * 3;
            }
            else if (HScroll.Value > x * BaseFormat.FontWidth)
            {
                // Going left
                HScroll.Value = Math.Max(x * BaseFormat.FontWidth, 0);
            }
        }
        internal void UpdateLabel(int line, int column)
        {
            if (Label == null) return;
            //label.Text = string.Format("    Ln: {0}    Ch: {1}        ", Y + 1, Math.Min(LineEnd, X) + 1);
            Label.Text = string.Format("    Ln: {0}  Ch: {1}    ", line, column);
        }
        #endregion

        #region "Info functions"
        public Point GetNextCursorLocationFromPoint(Point point)
        {
            int column = (ReferencePoint.X + point.X - TextArea.X + BaseFormat.FontWidth / 2 - BaseFormat.LeftMargin) / BaseFormat.FontWidth;
            int line = (point.Y - ReferencePoint.Y) / BaseFormat.FontHeight;
            return new Point(column, GetNearestLine(line));
        }
        public Point GetSideCursorLocationFromPoint(Point point)
        {
            int column = (ReferencePoint.X + point.X - TextArea.X + BaseFormat.FontWidth / 2 - BaseFormat.LeftMargin) / BaseFormat.FontWidth;
            int line = (point.Y - ReferencePoint.Y + BaseFormat.FontHeight / 4) / BaseFormat.FontHeight;
            return new Point(column, GetNearestLine(line));
        }
        public Point GetCursorLocationFromPoint(Point point)
        {
            int column = (int)Math.Floor((double)(ReferencePoint.X + point.X - TextArea.X - BaseFormat.LeftMargin) / BaseFormat.FontWidth);
            int line = (point.Y - ReferencePoint.Y) / BaseFormat.FontHeight;
            return new Point(column, GetNearestLine(line));
        }
        public int GetNearestLine(int line)
        {
            if (line > Text.DrawLines.Count - 1) line = Text.DrawLines.Count - 1;
            while (line >= 0)
            {
                if (Text.DrawLines[line] != null)
                    return Text.DrawLines[line].LineIndex;
                line--;
            }
            return 0;
        }
        #endregion
    }
}
