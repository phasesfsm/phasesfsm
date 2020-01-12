using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Cottle;

namespace Phases.Variables
{
    class BooleanFlag : Flag, IBooleanValue, IIndirectOutput, IInternalOutput
    {
        public string[] Operations { get; } = { "Clear", "Set", "Toggle" };

        public BooleanFlag(string name)
            : base(name)
        {

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
            if (!Serialization.DeserializeParameter(data, ref index, out defaultValue)) return false;
            return true;
        }

        public Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                { "Name", Name },
                { "Default", Default }
            };
        }

        public Value Evaluate(OperationType operation, Value currentValue)
        {
            switch (operation)
            {
                case OperationType.Clear:
                    return false;
                case OperationType.Set:
                    return true;
                case OperationType.Toggle:
                    return !currentValue.AsBoolean;
                default:
                    return currentValue;
            }
        }

        #endregion
    }
}
