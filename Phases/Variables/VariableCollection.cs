using Phases.DrawableObjects;
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
            // States
            public const int State = 8;
            public const int SuperState = 9;
            public const int Nested = 10;
        }
        public IMachineModel OwnerMachine { get; private set; }

        public VariableCollection(IMachineModel machine)
        {
            All = new List<Variable>();
            OwnerMachine = machine;
        }

        public List<Variable> All { get; private set; }
        public List<Variable> Inputs => All.FindAll(var => var is Input);
        public List<Variable> BooleanInputs => All.FindAll(var => var is BooleanInput);
        public List<Variable> EventInputs => All.FindAll(var => var is EventInput);
        public List<Variable> Outputs => All.FindAll(var => var is Output);
        public List<Variable> BooleanOutputs => All.FindAll(var => var is BooleanOutput);
        public List<Variable> EventOutputs => All.FindAll(var => var is EventOutput);
        public List<Variable> Flags => All.FindAll(var => var is Flag);
        public List<Variable> BooleanFlags => All.FindAll(var => var is BooleanFlag);
        public List<Variable> CounterFlags => All.FindAll(var => var is CounterFlag);
        public List<Variable> MessageFlags => All.FindAll(var => var is MessageFlag);
        public List<IIndirectOutput> IndirectOutputs => All.FindAll(var => var is IIndirectOutput).ConvertAll(obj => obj as IIndirectOutput);
        public List<IIndirectInput> IndirectInputs => All.FindAll(var => var is IIndirectInput).ConvertAll(obj => obj as IIndirectInput);
        public List<Variable> ConditionalVariables => All.FindAll(var => var is IConditional);
        public List<Variable> InternalOutputs => All.FindAll(var => var is IInternalOutput);
        public List<IBooleanValue> BooleanDefaults => All.FindAll(var => var is BooleanFlag).ConvertAll(var => (IBooleanValue)((BooleanFlag)var));

        public T AddVariable<T>() where T : Variable
        {
            var variable = (T)Activator.CreateInstance(typeof(T), GetNextVariableName(typeof(T).ToString().Split('.').Last()));
            variable.Owner = this;
            All.Add(variable);
            return variable;
        }

        public void AddVariable(Variable variable)
        {
            variable.Owner = this;
            All.Add(variable);
        }

        public void RemoveVariable(Variable variable)
        {
            All.Remove(variable);
        }

        private string GetNextVariableName(string prefix)
        {
            int i = 1;
            while (All.Exists(var => var.Name == prefix + i))
            {
                i++;
            }
            return prefix + i;
        }

        public Flag GetFlag(string name)
        {
            return (Flag)All.FirstOrDefault(var => var is Flag && var.Name == name);
        }

        public static List<string> GetIndirectInputsList(DrawingSheet sheet)
        {
            List<string> list;

            if (sheet is ModelSheet model)
            {
                list = model.Variables.IndirectInputs.ConvertAll(var => var.Name);
                foreach (Nested nested in model.Sketch.Nesteds)
                {
                    if (nested.PointedSheet == null) continue;
                    nested.PointedSheet.Variables.Outputs.ForEach(var => list.Add(string.Format("{0}.{1}", nested.Name, var.Name)));
                }
            }
            else
            {
                list = sheet.OwnerBook.Variables.IndirectInputs.ConvertAll(var => var.Name);
                foreach (GlobalSheet gsheet in sheet.OwnerBook.GlobalSheets)
                {
                    foreach (Nested nested in gsheet.Sketch.Nesteds)
                    {
                        if (nested.PointedSheet == null) continue;
                        nested.PointedSheet.Variables.Outputs.ForEach(var => list.Add(string.Format("{0}.{1}", nested.Name, var.Name)));
                    }
                }
            }
            return list;
        }

        public static Variable GetIndirectInput(DrawingSheet sheet, string name)
        {
            if (name.Contains("."))
            {
                if (sheet is ModelSheet model)
                {
                    foreach (Nested nested in model.Sketch.Nesteds)
                    {
                        if (nested.PointedSheet == null) continue;
                        foreach (Variable var in nested.PointedSheet.Variables.Outputs)
                        {
                            if (string.Format("{0}.{1}", nested.Name, var.Name) == name) return var;
                        }
                    }
                }
                else
                {
                    foreach (GlobalSheet gsheet in sheet.OwnerBook.GlobalSheets)
                    {
                        foreach (Nested nested in gsheet.Sketch.Nesteds)
                        {
                            if (nested.PointedSheet == null) continue;
                            foreach (Variable var in nested.PointedSheet.Variables.Outputs)
                            {
                                if (string.Format("{0}.{1}", nested.Name, var.Name) == name) return var;
                            }
                        }
                    }
                }
            }
            else
            {
                if (sheet.Variables.IndirectInputs.Exists(obj => obj.Name == name))
                {
                    return sheet.Variables.All.FindAll(var => var is IIndirectInput).FirstOrDefault(obj => obj.Name == name);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public static List<string> GetIndirectOutputsList(DrawingSheet sheet)
        {
            List<string> list;

            if (sheet is ModelSheet model)
            {
                list = model.Variables.IndirectOutputs.ConvertAll(var => var.Name);
                foreach (Nested nested in model.Sketch.Nesteds)
                {
                    if (nested.PointedSheet == null) continue;
                    nested.PointedSheet.Variables.Inputs.ForEach(var => list.Add(string.Format("{0}.{1}", nested.Name, var.Name)));
                }
            }
            else
            {
                list = sheet.OwnerBook.Variables.IndirectOutputs.ConvertAll(var => var.Name);
                foreach (GlobalSheet gsheet in sheet.OwnerBook.GlobalSheets)
                {
                    foreach (Nested nested in gsheet.Sketch.Nesteds)
                    {
                        if (nested.PointedSheet == null) continue;
                        nested.PointedSheet.Variables.Inputs.ForEach(var => list.Add(string.Format("{0}.{1}", nested.Name, var.Name)));
                    }
                }
            }
            return list;
        }

        public static Variable GetIndirectOutput(DrawingSheet sheet, string name)
        {
            if (name.Contains("."))
            {
                if (sheet is ModelSheet model)
                {
                    foreach (Nested nested in model.Sketch.Nesteds)
                    {
                        if (nested.PointedSheet == null) continue;
                        foreach (Variable var in nested.PointedSheet.Variables.Inputs)
                        {
                            if (string.Format("{0}.{1}", nested.Name, var.Name) == name) return var;
                        }
                    }
                }
                else
                {
                    foreach (GlobalSheet gsheet in sheet.OwnerBook.GlobalSheets)
                    {
                        foreach (Nested nested in gsheet.Sketch.Nesteds)
                        {
                            if (nested.PointedSheet == null) continue;
                            foreach (Variable var in nested.PointedSheet.Variables.Inputs)
                            {
                                if (string.Format("{0}.{1}", nested.Name, var.Name) == name) return var;
                            }
                        }
                    }
                }
            }
            else
            {
                if (sheet.Variables.IndirectOutputs.Exists(obj => obj.Name == name))
                {
                    return sheet.Variables.All.FindAll(var => var is IIndirectOutput).FirstOrDefault(obj => obj.Name == name);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public static string[] GetIndirectOutputOperations(DrawingSheet sheet, string name)
        {
            if (name.Contains("."))
            {
                if (sheet is ModelSheet model)
                {
                    foreach (Nested nested in model.Sketch.Nesteds)
                    {
                        if (nested.PointedSheet == null) continue;
                        foreach(Variable var in nested.PointedSheet.Variables.Inputs)
                        {
                            if (string.Format("{0}.{1}", nested.Name, var.Name) == name)
                            {
                                if (var is BooleanInput) return new string[]{ "Clear", "Set", "Toggle" };
                                else if(var is EventInput) return new string[] { "Send" };
                            }
                        }
                    }
                }
                else
                {
                    foreach (GlobalSheet gsheet in sheet.OwnerBook.GlobalSheets)
                    {
                        foreach (Nested nested in gsheet.Sketch.Nesteds)
                        {
                            if (nested.PointedSheet == null) continue;
                            foreach (Variable var in nested.PointedSheet.Variables.Inputs)
                            {
                                if (string.Format("{0}.{1}", nested.Name, var.Name) == name)
                                {
                                    if (var is BooleanInput) return new string[] { "Clear", "Set", "Toggle" };
                                    else if (var is EventInput) return new string[] { "Send" };
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (sheet.Variables.IndirectOutputs.Exists(obj => obj.Name == name))
                {
                    return sheet.Variables.IndirectOutputs.First(obj => obj.Name == name).Operations;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public static Dictionary<string, int> GetConditionDictionary(DrawingSheet sheet)
        {
            Dictionary<string, int> dictionary;

            if (sheet is ModelSheet model)
            {
                dictionary = model.Variables.ConditionalVariables.ToDictionary(var => var.Name, var => var.GetImageIndex());
                foreach (Nested nested in model.Sketch.Nesteds)
                {
                    if (nested.PointedSheet == null) continue;
                    nested.PointedSheet.Variables.Outputs.ForEach(var => dictionary.Add(string.Format("{0}.{1}", nested.Name, var.Name), var.GetImageIndex()));
                }
            }
            else
            {
                dictionary = sheet.OwnerBook.Variables.ConditionalVariables.ToDictionary(var => var.Name, var => var.GetImageIndex());
                foreach (GlobalSheet gsheet in sheet.OwnerBook.GlobalSheets)
                {
                    foreach (State state in gsheet.Sketch.States)
                    {
                        if (state is SuperState)
                        {
                            dictionary.Add(state.Name, ImageIndex.SuperState);
                        }
                        else if (state is Nested)
                        {
                            dictionary.Add(state.Name, ImageIndex.Nested);
                        }
                        else
                        {
                            dictionary.Add(state.Name, ImageIndex.State);
                        }
                    }
                    foreach (Nested nested in gsheet.Sketch.Nesteds)
                    {
                        if (nested.PointedSheet == null) continue;
                        nested.PointedSheet.Variables.Outputs.ForEach(var => dictionary.Add(string.Format("{0}.{1}", nested.Name, var.Name), var.GetImageIndex()));
                        foreach (State state in nested.PointedSheet.Sketch.States)
                        {
                            if (state is SuperState)
                            {
                                dictionary.Add(string.Format("{0}.{1}", nested.Name, state.Name), ImageIndex.SuperState);
                            }
                            else if (state is Nested)
                            {
                                dictionary.Add(string.Format("{0}.{1}", nested.Name, state.Name), ImageIndex.Nested);
                            }
                            else
                            {
                                dictionary.Add(string.Format("{0}.{1}", nested.Name, state.Name), ImageIndex.State);
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        public static Dictionary<string, int> GetOutputsDictionary(DrawingSheet sheet)
        {
            Dictionary<string, int> dictionary;

            if (sheet is ModelSheet model)
            {
                dictionary = model.Variables.InternalOutputs.ToDictionary(var => var.Name, var => var.GetImageIndex());
                foreach (Nested nested in model.Sketch.Nesteds)
                {
                    if (nested.PointedSheet == null) continue;
                    nested.PointedSheet.Variables.Inputs.ForEach(var => dictionary.Add(string.Format("{0}.{1}", nested.Name, var.Name), var.GetImageIndex()));
                }
            }
            else
            {
                dictionary = sheet.OwnerBook.Variables.InternalOutputs.ToDictionary(var => var.Name, var => var.GetImageIndex());
                foreach (GlobalSheet gsheet in sheet.OwnerBook.GlobalSheets)
                {
                    foreach (Nested nested in gsheet.Sketch.Nesteds)
                    {
                        nested.PointedSheet?.Variables.Inputs.ForEach(var => dictionary.Add(string.Format("{0}.{1}", nested.Name, var.Name), var.GetImageIndex()));
                    }
                }
            }
            return dictionary;
        }

        #region "Serialization"

        public byte[] Serialize()
        {
            var data = new List<byte>();

            //Start of Variables serialization
            data.Add(Serialization.Token.StartBookVariables);

            //Serialize variables definitions
            foreach(Variable variable in All)
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
            All = new List<Variable>();

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
