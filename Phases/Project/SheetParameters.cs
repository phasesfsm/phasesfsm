using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using Phases.DrawableObjects;

namespace Phases
{
    abstract class SheetParameters
    {
        //Default values
        public static readonly Size DefaultSize = new Size(1200, 900);
        public static readonly int DefaultZoomValue = 100;
        public static readonly Point DefaultViewPoint = Point.Empty;
        public static readonly GridStyle DefaultGridStyle = GridStyle.Hide;

        public SheetParameters(string sheetName, Size size)
        {
            name = sheetName;
            sheetRectangle = new Rectangle(Util.InvertSign(Util.Middle(size)), size);
        }

        #region "Name"
        protected string name;
        [DisplayName("(Name)"), Description("The object name."), Category("General")]
        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        #endregion

        #region "Sheet size"

        protected Rectangle sheetRectangle;
        public Size Size
        {
            get
            {
                return sheetRectangle.Size;
            }
            set
            {
                sheetRectangle = new Rectangle(Util.InvertSign(Util.Middle(value)), value);
            }
        }

        #endregion

        #region "Zoom"

        protected int zoom = DefaultZoomValue;
        public int ZoomValue
        {
            get
            {
                return zoom;
            }
        }

        #endregion

        #region "View point"

        protected Point viewPoint = DefaultViewPoint;
        public Point ViewPoint
        {
            get
            {
                return viewPoint;
            }
        }

        #endregion

        #region "Grid"

        public enum GridStyle
        {
            Hide,
            Points,
            Squares
        }

        private GridStyle grid = DefaultGridStyle;
        public GridStyle Grid
        {
            get
            {
                return grid;
            }
            set
            {
                grid = value;
            }
        }

        #endregion

        #region "Serialization"

        public virtual byte[] Serialize()
        {
            var data = new List<byte>();

            //Name
            data.Add(Serialization.Token.SheetName);
            data.AddRange(Serialization.SerializeParameter(name));
            //Sheet size
            data.Add(Serialization.Token.SheetSize);
            data.AddRange(Serialization.SerializeParameter(sheetRectangle.Size));
            //Zoom
            data.Add(Serialization.Token.SheetZoom);
            data.AddRange(Serialization.SerializeParameter(zoom));
            //View point
            data.Add(Serialization.Token.SheetViewPoint);
            data.AddRange(Serialization.SerializeParameter(viewPoint));
            //Grid
            data.Add(Serialization.Token.SheetGrid);
            data.AddRange(Serialization.SerializeParameter((byte)Grid));

            return data.ToArray();
        }

        public virtual bool Deserialize(byte[] data, ref int index)
        {
            byte bt = 0;
            //Name
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.SheetName)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref name)) return false;
            
            //Sheet size
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.SheetSize)) return false;
            Size size = Size.Empty;
            if (!Serialization.DeserializeParameter(data, ref index, ref size)) return false;
            sheetRectangle = new Rectangle(Util.InvertSign(Util.Middle(size)), size);
            //Zoom
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.SheetZoom)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref zoom)) return false;
            //View point
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.SheetViewPoint)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref viewPoint)) return false;
            //Grid
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.SheetGrid)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref bt)) return false;
            grid = (GridStyle)bt;

            return true;
        }

        #endregion
    }
}
