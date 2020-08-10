using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualText
{
    class Text
    {
        public TextView View { get; private set; }
        public TextFormatter Formatter { get; private set; }
        public int LineNumbersWidth { get; private set; }
        public List<TextLine> Lines { get; private set; }
        public List<TextLine> DrawLines { get; private set; }
        public StringBuilder Builder { get; private set; }
        public Size Size { get; set; }
        public int LastLine => Lines.Count - 1;
        public int LastDrawLine => DrawLines.Count - 1;
        public int Length { get; private set; }
        public bool ReadOnly { get; set; }
        public Text(TextView view, TextFormatter formatter)
        {
            View = view;
            Formatter = formatter;
            Lines = new List<TextLine>();
            Lines.Add(new TextLine());
            DrawLines = new List<TextLine>();
            Builder = new StringBuilder();
        }
        public event Action TextChanged;
        public void Rebuild_From_Lines()
        {
            Builder = new StringBuilder(ToString());
        }
        public void Rebuild_From_Builder()
        {
            //Formatter.FormatString(Builder.ToString(), this);
            TextChanged();
        }
        public void Build()
        {
            int lineIndex = 0;
            Length = 0;
            foreach(TextLine line in Lines)
            {
                line.LineIndex = lineIndex;
                line.LinePosition = lineIndex;
                Length += line.Chars.Count;
                lineIndex++;
            }
            Rebuild_From_Lines();
        }
        public void BuildUnline()
        {
            int lineIndex = 0;
            Length = 0;
            foreach (TextLine line in Lines)
            {
                if (int.TryParse(new string(line.Chars.TakeWhile(ch => ch.Char != BaseFormat.LinedChar).ToList().ConvertAll(ch => ch.Char).ToArray()), out int lineNumber))
                {
                    line.LineRelated = lineNumber;
                    line.Chars.RemoveRange(0, line.Chars.FindIndex(ch => ch.Char == BaseFormat.LinedChar) + 1);
                }
                else
                {
                    line.LineRelated = -1;
                }
                line.LineIndex = lineIndex;
                line.LinePosition = lineIndex;
                Length += line.Chars.Count;
                lineIndex++;
            }
            Rebuild_From_Lines();
        }
        public Size GetTextBoundaries()
        {
            LineNumbersWidth = Lines.Count.ToString().Length * BaseFormat.FontWidth;
            return new Size(Lines.Max(ln => ln.Chars.Count), Lines.Sum(ln => Math.Abs(ln.LinesBelow) + 1));
        }
        public string GetLinedText()
        {
            StringBuilder linedText = new StringBuilder();
            int index = 0;
            foreach (TextLine line in Lines)
            {
                linedText.Append(index);
                linedText.Append(BaseFormat.LinedChar);
                linedText.Append(line.ToString());
                if (line != Lines.Last()) linedText.Append(BaseFormat.NewLine);
                index++;
            }
            return linedText.ToString();
        }

        internal void Clear()
        {
            Lines.Clear();
            Lines.Add(new TextLine());
            DrawLines = new List<TextLine>();
        }

        internal void Draw(Graphics g, Rectangle textArea, TextCursor cursor)
        {
            int startX = textArea.Left * BaseFormat.FontWidth;
            int endX = textArea.Right * BaseFormat.FontWidth;
            int x = startX + BaseFormat.LeftMargin;
            int y = textArea.Y * BaseFormat.FontHeight;
            int emptyLines = 0;
            foreach (TextLine line in DrawLines.Skip(textArea.Top).Take(textArea.Height))
            {
                if (line != null)
                {
                    if (emptyLines > 0)
                    {
                        DrawEmptyLines(g, new Rectangle(startX, y, endX, BaseFormat.FontHeight * emptyLines));
                        y += BaseFormat.FontHeight * emptyLines;
                        emptyLines = 0;
                    }
                    line.Draw(g, cursor, textArea.Left, textArea.Width, x, ref y);
                    y += BaseFormat.FontHeight;
                }
                else
                {
                    emptyLines++;
                }
            }
            if (emptyLines > 0)
            {
                DrawEmptyLines(g, new Rectangle(startX, y, endX, BaseFormat.FontHeight * emptyLines));
            }
        }

        private void DrawEmptyLines(Graphics g, Rectangle area)
        {
            g.FillRectangle(BaseFormat.EmptySpaceBrush, area);
        }

        public override string ToString()
        {
            return string.Join(BaseFormat.NewLine, Lines.ConvertAll(ln => ln.ToString()));
        }

        public string ToString(int index, int count)
        {
            return ToString().Substring(index, count);
        }
    }
}
