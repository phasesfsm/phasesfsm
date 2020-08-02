using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualText
{
    internal class TextLine
    {
        internal List<FormattedChar> Chars { get; private set; }
        public int LineIndex { get; set; }
        public int LinePosition { get; set; }
        public int LineRelated { get; set; }
        public int LinesBelow { get; set; }
        public int Width => BaseFormat.FontWidth * Chars.Count;
        public int Height => BaseFormat.FontHeight * (LinesBelow + 1);
        public int Length => Chars.Count;
        public string LineNumber => (LineIndex + 1).ToString();

        public TextLine()
        {
            Chars = new List<FormattedChar>();
        }
        public void AddChar(char ch, TextFormat format) => Chars.Add(new FormattedChar(ch, format));
        public void Reset(int linesBelow)
        {
            Chars.Clear();
            LinesBelow = linesBelow;
        }
        public void AppendLine()
        {
            LinesBelow += 1;
        }
        public void InsertLine()
        {
            LinesBelow -= 1;
        }
        public void Draw(Graphics g, TextCursor cursor, int start, int count, int x, ref int y)
        {
            drawChars(g, cursor, start, count, x, y);
        }
        private void drawChars(Graphics g, TextCursor cursor, int start, int count, int x, int y)
        {
            int charIndex = start;
            foreach (FormattedChar ch in Chars.Skip(start).Take(count))
            {
                ch.Draw(g, x, y);
                charIndex++;
                x += BaseFormat.FontWidth;
            }
            // draw selection
            if (cursor.GetLineSelection(LineIndex, out int st, out int ct))
            {
                if (st < start)
                {
                    ct -= start - st;
                    st = start;
                }
                if (st + ct > start + count)
                {
                    ct = start + count - st;
                }
                x = (st - start + 1) * BaseFormat.FontWidth;
                int w = ct * BaseFormat.FontWidth;
                g.FillRectangle(BaseFormat.SelectionBrush, x, y, w, BaseFormat.FontHeight);
            }
        }
        public override string ToString()
        {
            return new string(Chars.ConvertAll(ch => ch.Char).ToArray());
        }
        public string Substring(int index)
        {
            return ToString().Substring(index);
        }
        public string Substring(int index, int length)
        {
            return ToString().Substring(index, length);
        }
    }
}
