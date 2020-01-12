using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Variables
{
    class VariableCollection
    {
        public sealed class ImageIndex
        {
            //Inputs
            public const int BooleanInput = 0;
            public const int EventInput = 1;
            //Outputs
            public const int BooleanOutput = 3;
            public const int EventOutput = 4;
            //Flags
            public const int BooleanFlag = 5;
            public const int CounterFlag = 7;
            public const int MessageFlag = 2;
        }

        private List<Variable> list;
        public PhasesBook OwnerBook { get; private set; }

        public VariableCollection(PhasesBook owner)
        {
            list = new List<Variable>();
            OwnerBook = owner;
        }

        public List<Variable> All
        {
            get
            {
                return list;
            }
        }

        public List<Variable> Inputs
        {
            get
            {
                return list.FindAll(var => var is Input);
            }
        }

        public List<Variable> BooleanInputs
        {
            get
            {
                return list.FindAll(var => var is BooleanInput);
            }
        }

        public List<Variable> EventInputs
        {
            get
            {
                return list.FindAll(var => var is EventInput);
            }
        }

        public List<Variable> Outputs
        {
            get
            {
                return list.FindAll(var => var is Output);
            }
        }

        public List<Variable> BooleanOutputs
        {
            get
            {
                return list.FindAll(var => var is BooleanOutput);
            }
        }

        public List<Variable> EventOutputs
        {
            get
            {
                return list.FindAll(var => var is EventOutput);
            }
        }

        public List<Variable> Flags
        {
            get
            {
                return list.FindAll(var => var is Flag);
            }
        }

        public List<Variable> BooleanFlags
        {
            get
            {
                return list.FindAll(var => var is BooleanFlag);
            }
        }

        public List<Variable> CounterFlags
        {
            get
            {
                return list.FindAll(var => var is CounterFlag);
            }
        }

        public List<Variable> MessageFlags
        {
            get
            {
                return list.FindAll(var => var is MessageFlag);
            }
        }

        public List<IIndirectOutput> IndirectOutputs
        {
            get
            {
                return list.FindAll(var => var is IIndirectOutput).ConvertAll(obj => obj as IIndirectOutput);
            }
        }

        public List<IIndirectInput> IndirectInputs
        {
            get
            {
                return list.FindAll(var => var is IIndirectInput).ConvertAll(obj => obj as IIndirectInput);
            }
        }

        public List<Variable> ConditionalVariables
        {
            get
            {
                return list.FindAll(var => var is IConditional);
            }
        }

        public List<Variable> InternalOutputs
        {
            get
            {
                return list.FindAll(var => var is IInternalOutput);
            }
        }

        public List<IBooleanValue> BooleanDefaults
        {
            get
            {
                return list.FindAll(var => var is BooleanFlag).ConvertAll(var => (IBooleanValue)((BooleanFlag)var));
            }
        }

        public T AddVariable<T>() where T : Variable
        {
            var variable = (T)Activator.CreateInstance(typeof(T), GetNextVariableName(typeof(T).ToString().Split('.').Last()));
            variable.Owner = this;
            list.Add(variable);
            return variable;
        }

        public void AddVariable(Variable variable)
        {
            variable.Owner = this;
            list.Add(variable);
        }

        public void RemoveVariable(Variable variable)
        {
            list.Remove(variable);
        }

        private string GetNextVariableName(string prefix)
        {
            int i = 1;
            while (list.Exists(var => var.Name == prefix + i))
            {
                i++;
            }
            return prefix + i;
        }

        public Flag GetFlag(string name)
        {
            return (Flag)list.FirstOrDefault(var => var is Flag && var.Name == name);
        }

        #region "Serialization"

        public byte[] Serialize()
        {
            var data = new List<byte>();

            //Start of Variables serialization
            data.Add(Serialization.Token.StartBookVariables);

            //Serialize variables definitions
            foreach(Variable variable in list)
            {
                data.AddRange(variable.Serialize());
            }

            //End of Variable serialization
            data.Add(Serialization.Token.EndBookVariables);

            return data.ToArray();
        }

        public bool Deserialize(byte[] data, ref int index)
        {
            Dictionary<int, Variable> variables = new Dictionary<int, Variable>();
            Variable variable;
            int id = 0, rid;
            list = new List<Variable>();

            //Variables definitions
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.StartBookVariables)) return false;

            while (Serialization.Token.IsVariable(data, index))
            {
                if (!Variable.DeserializeDefinition(data, ref index, out variable, out rid)) return false;
                if (rid != id) return false;
                variables.Add(id, variable);
                AddVariable(variable);
                id++;
            }

            return Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndBookVariables);
        }

        #endregion
    }
}
