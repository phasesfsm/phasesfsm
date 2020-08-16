using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;

namespace Phases
{
    sealed class Util
    {
        public static int Adjust(int number, int minimum, int maximum)
        {
            return Math.Max(Math.Min(number, maximum), minimum);
        }

        public static Point Adjust(Point point, Point minimum, Point maximum)
        {
            var adj = new Point();
            adj.X = Adjust(point.X, minimum.X, maximum.X);
            adj.Y = Adjust(point.Y, minimum.Y, maximum.Y);
            return adj;
        }

        public static Point Adjust(Point point, Rectangle area)
        {
            var adj = new Point();
            adj.X = Adjust(point.X, area.X, area.X + area.Width);
            adj.Y = Adjust(point.Y, area.Y, area.Y + area.Height);
            return adj;
        }

        public static int Round(float number)
        {
            return (int)Math.Round(number);
        }

        public static Point InvertSign(Point point)
        {
            return new Point(-point.X, -point.Y);
        }

        public static Point Middle(Size size)
        {
            return new Point(size.Width / 2, size.Height / 2);
        }

        public static Point Center(Rectangle rectangle)
        {
            Point point = rectangle.Location;
            point.Offset(rectangle.Width / 2, rectangle.Height / 2);
            return point;
        }

        public static Point Center(Point[] points)
        {
            float x = 0, y = 0;
            for (int i = 0; i < points.Length; i++)
            {
                x += points[i].X;
                y += points[i].Y;
            }
            x /= points.Length;
            y /= points.Length;
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        public static Rectangle GetTextRectangle(Point location, string text, Font font, StringFormat format, int line = 0, int sel = 1)
        {
            Rectangle rect = GetRectangle(location, TextRenderer.MeasureText(text, font));
            if (format.Alignment == StringAlignment.Near) rect.X += rect.Width / 2;
            else if (format.Alignment == StringAlignment.Far) rect.X -= rect.Width / 2;
            if (format.LineAlignment == StringAlignment.Near) rect.Y += rect.Height / 2;
            else if (format.LineAlignment == StringAlignment.Far) rect.Y -= rect.Height / 2;
            if (line == 0) return rect;
            int lines = text.Split(new string[] { Environment.NewLine },StringSplitOptions.None).Length;
            int offset = rect.Height * (line - 1) / lines;
            rect.Y += offset;
            rect.Height = rect.Height * sel / lines;
            return rect;
        }

        public static Rectangle GetRectangle(Point center, Size size)
        {
            return new Rectangle(center.X - size.Width / 2, center.Y - size.Height / 2, size.Width, size.Height);
        }

        public static Rectangle GetRectangle(int x1, int y1, int x2, int y2)
        {
            return new Rectangle(x1, y2, x2 - x1, y2 - y1);
        }

        public static Rectangle GetRectangle(Point startPoint, Point endPoint)
        {
            Rectangle rect = new Rectangle();
            if (endPoint.X >= startPoint.X)
            {
                rect.X = startPoint.X;
                rect.Width = endPoint.X - startPoint.X;
            }
            else
            {
                rect.X = endPoint.X;
                rect.Width = startPoint.X - endPoint.X;
            }
            if (endPoint.Y >= startPoint.Y)
            {
                rect.Y = startPoint.Y;
                rect.Height = endPoint.Y - startPoint.Y;
            }
            else
            {
                rect.Y = endPoint.Y;
                rect.Height = startPoint.Y - endPoint.Y;
            }
            return rect;
        }

        public static void FixRectangle(ref Rectangle rect)
        {
            if (rect.Width < 0)
            {
                rect.X = rect.X + rect.Width;
                rect.Width = -rect.Width;
            }
            if (rect.Height < 0)
            {
                rect.Y = rect.Y + rect.Height;
                rect.Height = -rect.Height;
            }
        }

        public static Rectangle SquareRectangle(Rectangle rect)
        {
            if (rect.Width > rect.Height)
            {
                rect.Y -= (rect.Width - rect.Height) / 2;
                rect.Height = rect.Width;
            }
            else
            {
                rect.X -= (rect.Height - rect.Width) / 2;
                rect.Width = rect.Height;
            }
            return rect;
        }

        public static RectangleF GetRectangleF(PointF center, SizeF size)
        {
            return new RectangleF(center.X - size.Width / 2, center.Y - size.Height / 2, size.Width, size.Height);
        }

        public static Rectangle GetPositiveRectangle(int x1, int y1, int x2, int y2)
        {
            int x, y, w, h;
            x = Math.Min(x1, x2);
            w = Math.Max(x1, x2) - x;
            y = Math.Min(y1, y2);
            h = Math.Max(y1, y2) - y;
            return new Rectangle(x, y, w, h);
        }

        public static Point ScalePoint(Point location, Matrix transform)
        {
            Matrix m = transform.Clone();
            m.Invert();
            Point[] pts = new Point[] { new Point(location.X, location.Y) };
            m.TransformPoints(pts);
            return pts[0];
        }

        public static Point UnscalePoint(Point location, Matrix transform)
        {
            Point[] pts = new Point[] { new Point(location.X, location.Y) };
            transform.TransformPoints(pts);
            return pts[0];
        }

        public static Size ScaleSize(Size size, float scale)
        {
            return new Size(Util.Round(size.Width / scale), Util.Round(size.Height / scale));
        }

        public static Size UnscaleSize(Size size, float scale)
        {
            return new Size(Util.Round(size.Width * scale), Util.Round(size.Height * scale));
        }

        public static Rectangle ScaleRectangle(Rectangle rectangle, Matrix transform)
        {
            return new Rectangle(ScalePoint(rectangle.Location, transform), ScaleSize(rectangle.Size, transform.Elements[0]));
        }

        public static Rectangle UnscaleRectangle(Rectangle rectangle, Matrix transform)
        {
            return new Rectangle(UnscalePoint(rectangle.Location, transform), UnscaleSize(rectangle.Size, transform.Elements[0]));
        }

        public static int Distance(Point point1, Point point2)
        {
            return (int)Math.Round(Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2)));
        }

