using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using Cottle;

namespace Phases.Variables
{
    class CounterFlag : Flag, IIntegerValue, IIndirectOutput, IInternalOutput
    {
        public string[] Operations { get; } = { "Clear", "Set", "Increment", "Decrement", "Minimum", "Maximum" };

        public CounterFlag(string name)
            : base(name)
        {

        }

        private int maximumValue = 10;
        [Description("Default value at start."), Category("Parameters")]
        public int MaximumValue
        {
            get
            {
                return maximumValue;
            }
            set
            {
                if (value > maximumValue)
                {
                    MessageBox.Show("Inconsistent value. The maximum value must be greater than the minimum value.", "Property value error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                if (defaultValue > value) defaultValue = value;
                maximumValue = value;
            }
        }

        private int minimumValue = 0;
        [Description("Default value at start."), Category("Parameters")]
        public int MinimumValue
        {
            get
            {
                return minimumValue;
            }
            set
            {
                if (value > maximumValue)
                {
                    MessageBox.Show("Inconsistent value. The minimum value must be minor than the maximum value.", "Property value error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                if (defaultValue < value) defaultValue = value;
                minimumValue = value;
            }
        }

        private int defaultValue = 0;
        [Description("Default value at start."), Category("Parameters")]
        public int DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                if(value < minimumValue || value > maximumValue)
                {
                    MessageBox.Show("Value out of range.", "Property value error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                defaultValue = value;
            }
        }

        private int value = 0;
        [Browsable(false)]
        public int Value
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
            data.AddRange(Serialization.SerializeParameter(minimumValue));
            data.AddRange(Serialization.SerializeParameter(maximumValue));
            return data.ToArray();
        }

        public override bool Deserialize(byte[] data, ref int index)
        {
            if (!base.Deserialize(data, ref index)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref defaultValue)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref minimumValue)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref maximumValue)) return false;
            return true;
        }

        public Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                {"Name", Name },
                {"Default", Default },
                {"Minimum", MinimumValue },
                {"Maximum", MaximumValue }
            };
        }

        public Value Evaluate(OperationType operation, Value currentValue)
        {
            int value;
            switch (operation)
            {
                case OperationType.Clear:
                    return false;
                case OperationType.Minimum:
                    return MinimumValue;
                case OperationType.Maximum:
                    return MaximumValue;
                case OperationType.Increment:
                    value = Convert.ToInt32(currentValue.AsNumber);
                    if (value < MaximumValue) value++;
                    return value;
                case OperationType.Decrement:
                    value = Convert.ToInt32(currentValue.AsNumber);
                    if (value > MinimumValue) value--;
                    return value;
                default:
                    return currentValue;
            }
        }
        #endregion
    }
}
