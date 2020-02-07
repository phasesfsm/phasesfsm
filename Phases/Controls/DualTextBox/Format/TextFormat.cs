using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualText
{
    public class TextFormat
    {
        private Color color;
        private FontStyle style;
        public Font Font { get; private set; }
        public Brush Brush { get; private set; }
        public Color Color { get => color; set { color = value; Brush = new SolidBrush(color); } }
        public FontStyle Style { get => style; set { style = value; Font = new Font(BaseFormat.TextFont, style); } }
        public TextFormat(Color color, FontStyle style)
        {
            Color = color;
            Style = style;
        }
        public TextFormat(TextFormat format)
        {
            Color = format.Color;
            Style = format.Style;
        }
    }
}
