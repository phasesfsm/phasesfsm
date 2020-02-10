using Phases.DrawableObjects;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases
{
    class ModelSheet : DrawingSheet, IMachineModel
    {
        public override VariableCollection Variables { get; }
        public override List<IGlobal> Globals { get; }
        internal ModelSheet(PhasesBook ownerBook, string sheetName, Size size)
            : base(ownerBook, sheetName, size, Constants.ImageIndex.ModelSheet)
        {
            Variables = new VariableCollection(this);
            Globals = new List<IGlobal>();
        }
        public override string NextObjectName(string prefix)
        {
            int i = 1;
            while (Sketch.Objects.Exists(obj => obj.Name == prefix + i))
            {
                i++;
            }
            return prefix + i;
        }

        public override string NextObjectName(string prefix, List<DrawableObject> list)
        {
            int i = 1;
            while (Sketch.Objects.Exists(obj => obj.Name == prefix + i) || list.Exists(obj => obj.Name == prefix + i))
            {
                i++;
            }
            return prefix + i;
        }

        public override bool ExistsName(string name)
        {
            if (Variables.All.Exists(obj => obj.Name == name)) return true;
            if (Sketch.Objects.Exists(obj => obj.Name == name)) return true;
            return false;
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>(base.Serialize());

            //Serialize book variables
            data.AddRange(Variables.Serialize());

            //Serialize global objects priorities
            data.Add(Serialization.Token.StartGlobalPriorityList);
            foreach (IGlobal ig in Globals)
            {
                data.Add(Serialization.Token.ObjectName);
                data.AddRange(Serialization.SerializeParameter(ig.Name));
            }
            data.Add(Serialization.Token.EndGlobalPriorityList);

            return data.ToArray();
        }

        public override bool Deserialize(byte[] data, ref int index)
        {
            if (!base.Deserialize(data, ref index)) return false;

            //Deserialize variables
            if (!Variables.Deserialize(data, ref index)) return false;

            //Global objects priorities
            if (Serialization.Token.Deserialize(data, ref index, Serialization.Token.StartGlobalPriorityList))
            {
                string name = "";
                while (Serialization.Token.Deserialize(data, ref index, Serialization.Token.ObjectName))
                {
                    Serialization.DeserializeParameter(data, ref index, out name);
                    if (!Sketch.Objects.Exists(obj => obj.Name == name)) return false;
                    var gobj = Sketch.Objects.Find(obj => obj.Name == name) as IGlobal;
                    if (!Globals.Contains(gobj))
                    {
                        Globals.Add(gobj);
                    }
                }
                if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndGlobalPriorityList)) return false;
            }
            return true;
        }
    }
}
