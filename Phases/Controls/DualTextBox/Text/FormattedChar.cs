using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DualText
{
    internal class FormattedChar
    {
        internal char Char { get; private set; }
        internal TextFormat Format { get; private set; }
        internal FormattedChar(char ch, TextFormat format)
        {
            Format = format;
            Char = ch;
        }
        internal char SpecialChar
        {
            get
            {
                switch (Char)
                {
                    case '\n':
                        return '\u00AC';
                    case '\r':
                        return '\u00B6';
                    case ' ':
                        return '.';
                    case '\t':
                        return '\u00BB';
                    default:
                        return Char;
                }
            }
        }
        internal void Draw(Graphics g, int x, int y)
        {
            g.DrawString(Char.ToString(), Format.Font, Format.Brush, x, y);
        }
    }
}
