using Phases.DrawableObjects;
using Phases.Importers.StateCad;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Importers
{
    class StateCadImporter
    {
        public List<Instruction> Instructions { get; private set; }
        public float Scale { get; private set; }
        public Matrix Transform { get; private set; }
        public Rectangle DrawArea { get; private set; }
        public string Text { get; private set; }
        public DrawableCollection OwnerDraw;

        public StateCadImporter(string text, DrawableCollection ownerDraw, float scale)
        {
            Text = text;
            OwnerDraw = ownerDraw;
            Scale = scale;
            string[] lines = text.Split(new string[] { "\x02\x0A" }, StringSplitOptions.RemoveEmptyEntries);
            Instructions = new List<Instruction>();

            Rectangle rect = Rectangle.Empty;
            DrawArea = Rectangle.Empty;
            foreach (string line in lines)
            {
                Instruction inst = new Instruction(line, this);
                Instructions.Add(inst);
                if (inst.GetDrawRectangle(out rect))
                {
                    DrawArea = GetDrawRectangle(DrawArea, rect);
                }
            }

            var sheet = new Rectangle(Util.InvertSign(Util.Middle(DrawArea.Size)), DrawArea.Size);
            Transform = new Matrix();
            Transform.Translate(DrawArea.X - sheet.X, -sheet.Y + DrawArea.Y);
            Transform.Scale(Scale, -Scale);
        }

        private Rectangle GetDrawRectangle(Rectangle rect1, Rectangle rect2)
        {
            int x1, x2, y1, y2;
            if (rect1 == Rectangle.Empty) return rect2;
            if (rect2 == Rectangle.Empty) return rect1;
            x1 = Math.Min(rect1.Left, rect2.Left);
            x2 = Math.Max(rect1.Right, rect2.Right);
            y1 = Math.Min(rect1.Top, rect2.Top);
            y2 = Math.Max(rect1.Bottom, rect2.Bottom);
            return Util.GetPositiveRectangle(x1, y1, x2, y2);
        }
    }
}
