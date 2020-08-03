using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Linq;

namespace Phases.DrawableObjects
{
    class Alias : Link
    {
        public static readonly int smallRadio = 12;
        public static readonly int radio = 15;
        public static readonly int selectionRadio = 16;
        public override int Radio { get { return radio; } }
        public override int SelectionRadio { get { return selectionRadio; } }

        public Alias(DrawableCollection ownerDraw, Point location)
            : base(ownerDraw, location)
        {
            this.location = location;
        }

        public Alias(DrawableCollection ownerDraw, string _name, string _description)
            : base(ownerDraw, _name, _description)
        {

        }

        private DrawableObject pointing;
        [Description("The object being pointed by this alias."), Category("General"), Browsable(true), TypeConverter(typeof(Phases.PropertiesCoverters.ObjectsListConverter))]
        public string PointingTo
        {
            get
            {
                if (pointing == null) return null;
                return pointing.Name;
            }
            set
            {
                if (pointing != null)
                {
                    outTransitions.RemoveAll(trans => trans.StartObject == this);
                }
                pointing = OwnerDraw.Objects.Find(obj => obj.Name == value);
                if (pointing != null)
                {
                    outTransitions.AddRange(base.outTransitions);
                }
            }
        }

        internal override List<Transition> outTransitions => pointing == null ? base.outTransitions : pointing.outTransitions;
        internal List<Transition> AliasOutTransitions => base.outTransitions;
#if DEBUG
        public Transition[] aliasOutTransitions => base.outTransitions.ToArray();
#endif
        [Browsable(false)]
        public State Pointing => pointing as State;

        protected override void DrawForm(Graphics g, DrawAttributes att)
        {
            Pen pen2 = new Pen(att.Pen.Color, 2f);
            g.FillEllipse(Brushes.White, location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
            g.DrawEllipse(pen2, location.X - Radio, location.Y - Radio, Radio * 2, Radio * 2);
            g.DrawEllipse(att.Pen, location.X - smallRadio, location.Y - smallRadio, smallRadio * 2, smallRadio * 2);
        }

        public override string Text
        {
            get
            {
                if (pointing == null) return name;
                else return pointing.Text;
            }
        }

        public override void CopyTo(DrawableObject obj)
        {
            base.CopyTo(obj);
            Alias alias = (Alias)obj;
            if (pointing != null) alias.pointing = OwnerDraw.Objects.Find(obj => obj.Name == pointing.Name);
        }

        public virtual byte[] SerializeRelations()
        {
            var data = new List<byte>();
            //Add relations
            data.AddRange(SerializeObjectId());
            data.AddRange(SerializeRelation(OwnerDraw.Objects.IndexOf(pointing)));
            return data.ToArray();
        }

        public virtual bool DeserializeRelations(Dictionary<int, DrawableObject> dictionary, byte[] data, ref int index)
        {
            DrawableObject stateRef;
            if (DeserializeRelation(dictionary, data, ref index, out stateRef)) pointing = stateRef;
            return true;
        }
    }
}
