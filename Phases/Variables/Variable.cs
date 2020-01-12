using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.ComponentModel;
using System.Linq;
using Cottle;

namespace Phases.Variables
{
    public enum OperationType
    {
        None,
        Falling,
        Raising,
        Toggle,
        Increment,
        Decrement,
        Clear,
        Set,
        Minimum,
        Maximum,
        Send,
        Unknown
    }

    abstract class Variable
    {
        public ListViewItem Item;
        public VariableCollection Owner;

        public Variable(string variableName)
        {
            name = variableName;
            Item = new ListViewItem(name, GetImageIndex());
            Item.Tag = this;
        }

        private string name;
        [DisplayName("(Name)"), Description("The object name."), Category("General")]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (!Util.IsValidName(value))
                {
                    MessageBox.Show("Invalid variable name.", "Property value error.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (value != name && Owner.OwnerBook.ExistsName(value))
                {
                    MessageBox.Show("The captures variable name already exists.", "Property value error.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                name = value;
                Item.Text = value;
            }
        }

        public string Default
        {
            get
            {
                switch (this)
                {
                    case IBooleanValue boolVar:
                        return boolVar.DefaultValue ? "1" : "0";
                    case IIntegerValue intVar:
                        return intVar.DefaultValue.ToString();
                }
                return "(0)";
            }
        }
        
        public string GetOperationCode(OperationType operationType)
        {
            switch (this)
            {
                case IBooleanValue boolVar:
                    switch (operationType)
                    {
                        case OperationType.Clear:
                            return name + " = 0";
                        case OperationType.None:
                            return name + " = 1";
                    }
                    break;
                case IIntegerValue intVar:
                    switch (operationType)
                    {
                        case OperationType.Clear:
                            return name + " = 0";
                        case OperationType.Decrement:
                            return name + "--";
                        case OperationType.Increment:
                            return name + "++";
                        case OperationType.Maximum:
                            return name + " = " + intVar.MaximumValue;
                        case OperationType.Minimum:
                            return name + " = " + intVar.MinimumValue;
                    }
                    break;
                case EventOutput eventOutput:
                    return String.Format("{0}_ReceiveEvent(mach, {1})", "nameproj", name);
            }
            throw new Exception("Non handled operation.");
        }

        #region "Serialization"

        public int GetImageIndex()
        {
            if (this is BooleanInput) return VariableCollection.ImageIndex.BooleanInput;
            if (this is EventInput) return VariableCollection.ImageIndex.EventInput;
            if (this is BooleanOutput) return VariableCollection.ImageIndex.BooleanOutput;
            if (this is EventOutput) return VariableCollection.ImageIndex.EventOutput;
            if (this is BooleanFlag) return VariableCollection.ImageIndex.BooleanFlag;
            if (this is CounterFlag) return VariableCollection.ImageIndex.CounterFlag;
            if (this is MessageFlag) return VariableCollection.ImageIndex.MessageFlag;
            throw new Exception("Invalid variable type.");
        }

        private byte GetSerializedType()
        {
            if (this is BooleanInput) return Serialization.Token.BooleanInputVariable;
            if (this is BooleanOutput) return Serialization.Token.BooleanOutputVariable;
            if (this is BooleanFlag) return Serialization.Token.BooleanFlagVariable;
            if (this is EventInput) return Serialization.Token.EventInputVariable;
            if (this is EventOutput) return Serialization.Token.EventOutputVariable;
            if (this is CounterFlag) return Serialization.Token.CounterFlagVariable;
            if (this is MessageFlag) return Serialization.Token.MessageFlagVariable;
            throw new Exception("Invalid variable type.");
        }

        public string GetTypeString()
        {
            if (this is BooleanInput) return "input-bool";
            if (this is EventInput) return "input-event";
            if (this is BooleanOutput) return "ouput-bool";
            if (this is EventOutput) return "output-event";
            if (this is BooleanFlag) return "flag-bool";
            if (this is CounterFlag) return "flag-counter";
            if (this is MessageFlag) return "flag-msg";
            throw new Exception("Invalid variable type.");
        }

        public virtual byte[] Serialize()
        {
            var data = new List<byte>();
            data.Add(GetSerializedType());
            data.AddRange(Serialization.SerializeId(Owner.All.IndexOf(this)));
            data.AddRange(Serialization.SerializeParameter(name));
            return data.ToArray();
        }

        public virtual bool Deserialize(byte[] data, ref int index)
        {
            return true;
        }

        public static bool DeserializeDefinition(byte[] data, ref int index, out Variable variable, out int id)
        {
            variable = null;
            id = 0;
            byte token;
            string name = "";
            if (!Serialization.Token.IsVariable(data, index)) return false;
            token = data[index++];
            if (!Serialization.DeserializeId(data, ref index, ref id)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref name)) return false;
            switch (token)
            {
                case Serialization.Token.BooleanInputVariable:
                    variable = new BooleanInput(name);
                    break;
                case Serialization.Token.EventInputVariable:
                    variable = new EventInput(name);
                    break;
                case Serialization.Token.BooleanOutputVariable:
                    variable = new BooleanOutput(name);
                    break;
                case Serialization.Token.EventOutputVariable:
                    variable = new EventOutput(name);
                    break;
                case Serialization.Token.BooleanFlagVariable:
                    variable = new BooleanFlag(name);
                    break;
                case Serialization.Token.CounterFlagVariable:
                    variable = new CounterFlag(name);
                    break;
                case Serialization.Token.MessageFlagVariable:
                    variable = new MessageFlag(name);
                    break;
                default:
                    throw new Exception("Not recognized type.");
            }
            variable.Deserialize(data, ref index);
            return true;
        }

        #endregion
    }
}