        public static Point GetOutOffset(Rectangle rectangle, Point point)
        {
            if (rectangle.Contains(point)) return Point.Empty;
            var pt = Point.Empty;
            if (point.X <= rectangle.Left) pt.X = rectangle.Left - point.X;
            else if (point.X >= rectangle.Right) pt.X = rectangle.Right - point.X;
            if (point.Y <= rectangle.Top) pt.Y = rectangle.Top - point.Y;
            else if (point.Y >= rectangle.Bottom) pt.Y = rectangle.Bottom - point.Y;
            return pt;
        }

        public static double GetAngle(Point center, Point position)
        {
            float x = position.X - center.X;
            float y = position.Y - center.Y;
            if (x == 0f)
            {
                if (y >= 0f) return Math.PI / 2;
                else return -Math.PI / 2;
            }
            else
            {
                return (x >= 0f ? 0 : Math.PI) + Math.Atan(y / x);
            }
        }

        public static Point GetPoint(double distance, double angle)
        {
            double x = distance * Math.Cos(angle);
            double y = distance * Math.Sin(angle);
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        public static Point GetPoint(Point center, double distance, double angle)
        {
            Point point = center;
            double x = distance * Math.Cos(angle);
            double y = distance * Math.Sin(angle);
            point.Offset((int)Math.Round(x), (int)Math.Round(y));
            return point;
        }
        
        private static readonly string[] ReservedWords = {
            "public", "private", "friend", "internal", "partial",
            "static", "readonly", "const", "volatile", "unsigned", "signed",
            "class", "struct", "template", "enum", "typedef", "union",
            "long", "int", "short", "char", "byte", "bool", "boolean", "string", "float", "double",
            "if", "else", "switch", "for", "foreach", "do", "while",
            "return", "break", "continue",
            "new", "delete", "malloc",
            "memcpy", "memset", "memcmp",
            "false", "true", "FALSE", "TRUE"
        };

        public static bool IsValidName(string name)
        {
            if (name.Length == 0) return false;
            if (ReservedWords.Contains(name)) return false;
            if ((name[0] < 'A' || name[0] > 'Z') && (name[0] < 'a' || name[0] > 'z')) return false;
            foreach (char ch in name)
            {
                if ((ch < 'A' || ch > 'Z') && (ch < 'a' || ch > 'z') && (ch < '0' || ch > '9') && ch != '_') return false;
            }
            return true;
        }

        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public static string CounterName(string name)
        {
            return name + "_ct";
        }
    }
}
