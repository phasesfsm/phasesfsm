using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Phases.Variables
{
    class FlipFlopFlag : Flag, IBooleanValue
    {
        public FlipFlopFlag(string name)
            : base(name)
        {

        }

        private TriggerInput trigger;
        [Description("The trigger used to activate this flag."), Category("Usage"), Browsable(true), TypeConverter(typeof(Phases.PropertiesCoverters.TriggersList))]
        public string Trigger
        {
            get
            {
                if (trigger == null) return Name;
                return trigger.Name;
            }
            set
            {
                SetTrigger(ref trigger, OperationType.Toggle, value);
            }
        }

        private bool defaultValue = false;
        [Description("Default value at start."), Category("Parameters")]
        public bool DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                defaultValue = value;
            }
        }

        private bool value = false;
        [Browsable(false)]
        public bool Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        #region "Serialization"

        public override byte[] Serialize()
        {
            var data = new List<byte>(base.Serialize());
            data.AddRange(Serialization.SerializeParameter(defaultValue));
            return data.ToArray();
        }

        public override bool Deserialize(byte[] data, ref int index)
        {
            if (!base.Deserialize(data, ref index)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref defaultValue)) return false;
            return true;
        }

        #endregion
    }
}
