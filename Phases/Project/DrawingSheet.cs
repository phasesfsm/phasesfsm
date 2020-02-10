using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Phases.DrawableObjects;
using Phases.Variables;

namespace Phases
{
    abstract class DrawingSheet : SheetParameters, IDisposable
    {
        readonly int crossSize = 20;
        readonly int gridSeparation = 10;

        public PhasesBook OwnerBook;
        public TreeNode sheetTree;
        
        public DrawableCollection Sketch;
        Pen borderPen = new Pen(Color.Black, 0.001f);
        Pen gridPointPen = new Pen(Color.LightGray, 0.001f);
        Pen gridLinePen = new Pen(Color.WhiteSmoke, 0.001f);
        public abstract VariableCollection Variables { get; }
        public abstract List<IGlobal> Globals { get; }

        public override string Name
        {
            set
            {
                OwnerBook.Sheets.FindAll(sh => sh.Name != name).ForEach(sh => sh.Sketch.Objects.FindAll(obj => obj is Nested nested && nested.PointingTo == name).ForEach(obj => ((Nested)obj).PointingTo = value));
                name = value;
                if (sheetTree != null)
                {
                    sheetTree.Name = value;
                    sheetTree.Text = value;
                }
            }
        }
        public override string ToString() => Name;

        public DrawingSheet(PhasesBook ownerBook, string sheetName, Size size, int imageIndex = Constants.ImageIndex.SubSheet)
            : base(sheetName, size)
        {
            OwnerBook = ownerBook;
            sheetTree = new TreeNode(sheetName, imageIndex, imageIndex);
            Sketch = new DrawableCollection(this);
        }

        public void Draw(Graphics g)
        {
            //Draw Sheet
            g.FillRectangle(Brushes.White, sheetRectangle);

            //Draw sheet border
            g.DrawRectangle(borderPen, sheetRectangle);
        }

        public void DrawFeatures(Graphics g, float scale)
        {
            //Draw grid
            if (Grid == SheetParameters.GridStyle.Points) DrawPointsGrid(g, gridSeparation, scale);
            if (Grid == SheetParameters.GridStyle.Squares) DrawLinesGrid(g, gridSeparation, scale);

            //Center cross
            g.DrawLine(Pens.LightGray, -crossSize, 0, crossSize, 0);
            g.DrawLine(Pens.LightGray, 0, -crossSize, 0, crossSize);

            //Over draw sheet border
            g.DrawRectangle(borderPen, sheetRectangle);
        }

        private void spanAdjust(ref int span, float scale)
        {
            if (scale < 0.2) span *= 8;
            else if (scale < 0.5) span *= 4;
            else if (scale < 0.8) span *= 2;
        }

        private void DrawPointsGrid(Graphics g, int span, float scale)
        {
            spanAdjust(ref span, scale);
            for (int i = -sheetRectangle.Left % span; i < sheetRectangle.Width; i += span)
            {
                for (int j = -sheetRectangle.Top % span; j < sheetRectangle.Height; j += span)
                {
                    g.DrawLine(gridPointPen, sheetRectangle.Left + i, sheetRectangle.Top + j, sheetRectangle.Left + i + 1/scale, sheetRectangle.Top + j);
                }
            }
        }

        private void DrawLinesGrid(Graphics g, int span, float scale)
        {
            spanAdjust(ref span, scale);
            for (int i = -sheetRectangle.Left % span; i < sheetRectangle.Width; i += span)
            {
                g.DrawLine(gridLinePen, sheetRectangle.Left + i, sheetRectangle.Top, sheetRectangle.Left + i, sheetRectangle.Bottom);
            }
            for (int j = -sheetRectangle.Top % span; j < sheetRectangle.Height; j += span)
            {
                g.DrawLine(gridLinePen, sheetRectangle.Left, sheetRectangle.Top + j, sheetRectangle.Right, sheetRectangle.Top + j);
            }
        }

        public virtual string NextObjectName(string prefix) => OwnerBook.NextObjectName(prefix);

        public virtual string NextObjectName(string prefix, List<DrawableObject> list) => OwnerBook.NextObjectName(prefix, list);

        public virtual bool ExistsName(string name) => OwnerBook.ExistsName(name);

        #region "Serialization"

        public override byte[] Serialize()
        {
            var data = new List<byte>();
            data.Add(Serialization.Token.StartSheetDefinition);

            switch (this)
            {
                case ModelSheet model:
                    data.Add((byte)SheetTypes.Model);
                    break;
            }

            //Sheet parameters
            data.AddRange(base.Serialize());

            //Serialize draw
            data.AddRange(Sketch.Serialize());

            data.Add(Serialization.Token.EndSheetDefinition);
            return data.ToArray();
        }

        public override bool Deserialize(byte[] data, ref int index)
        {
            if (data.Length < 6) return false;

            if (!base.Deserialize(data, ref index)) return false;
            sheetTree.Text = name;

            //Deserialize draw
            if (!Sketch.Deserialize(data, ref index)) return false;

            return Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndSheetDefinition);
        }

        public enum SheetTypes
        {
            Model,

            // Default
            Global = Serialization.Token.SheetName,
            Error = -1
        }

        public static SheetTypes DeserializeSheetType(byte[] data, ref int index)
        {
            if (data.Length < 7 || !Serialization.Token.Deserialize(data, ref index, Serialization.Token.StartSheetDefinition)) return SheetTypes.Error;
            SheetTypes sheetType = (SheetTypes)data[index];
            if (data[index] != (byte)SheetTypes.Global) index++;
            return sheetType;
        }

        public void Dispose()
        {
            borderPen.Dispose();
            gridPointPen.Dispose();
            gridLinePen.Dispose();
        }

        #endregion
    }
}
