using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Phases.DrawableObjects
{
    class SuperTransition : Transition
    {

        public SuperTransition(DrawableCollection ownerDraw, Point[] splinePoints, DrawableObject startObject)
            : base(ownerDraw, splinePoints, startObject)
        {

        }

        public SuperTransition(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {
            
        }

        public override Pen DrawPen
        {
            get
            {
                return Pens.Blue;
            }
        }

        private string linkedObject = "";
        [Description("The object that activates this transition."), Category("General"), Browsable(true), TypeConverter(typeof(PropertiesCoverters.LinksObjectsCoverter))]
        public string Links
        {
            get
            {
                return linkedObject;
            }
            set
            {
                linkedObject = value;
            }
        }

        public override string Text
        {
            get
            {
                if (linkedObject == "") return name;
                return linkedObject;
            }
        }

        public override byte[] SerializeRelations()
        {
            //Add base relations
            var data = new List<byte>(base.SerializeRelations());
            //Add link object
            data.AddRange(SerializeRelation(linkedObject));
            return data.ToArray();
        }

        public override bool DeserializeRelations(Dictionary<int, DrawableObject> dictionary, byte[] data, ref int index)
        {
            if (!base.DeserializeRelations(dictionary, data, ref index)) return false;
            return DeserializeRelation(data, ref index, out linkedObject);
        }
    }
}
