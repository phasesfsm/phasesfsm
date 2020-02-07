using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualText
{
    public static class BaseFormat
    {
        #region "Special charecteres"
        public const string NewLine = "\r\n";
        public const int TabSpaces = 4;
        public const int TabWidth = 4;
        public const char LinedChar = '\t';
        #endregion

        #region "Common Text attributes"
        public const string FontName = "Consolas"; //"Courier New";
        public static readonly Font TextFont = new Font(FontName, 10f);
        public static readonly TextFormat TextFormat = new TextFormat(Color.Black, FontStyle.Regular);
        public static readonly Brush SelectionBrush = new SolidBrush(Color.FromArgb(50, Color.SteelBlue));
        #endregion

        #region "Other text attibutes"
        public static readonly Pen CursorPen = new Pen(Color.Black, 2f);
        public static readonly Brush EmptySpaceBrush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.Gainsboro, Color.Transparent);
        #endregion

        #region "Visual attibutes"
        public static readonly Color ForeColor = Color.Black;
        public static readonly Color BackgroundColor = Color.White;
        public static readonly FontStyle Style = FontStyle.Regular;
        public const int FontWidth = 7;
        public const int FontHeight = 16;
        public const int LeftMargin = 5;
        #endregion

        public static class LineNumbers
        {
            public const int LeftMargin = 10;
            public const int RightMargin = 1;
            public static readonly Font Font = new Font(FontName, 9f, FontStyle.Bold);
            public static readonly Color ForeColor = Color.DimGray;
            public static readonly Color BackgroundColor = Color.Gainsboro;
            public static readonly StringFormat Format = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        }
    }
}
