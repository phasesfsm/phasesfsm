using Phases.BasicObjects;
using Phases.DrawableObjects;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Phases.CodeGeneration
{
    class CodeGeneratorProperties
    {
        #region "Sources"

        [Browsable(false)]
        internal string MainIniFile { get; set; } = "";

        [Browsable(false)]
        internal string LocalIniFile { get; set; } = "";

        #endregion

        public CodeGeneratorProperties()
        {

        }

        /// <summary>
        /// Generates a new instance of CodeGeneratorProperties.
        /// </summary>
        /// <param name="iniFile">Main ini file (cottle.ini) that is on the code generator template folder.</param>
        /// <param name="localIniFile">Secondary ini file (project.ini) that is in the same folder as phases file.
        /// the settings in this file will overwrite main ini settings.</param>
        public CodeGeneratorProperties(string iniFile, string localIniFile = "")
        {
            List<string> lines = new List<string>(File.ReadAllLines(iniFile));

            if (localIniFile != "")
            {
                lines.AddRange(File.ReadAllLines(localIniFile));
            }

            // Declaring and intializing object of Type 
            Type objType = typeof(CodeGeneratorProperties);

            // using GetProperties() Method 
            PropertyInfo[] type = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (string line in lines)
            {
                if (line.Contains("=") && !line.StartsWith("#"))
                {
                    int equalIndex = line.IndexOf('=');
                    string key = line.Substring(0, equalIndex).Trim();
                    string value = line.Substring(equalIndex + 1).Trim();
                    object obj;

                    if (value.StartsWith("\"")) // String
                    {
                        if (value.EndsWith("\""))
                        {
                            obj = value.Substring(1, value.Length - 2);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (value.StartsWith("'")) // Char
                    {
                        if (value.EndsWith("'") && value.Length == 3)
                        {
                            obj = value[1];
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (value.ToLower() == "true") // bool true
                    {
                        obj = true;
                    }
                    else if (value.ToLower() == "false") // bool false
                    {
                        obj = false;
                    }
                    else if (int.TryParse(value, out int number)) // integer
                    {
                        obj = number;
                    }
                    else
                    {
                        obj = value;
                        //continue;
                    }
                    foreach (PropertyInfo propertyInfo in type)
                    {
                        if (propertyInfo.CanWrite && propertyInfo.Name == key && propertyInfo.PropertyType == obj.GetType())
                        {
                            object firstValue = propertyInfo.GetValue(this, null);
                            propertyInfo.SetValue(this, obj);
                            break;
                        }
                    }
                }
            }
        }

        #region Macros

        [Flags]
        public enum ContextLevel
        {
            NoContext = 0x00,
            Directory = 0x100,
            File = 0x200,
            Project = 0x01,
            Machine = 0x02,
            Node = 0x04,
            SuperState = 0x08,
            State = 0x10,
            Transition = 0x20,
            EnterOutputs = 0x40,
            ExitOutputs = 0x80,
            Variable = 0x400,
            Pointing = 0x800,

            // Ignore line
            Ignore = 0x8000,

            // Combos
            All = File | Project | Machine | Node | SuperState | State | Transition
                | EnterOutputs | ExitOutputs | Directory | Pointing | Ignore,
            NoDirName = All - Directory,
            NoFileName = All - File - Directory,
            NoOutputs = NoFileName - EnterOutputs - ExitOutputs,
            NoCondition = NoFileName - Transition,
            States = SuperState | State | EnterOutputs | ExitOutputs,
        }

        public enum MacroRender
        {
            AsIs,
            LowerCase,
            UpperCase,
        }

        public class ContextObjects
        {
            public GeneratorData Data;
            public BasicObjectsTree Machine;
            public BasicMachine SuperState;
            public BasicState State;
            public BasicTransition Transition;
            public Variable Variable;
            public DrawableObject Pointing;

            public ContextObjects(GeneratorData data)
            {
                Data = data;
            }

            public ContextObjects(ContextObjects objects)
            {
                Data = objects.Data;
                Machine = objects.Machine;
                SuperState = objects.SuperState;
                State = objects.State;
                Transition = objects.Transition;
                Variable = objects.Variable;
            }
        }

        public class RenderingContext
        {
            public ContextLevel Level { get; set; }
            public ContextObjects Objects { get; }
            public string Value { get; }
            public bool First { get; set; }
            public bool Last { get; set; }
            public int Index { get; set; }
            public string File { get; }

            public RenderingContext(string file, ContextLevel level, GeneratorData data, string value)
            {
                File = file;
                Level = level;
                Objects = new ContextObjects(data);
                Value = value;
            }

            public RenderingContext(string file, ContextLevel level, ContextObjects objects, string value)
            {
                File = file;
                Level = level;
                Objects = objects;
                Value = value;
            }

            public RenderingContext(string file, string value, RenderingContext context)
            {
                File = file;
                Value = value;
                Level = context.Level;
                Objects = new ContextObjects(context.Objects);
            }

            public RenderingContext(RenderingContext context, string newValue = "", ContextLevel newLevel = ContextLevel.NoContext, string file = null)
            {
                if (newLevel == ContextLevel.NoContext)
                    Level = context.Level;
                else
                    Level = context.Level | newLevel;
                if (newValue == "")
                    Value = context.Value;
                else
                    Value = newValue;
                Objects = new ContextObjects(context.Objects);
                File = string.IsNullOrWhiteSpace(file) ? context.File : file;
            }
        }

        public struct MacroToken
        {
            public ContextLevel Context { get; }
            public string Name { get; }
            public MacroRender Render { get; set; }

            public MacroToken(string name, ContextLevel context)
            {
                Name = name;
                Context = context;
                Render = MacroRender.AsIs;
            }

            public static readonly MacroToken Empty = new MacroToken("", ContextLevel.NoContext);
        }

        public sealed class MacroTokens
        {
            public static readonly MacroToken FileName = new MacroToken("FileName", ContextLevel.NoFileName);
            public static readonly MacroToken FileExt = new MacroToken("FileExt", ContextLevel.NoFileName);
            public static readonly MacroToken File = new MacroToken("File", ContextLevel.NoFileName);

            public static readonly MacroToken Project = new MacroToken("Project", ContextLevel.All);
            public static readonly MacroToken Machines = new MacroToken("Machines", ContextLevel.NoDirName);
            public static readonly MacroToken Machine = new MacroToken("Machine", ContextLevel.NoDirName);
            public static readonly MacroToken Nodes = new MacroToken("Nodes", ContextLevel.NoFileName);
            public static readonly MacroToken Node = new MacroToken("Node", ContextLevel.NoFileName);
            public static readonly MacroToken SuperStates = new MacroToken("SuperStates", ContextLevel.NoFileName);
            public static readonly MacroToken SuperState = new MacroToken("SuperState", ContextLevel.NoFileName);
            public static readonly MacroToken States = new MacroToken("States", ContextLevel.NoFileName);
            public static readonly MacroToken State = new MacroToken("State", ContextLevel.NoFileName);
            public static readonly MacroToken Transitions = new MacroToken("Transitions", ContextLevel.NoOutputs);
            public static readonly MacroToken Transition = new MacroToken("Transition", ContextLevel.NoOutputs);
            public static readonly MacroToken Pointing = new MacroToken("Pointing:", ContextLevel.NoOutputs);
            public static readonly MacroToken Abort = new MacroToken("Abort", ContextLevel.NoOutputs);
            public static readonly MacroToken End = new MacroToken("End", ContextLevel.NoOutputs);

            public static readonly MacroToken Variables = new MacroToken("Variables", ContextLevel.NoFileName);
            public static readonly MacroToken Variable = new MacroToken("Variable", ContextLevel.NoFileName);
            public static readonly MacroToken BoolInputs = new MacroToken("BoolInputs", ContextLevel.NoOutputs);
            public static readonly MacroToken BoolInput = new MacroToken("BoolInput", ContextLevel.NoOutputs);
            public static readonly MacroToken BoolOutputs = new MacroToken("BoolOutputs", ContextLevel.NoFileName);
            public static readonly MacroToken BoolOutput = new MacroToken("BoolOutput", ContextLevel.NoFileName);
            public static readonly MacroToken BoolFlags = new MacroToken("BoolFlags", ContextLevel.NoFileName);
            public static readonly MacroToken BoolFlag = new MacroToken("BoolFlag", ContextLevel.NoFileName);
            public static readonly MacroToken InputEvents = new MacroToken("InputEvents", ContextLevel.NoOutputs);
            public static readonly MacroToken InputEvent = new MacroToken("InputEvent", ContextLevel.NoOutputs);
            public static readonly MacroToken OutputEvents = new MacroToken("OutputEvents", ContextLevel.NoCondition);
            public static readonly MacroToken OutputEvent = new MacroToken("OutputEvent", ContextLevel.NoCondition);
            public static readonly MacroToken CounterFlags = new MacroToken("CounterFlags", ContextLevel.NoFileName);
            public static readonly MacroToken CounterFlag = new MacroToken("CounterFlag", ContextLevel.NoFileName);
            public static readonly MacroToken MessageFlags = new MacroToken("MessageFlags", ContextLevel.NoFileName);
            public static readonly MacroToken MessageFlag = new MacroToken("MessageFlag", ContextLevel.NoFileName);

            public static readonly MacroToken Outputs = new MacroToken("Outputs", ContextLevel.Transition);
            public static readonly MacroToken Events = new MacroToken("Events", ContextLevel.Transition);
            public static readonly MacroToken Messages = new MacroToken("Messages", ContextLevel.Transition);
            public static readonly MacroToken EnterOutputs = new MacroToken("EnterOutputs", ContextLevel.States);
            public static readonly MacroToken EnterEvents = new MacroToken("EnterEvents", ContextLevel.States);
            public static readonly MacroToken EnterMessages = new MacroToken("EnterMessages", ContextLevel.States);
            public static readonly MacroToken ExitOutputs = new MacroToken("ExitOutputs", ContextLevel.States);
            public static readonly MacroToken ExitEvents = new MacroToken("ExitEvents", ContextLevel.States);
            public static readonly MacroToken ExitMessages = new MacroToken("ExitMessages", ContextLevel.States);

            public static readonly MacroToken Default = new MacroToken("Default", ContextLevel.Variable);
            public static readonly MacroToken Description = new MacroToken("Description", ContextLevel.Variable);

            public static readonly MacroToken Condition = new MacroToken("Condition(", ContextLevel.Transition);
            public static readonly MacroToken ConditionT = new MacroToken("Condition", ContextLevel.Transition);

            // Macro functions tokens
            public static readonly MacroToken Index = new MacroToken("Index", ContextLevel.All);
            public static readonly MacroToken Count = new MacroToken("Count", ContextLevel.All);
            public static readonly MacroToken NoFirst = new MacroToken("nf(", ContextLevel.All);
            public static readonly MacroToken NoLast = new MacroToken("nl(", ContextLevel.All);
            public static readonly MacroToken OnlyFirst = new MacroToken("of(", ContextLevel.All);
            public static readonly MacroToken OnlyLast = new MacroToken("ol(", ContextLevel.All);
        }
        #endregion

        public bool ContainsToken(string inputText, ContextLevel context, out MacroToken token)
        {
            // Declaring and intializing object of Type 
            Type objType = typeof(MacroTokens);

            // using GetProperties() Method 
            FieldInfo[] type = objType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            foreach (FieldInfo fieldInfo in type)
            {
                if (!fieldInfo.IsLiteral && fieldInfo.IsInitOnly)
                {
                    MacroToken reference = (MacroToken)fieldInfo.GetValue(null);
                    if ((context & reference.Context) > ContextLevel.NoContext && inputText.ToLower().Contains(MacroBegin + reference.Name.ToLower() + MacroEnd))
                    {
                        token = reference;
                        string macro = MacroBegin + fieldInfo.Name + MacroEnd;
                        if (inputText.Contains(macro.ToLower()))
                        {
                            token.Render = MacroRender.LowerCase;
                        }
                        else if (inputText.Contains(macro.ToUpper()))
                        {
                            token.Render = MacroRender.UpperCase;
                        }
                        else
                        {
                            token.Render = MacroRender.AsIs;
                        }
                        return true;
                    }
                }
            }
            token = MacroToken.Empty;
            return false;
        }

        public MacroToken GetBlockToken(string inputText, ContextLevel context)
        {
            MacroToken token;

            // Declaring and intializing object of Type 
            Type objType = typeof(MacroTokens);

            // using GetProperties() Method 
            FieldInfo[] type = objType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            foreach (FieldInfo fieldInfo in type)
            {
                if (!fieldInfo.IsLiteral && fieldInfo.IsInitOnly)
                {
                    MacroToken reference = (MacroToken)fieldInfo.GetValue(null);
                    if (reference.Context.HasFlag(context) && inputText.ToLower().Contains(MacroBlockBegin + fieldInfo.Name.ToLower() + MacroEnd))
                    {
                        token = reference;
                        string macro = MacroBegin + fieldInfo.Name + MacroEnd;
                        if (inputText.Contains(macro.ToLower()))
                        {
                            token.Render = MacroRender.LowerCase;
                        }
                        else if (inputText.Contains(macro.ToUpper()))
                        {
                            token.Render = MacroRender.UpperCase;
                        }
                        else
                        {
                            token.Render = MacroRender.AsIs;
                        }
                        return token;
                    }
                }
            }
            return MacroToken.Empty;
        }

        public string RenderMacro(string inputText, MacroToken token, string value, ContextLevel valueContextLevel, RenderingContext currentContext, out RenderingContext newContext)
        {
            newContext = null;
            int macroIndex = inputText.ToLower().IndexOf(MacroBegin + token.Name.ToLower() + MacroEnd);
            if (macroIndex == -1) return inputText;
            string macro, result;
            // Force context
            if (inputText.Length >= macroIndex + MacroBegin.Length + token.Name.Length + MacroEnd.Length + MacroContext.Length && inputText.Substring(macroIndex + MacroBegin.Length + token.Name.Length + MacroEnd.Length, MacroContext.Length) == MacroContext)
            {
                macro = inputText.Substring(macroIndex, MacroBegin.Length + token.Name.Length + MacroEnd.Length + MacroContext.Length);
                value = MacroBegin;
                result = value;
                newContext = new RenderingContext(currentContext, value);
                newContext.Level = valueContextLevel;
            }
            else
            {
                macro = inputText.Substring(macroIndex, MacroBegin.Length + token.Name.Length + MacroEnd.Length);

                if (token.Render == MacroRender.LowerCase)
                    value = value.ToLower();
                else if (token.Render == MacroRender.UpperCase)
                    value = value.ToUpper();
                result = inputText.Replace(macro, value);
                newContext = new RenderingContext(currentContext, result, valueContextLevel);
            }
            return result;
        }

        public List<RenderingContext> RenderMacroDirectory(string inputText, RenderingContext context)
        {
            List<RenderingContext> result = new List<RenderingContext>();
            ContextLevel level = context.Level;
            string origText = "";

            while (origText != inputText && ContainsToken(inputText, context.Level, out MacroToken token))
            {
                origText = inputText;
                switch (token.Name)
                {
                    case "Project":
                        if (context.Objects.Data == null)
                        {
                            inputText = RenderMacro(inputText, token, token.Name, ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        else
                        {
                            inputText = RenderMacro(inputText, token, context.Objects.Data.Profile.ProjectName, ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        break;
                    case "Machine":
                        switch (level)
                        {
                            case ContextLevel.Project:
                                if (context.Objects.Data == null)
                                {
                                    RenderMacro(inputText, token, token.Name, ContextLevel.Machine, context, out RenderingContext newContext);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (BasicObjectsTree machine in context.Objects.Data.Trees)
                                    {
                                        RenderMacro(inputText, token, machine.Name, ContextLevel.Machine, context, out RenderingContext newContext);
                                        newContext.Objects.Machine = machine;
                                        result.Add(newContext);
                                    }
                                }
                                level = ContextLevel.Machine;
                                break;
                            case ContextLevel.Machine:
                            {
                                RenderMacro(inputText, token, context.Objects.Machine.Name, ContextLevel.Machine, context, out RenderingContext newContext);
                                result.Add(newContext);
                                break;
                            }
                        }
                        inputText = result.First().Value;
                        break;
                    case "State":
                        if (level.HasFlag(ContextLevel.State))
                        {

                        }
                        else if (level.HasFlag(ContextLevel.Machine))
                        {
                            foreach (BasicObjectsTree machine in context.Objects.Data.Trees)
                            {
                                foreach (BasicState state in machine.States)
                                {
                                    RenderMacro(inputText, token, state.Name, ContextLevel.State, context, out RenderingContext newContext);
                                    newContext.Objects.Machine = machine;
                                    newContext.Objects.State = state;
                                    result.Add(newContext);
                                }
                            }
                        }
                        else if (level.HasFlag(ContextLevel.Project))
                        {

                        }
                        inputText = result.First().Value;
                        break;
                }
            }
            if (result.Count == 0)
            {
                RenderingContext newContext = new RenderingContext(context, inputText, ContextLevel.NoContext, Path.Combine(context.File, inputText));
                result.Add(newContext);
            }
            return result;
        }

        public List<RenderingContext> RenderMacroFile(string inputText, List<RenderingContext> contexts)
        {
            List<RenderingContext> result = new List<RenderingContext>();
            ContextLevel level = contexts.First().Level;
            string origText = "";

            while (origText != inputText && ContainsToken(inputText, contexts.First().Level, out MacroToken token))
            {
                if (result.Count > 0)
                {
                    contexts = result;
                    result = new List<RenderingContext>();
                }
                origText = inputText;
                switch (token.Name)
                {
                    case "Project":
                        if (contexts.First().Objects.Data == null)
                        {
                            inputText = RenderMacro(inputText, token, token.Name, ContextLevel.Project, contexts.First(), out RenderingContext newContext);
                        }
                        else
                        {
                            inputText = RenderMacro(inputText, token, contexts.First().Objects.Data.Profile.ProjectName, ContextLevel.Project, contexts.First(), out RenderingContext newContext);
                        }
                        break;
                    case "Machine":
                        if (level.HasFlag(ContextLevel.Machine))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.Machine, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (RenderingContext context in contexts)
                                {
                                    RenderMacro(inputText, token, context.Objects.Machine.Name, ContextLevel.Machine, contexts.First(), out RenderingContext newContext);
                                    result.Add(newContext);
                                }
                            }
                        }
                        else if (level.HasFlag(ContextLevel.Project))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.Machine, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (BasicObjectsTree machine in contexts.First().Objects.Data.Trees)
                                {
                                    foreach (RenderingContext context in contexts)
                                    {
                                        RenderMacro(inputText, token, machine.Name, ContextLevel.Machine, contexts.First(), out RenderingContext newContext);
                                        newContext.Objects.Machine = machine;
                                        result.Add(newContext);
                                    }
                                }
                            }
                            level = ContextLevel.Machine;
                        }
                        inputText = result.First().Value;
                        break;
                    case "Node":
                        if (level.HasFlag(ContextLevel.Machine))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.Node, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (RenderingContext context in contexts)
                                {
                                    foreach (BasicState state in context.Objects.Machine.BasicStatesList)
                                    {
                                        RenderMacro(inputText, token, state.Name, ContextLevel.Node, contexts.First(), out RenderingContext newContext);
                                        newContext.Objects.State = state;
                                        result.Add(newContext);
                                    }
                                }
                            }
                        }
                        else if (level.HasFlag(ContextLevel.Project))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.Node, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (BasicState state in contexts.First().Objects.Data.BasicStatesList)
                                {
                                    RenderMacro(inputText, token, state.Name, ContextLevel.Node, contexts.First(), out RenderingContext newContext);
                                    newContext.Objects.State = state;
                                    result.Add(newContext);
                                }
                            }
                            level = ContextLevel.Machine;
                        }
                        inputText = result.First().Value;
                        break;
                    case "SuperState":
                        if (level.HasFlag(ContextLevel.Machine))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.SuperState, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (RenderingContext context in contexts)
                                {
                                    foreach (BasicState state in context.Objects.Machine.SuperStatesList())
                                    {
                                        RenderMacro(inputText, token, state.Name, ContextLevel.SuperState, contexts.First(), out RenderingContext newContext);
                                        newContext.Objects.State = state;
                                        result.Add(newContext);
                                    }
                                }
                            }
                        }
                        else if (level.HasFlag(ContextLevel.Project))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.SuperState, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (BasicState state in contexts.First().Objects.Data.SuperStatesList())
                                {
                                    RenderMacro(inputText, token, state.Name, ContextLevel.SuperState, contexts.First(), out RenderingContext newContext);
                                    newContext.Objects.State = state;
                                    result.Add(newContext);
                                }
                            }
                            level = ContextLevel.Machine;
                        }
                        inputText = result.First().Value;
                        break;
                    case "State":
                        if (level.HasFlag(ContextLevel.State))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.State, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (RenderingContext context in contexts)
                                {
                                    foreach (BasicState state in context.Objects.Machine.StatesList())
                                    {
                                        RenderMacro(inputText, token, state.Name, ContextLevel.State, contexts.First(), out RenderingContext newContext);
                                        newContext.Objects.State = state;
                                        result.Add(newContext);
                                    }
                                }
                            }
                        }
                        else if (level.HasFlag(ContextLevel.Machine))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.State, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (RenderingContext context in contexts)
                                {
                                    foreach (BasicState state in context.Objects.Machine.StatesList())
                                    {
                                        RenderMacro(inputText, token, state.Name, ContextLevel.State, contexts.First(), out RenderingContext newContext);
                                        newContext.Objects.State = state;
                                        result.Add(newContext);
                                    }
                                }
                            }
                            level = ContextLevel.State;
                        }
                        else if (level.HasFlag(ContextLevel.Project))
                        {
                            if (contexts.First().Objects.Data == null)
                            {
                                RenderMacro(inputText, token, token.Name, ContextLevel.State, contexts.First(), out RenderingContext newContext);
                                result.Add(newContext);
                            }
                            else
                            {
                                foreach (BasicState state in contexts.First().Objects.Data.StatesList())
                                {
                                    RenderMacro(inputText, token, state.Name, ContextLevel.State, contexts.First(), out RenderingContext newContext);
                                    newContext.Objects.State = state;
                                    result.Add(newContext);
                                }
                            }
                            level = ContextLevel.Machine;
                        }
                        inputText = result.First().Value;
                        break;
                }
            }
            if (result.Count == 0)
            {
                if (contexts.First().Level == ContextLevel.Machine)
                {
                    foreach (RenderingContext context in contexts)
                    {
                        RenderingContext newContext = new RenderingContext(context, inputText, ContextLevel.NoContext, Path.Combine(contexts.First().File, inputText));
                        result.Add(newContext);
                    }
                }
                else
                {
                    RenderingContext newContext = new RenderingContext(contexts.First(), inputText, ContextLevel.NoContext, Path.Combine(contexts.First().File, inputText));
                    result.Add(newContext);
                }
            }
            return result;
        }

        public string RenderMacroLine(string inputText, MacroToken token, string value)
        {
            string macro = inputText.Substring(inputText.IndexOf(MacroBegin), MacroBegin.Length + token.Name.Length + MacroEnd.Length);

            if (token.Render == MacroRender.LowerCase)
                value = value.ToLower();
            else if (token.Render == MacroRender.UpperCase)
                value = value.ToUpper();

            return inputText.Replace(macro, value);
        }

        public string RenderMacroDocument(string documentText, RenderingContext context)
        {
            string[] lines = documentText.Split(new string[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None);
            int lineIndex = 0;
            StringBuilder text = RenderMacroBlock(context, lines, ref lineIndex);

            // Add the rest of the lines at the end: this means there was an error
            while (lineIndex < lines.Length)
            {
                text.AppendLine(lines[lineIndex]);
                lineIndex++;
            }
            text.Remove(text.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            return text.ToString();
        }

        /// <summary>
        /// Render with the macro processor a block of text.
        /// </summary>
        /// <param name="context">Current context for the block.</param>
        /// <param name="lines">Full document lines.</param>
        /// <param name="lineIndex">Index where the block starts, at return is the index where the block ends.</param>
        /// <returns>The resulting text from the block rendering.</returns>
        public StringBuilder RenderMacroBlock(RenderingContext context, string[] lines, ref int lineIndex)
        {
            StringBuilder text = new StringBuilder();

            while (lineIndex < lines.Length)
            {
                string line = lines[lineIndex];
                if (line.Contains(MacroBlockEnd))
                {
                    break;
                }
                else if (line.Contains(MacroBlockBegin))
                {
                    MacroToken token = GetBlockToken(line, context.Level);
                    int blockIndex = lineIndex + 1;
                    switch (token.Name)
                    {
                        case "Machine":
                            if (context.Level.HasFlag(ContextLevel.Machine))
                            {
                                text.Append(RenderMacroBlock(context, lines, ref blockIndex));
                            }
                            else
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Machine,
                                    context.Objects.Data.Trees, (ctx, machine) => ctx.Objects.Machine = machine);
                            }
                            break;
                        case "State":
                            if (context.Level.HasFlag(ContextLevel.State))
                            {
                                text.Append(RenderMacroBlock(context, lines, ref blockIndex));
                            }
                            else if (context.Level.HasFlag(ContextLevel.SuperState))
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.State,
                                    context.Objects.Data.BasicStatesList, (ctx, state) => ctx.Objects.State = state);
                            }
                            else if (context.Level.HasFlag(ContextLevel.Machine))
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.State,
                                    context.Objects.Machine.States, (ctx, state) => ctx.Objects.State = state);
                            }
                            else
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.State,
                                    context.Objects.Data.BasicStatesList, (ctx, state) => ctx.Objects.State = state);
                            }
                            break;
                        case "Transition":
                            if (context.Level.HasFlag(ContextLevel.Transition))
                            {
                                text.Append(RenderMacroBlock(context, lines, ref blockIndex));
                            }
                            else if (context.Level.HasFlag(ContextLevel.State))
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Transition,
                                    context.Objects.State.Transitions, (ctx, trans) => ctx.Objects.Transition = trans);
                            }
                            else if (context.Level.HasFlag(ContextLevel.SuperState))
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Transition,
                                    context.Objects.SuperState.Transitions, (ctx, trans) => ctx.Objects.Transition = trans);
                            }
                            else if (context.Level.HasFlag(ContextLevel.Machine))
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Transition,
                                    context.Objects.Machine.BasicTransitionsList, (ctx, trans) => ctx.Objects.Transition = trans);
                            }
                            else
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Transition,
                                    context.Objects.Data.BasicTransitionsList(), (ctx, trans) => ctx.Objects.Transition = trans);
                            }
                            break;
                        case "Variable":
                            if (context.Level.HasFlag(ContextLevel.Variable))
                            {
                                text.Append(RenderMacroBlock(context, lines, ref blockIndex));
                            }
                            else if (context.Level.HasFlag(ContextLevel.Project))
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Variable,
                                    context.Objects.Data.Variables.All, (ctx, var) => ctx.Objects.Variable = var);
                            }
                            break;
                        case "InputEvent":
                            if (context.Level.HasFlag(ContextLevel.Variable))
                            {
                                text.Append(RenderMacroBlock(context, lines, ref blockIndex));
                            }
                            else if (context.Level.HasFlag(ContextLevel.Project))
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Variable,
                                    context.Objects.Data.Variables.EventInputs, (ctx, var) => ctx.Objects.Variable = var);
                            }
                            break;
                        case "OutputEvent":
                            if (context.Level.HasFlag(ContextLevel.Variable))
                            {
                                text.Append(RenderMacroBlock(context, lines, ref blockIndex));
                            }
                            else if (context.Level.HasFlag(ContextLevel.Project))
                            {
                                RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Variable,
                                    context.Objects.Data.Variables.EventOutputs, (ctx, var) => ctx.Objects.Variable = var);
                            }
                            break;
                        case "Pointing:":
                            if (context.Level.HasFlag(ContextLevel.Transition))
                            {
                                int tokenIndex = line.IndexOf(MacroBlockBegin + token.Name);
                                if (tokenIndex < 0) break;
                                int afterTokenIndex = tokenIndex + MacroBlockBegin.Length + token.Name.Length;
                                int spaceIndex = line.IndexOf(' ', afterTokenIndex);
                                if (spaceIndex < 0) spaceIndex = line.Length;
                                string objType = line.Substring(afterTokenIndex, spaceIndex - afterTokenIndex);

                                if (context.Objects.Transition.Transition.EndObject.GetType().ToString().EndsWith(objType))
                                {
                                    if (context.Level.HasFlag(ContextLevel.Ignore)) context.Level -= ContextLevel.Ignore;
                                    if (context.Level.HasFlag(ContextLevel.Pointing))
                                    {
                                        context.Objects.Pointing = context.Objects.Transition.Transition.EndObject;
                                        RenderMacroBlock(context, lines, ref lineIndex);
                                    }
                                    else
                                    {
                                        RenderGenericBlock(text, context, lines, ref blockIndex, ContextLevel.Pointing,
                                            new DrawableObject[] { context.Objects.Transition.Transition.EndObject }, (ctx, obj) => ctx.Objects.Pointing = obj);
                                    }
                                }
                                else
                                {
                                    context.Objects.Pointing = null;
                                    context.Level |= ContextLevel.Ignore;
                                }
                            }
                            break;
                        default:
                            text.Append(RenderMacroBlock(context, lines, ref blockIndex));
                            break;
                    }
                    lineIndex = blockIndex;
                }
                else
                {
                    if (!context.Level.HasFlag(ContextLevel.Ignore)) text.Append(RenderMacroLine(context, line));
                }
                lineIndex++;
            }
            
            return text;
        }

        private void RenderGenericBlock<T>(StringBuilder text, RenderingContext context, string[] lines, ref int lineIndex,
            ContextLevel newLevel, IEnumerable<T> list, Action<RenderingContext, T> load)
        {
            if (lineIndex >= lines.Length) return;
            int blockIndex = lineIndex;
            int index = 0;
            foreach (T obj in list)
            {
                var newContext = new RenderingContext(context, lines[lineIndex], newLevel);
                load(newContext, obj);
                newContext.First = obj.Equals(list.First());
                newContext.Last = obj.Equals(list.Last());
                newContext.Index = index++;
                blockIndex = lineIndex;
                text.Append(RenderMacroBlock(newContext, lines, ref blockIndex));
            }
            lineIndex = blockIndex;
        }


        public string RenderMacroLine(RenderingContext context, string inputText, bool appendNewLine = true)
        {
            StringBuilder text = new StringBuilder();
            string origText = "", outputText = inputText;
            int index1, index2;

            while (origText != outputText && ContainsToken(outputText, context.Level, out MacroToken token))
            {
                inputText = outputText;
                origText = inputText;
                switch (token.Name)
                {
                    case "File":
                    {
                        outputText = RenderMacro(inputText, token, context.File, ContextLevel.File, context, out RenderingContext newContext);
                        break;
                    }
                    case "FileName":
                    {
                        outputText = RenderMacro(inputText, token, Path.GetFileNameWithoutExtension(context.File), ContextLevel.File, context, out RenderingContext newContext);
                        break;
                    }
                    case "FileExt":
                    {
                        string ext = Path.GetExtension(context.File);
                        if (ext.StartsWith(".")) ext = ext.Substring(1);
                        outputText = RenderMacro(inputText, token, ext, ContextLevel.File, context, out RenderingContext newContext);
                        break;
                    }
                    case "Project":
                    {
                        outputText = RenderMacro(inputText, token, context.Objects.Data.Profile.ProjectName, ContextLevel.Project, context, out RenderingContext newContext);
                        break;
                    }
                    case "Machines":
                    {
                        outputText = RenderMacro(inputText, token, context.Objects.Data.Trees.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        break;
                    }
                    case "Machine":
                        if (context.Level.HasFlag(ContextLevel.Machine))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Machine.Name, ContextLevel.Machine, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(context, outputText));
                        }
                        else
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.Machine,
                                context.Objects.Data.Trees, (ctx, mach) => ctx.Objects.Machine = mach, mach => mach.Name);
                        }
                        return text.ToString();
                    case "States":
                    {
                        if (context.Level.HasFlag(ContextLevel.SuperState))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.SuperState.States.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        else if (context.Level.HasFlag(ContextLevel.Machine))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Machine.States.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        else
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Data.BasicStatesList.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        break;
                    }
                    case "State":
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.State.Name, ContextLevel.State, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(context, outputText));
                        }
                        else if (context.Level.HasFlag(ContextLevel.SuperState))
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.State,
                                context.Objects.SuperState.States, (ctx, state) => ctx.Objects.State = state, state => state.Name);
                        }
                        else if (context.Level.HasFlag(ContextLevel.Machine))
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.State,
                                context.Objects.Machine.States, (ctx, state) => ctx.Objects.State = state, state => state.Name);
                        }
                        else
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.State,
                                context.Objects.Data.BasicStatesList, (ctx, state) => ctx.Objects.State = state, state => state.Name);
                        }
                        return text.ToString();
                    case "Abort":
                    case "End":
                    {
                        if (context.Level.HasFlag(ContextLevel.Pointing) && context.Objects.Pointing.GetType().ToString().EndsWith(token.Name))
                        {
                            string macro = MacroBegin + token.Name;
                            string result = context.Objects.Pointing.Name;
                            outputText = inputText.Replace(macro, result);
                            text.Append(RenderMacroLine(context, outputText));
                        }
                        return text.ToString();
                    }
                    case "Transitions":
                    {
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.State.Transitions.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        else if (context.Level.HasFlag(ContextLevel.SuperState))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.SuperState.Transitions.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        else if (context.Level.HasFlag(ContextLevel.Machine))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Machine.BasicTransitionsList.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        else
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Data.TransitionsList.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        break;
                    }
                    case "Transition":
                        if (context.Level.HasFlag(ContextLevel.Transition))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Transition.Name, ContextLevel.Transition, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(context, outputText));
                        }
                        else if (context.Level.HasFlag(ContextLevel.State))
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.Transition,
                                context.Objects.State.Transitions, (ctx, trans) => ctx.Objects.Transition = trans, trans => trans.Name);
                        }
                        else if (context.Level.HasFlag(ContextLevel.SuperState))
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.Transition,
                                context.Objects.SuperState.Transitions, (ctx, trans) => ctx.Objects.Transition = trans, trans => trans.Name);
                        }
                        else if (context.Level.HasFlag(ContextLevel.Machine))
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.Transition,
                                context.Objects.Machine.BasicTransitionsList, (ctx, trans) => ctx.Objects.Transition = trans, trans => trans.Name);
                        }
                        else
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.Transition,
                                context.Objects.Data.BasicTransitionsList(), (ctx, trans) => ctx.Objects.Transition = trans, trans => trans.Name);
                        }
                        return text.ToString();
                    case "Pointing":
                        if (context.Level.HasFlag(ContextLevel.Transition))
                        {
                            if (context.Objects.Transition.Pointing == null)
                            {
                                outputText = RenderMacro(inputText, token, context.Objects.Transition.Transition.EndObject.Name, ContextLevel.File, context, out RenderingContext newContext);
                                break;
                            }
                            else
                            {
                                outputText = RenderMacro(inputText, token, context.Objects.Transition.Pointing.Name, ContextLevel.Transition, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            outputText = RenderMacro(inputText, token, "??", ContextLevel.File, context, out RenderingContext newContext);
                            break;
                        }
                        return text.ToString();
                    case "Outputs":
                        if (context.Level.HasFlag(ContextLevel.Transition))
                        {
                            foreach (BasicOutput output in context.Objects.Transition.Outputs.FindAll(var => var.Output is IBooleanValue))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.GetOperationCode(output.Operation), ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "Events":
                        if (context.Level.HasFlag(ContextLevel.Transition))
                        {
                            foreach (BasicOutput output in context.Objects.Transition.Outputs.FindAll(var => var.Output is EventOutput))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.Name, ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "Messages":
                        if (context.Level.HasFlag(ContextLevel.Transition))
                        {
                            foreach (BasicOutput output in context.Objects.Transition.Outputs.FindAll(var => var.Output is MessageFlag))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.Name, ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "Variables":
                    {
                        outputText = RenderMacro(inputText, token, context.Objects.Data.Variables.All.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        break;
                    }
                    case "Variable":
                        if (context.Level.HasFlag(ContextLevel.Variable))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Variable.Name, ContextLevel.Variable, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(newContext, outputText));
                        }
                        else if (context.Level.HasFlag(ContextLevel.Project))
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.Variable,
                                context.Objects.Data.Variables.All, (ctx, var) => ctx.Objects.Variable = var, var => var.Name);
                        }
                        return text.ToString();
                    case "InputEvents":
                    {
                        outputText = RenderMacro(inputText, token, context.Objects.Data.Variables.EventInputs.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        break;
                    }
                    case "InputEvent":
                        if (context.Level.HasFlag(ContextLevel.Variable) && context.Objects.Variable is EventInput)
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Variable.Name, ContextLevel.Variable, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(newContext, outputText));
                        }
                        else if (context.Level.HasFlag(ContextLevel.Project))
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.Variable,
                                context.Objects.Data.Variables.EventInputs, (ctx, var) => ctx.Objects.Variable = var, var => var.Name);
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "OutputEvents":
                    {
                        outputText = RenderMacro(inputText, token, context.Objects.Data.Variables.EventOutputs.Count.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        break;
                    }
                    case "OutputEvent":
                        if (context.Level.HasFlag(ContextLevel.Variable) && context.Objects.Variable is EventOutput)
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Variable.Name, ContextLevel.Variable, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(newContext, outputText));
                        }
                        else if (context.Level.HasFlag(ContextLevel.Project))
                        {
                            RenderGenericLine(text, context, token, inputText, ContextLevel.Variable,
                                context.Objects.Data.Variables.EventOutputs, (ctx, var) => ctx.Objects.Variable = var, var => var.Name);
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "Default":
                        if (context.Level.HasFlag(ContextLevel.Variable))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Variable.Default, ContextLevel.Variable, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(newContext, outputText));
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "Description":
                        // TODO: Handle redundance if the macro is inside the description.
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.State.State.Description, ContextLevel.Variable, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(newContext, outputText));
                        }
                        else if (context.Level.HasFlag(ContextLevel.SuperState))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.SuperState.State.Description, ContextLevel.Variable, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(newContext, outputText));
                        }
                        else if (context.Level.HasFlag(ContextLevel.Transition))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Transition.Transition.Description, ContextLevel.Variable, context, out RenderingContext newContext);
                            text.Append(RenderMacroLine(newContext, outputText));
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "EnterOutputs":
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            foreach (BasicOutput output in context.Objects.State.EnterOutputs.FindAll(var => var.Output is IBooleanValue))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.GetOperationCode(output.Operation), ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "EnterMessages":
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            foreach (BasicOutput output in context.Objects.State.EnterOutputs.FindAll(var => var.Output is IBooleanValue))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.Name, ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "EnterFlags":
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            foreach (BasicOutput output in context.Objects.State.EnterOutputs.FindAll(var => var.Output is IBooleanValue))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.Name, ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "ExitOutputs":
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            foreach (BasicOutput output in context.Objects.State.ExitOutputs.FindAll(var => var.Output is IBooleanValue))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.GetOperationCode(output.Operation), ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "ExitEvents":
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            foreach (BasicOutput output in context.Objects.State.ExitOutputs.FindAll(var => var.Output is EventOutput))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.Name, ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "ExitMessages":
                        if (context.Level.HasFlag(ContextLevel.State))
                        {
                            foreach (BasicOutput output in context.Objects.State.ExitOutputs.FindAll(var => var.Output is MessageFlag))
                            {
                                outputText = RenderMacro(inputText, token, output.Output.Name, ContextLevel.ExitOutputs, context, out RenderingContext newContext);
                                text.Append(RenderMacroLine(context, outputText));
                            }
                        }
                        else
                        {
                            text.Append(inputText);
                        }
                        return text.ToString();
                    case "Condition(":
                    {
                        index1 = inputText.IndexOf(MacroBegin + token.Name);
                        index2 = inputText.IndexOf(")", index1);
                        if (index2 == -1) break;
                        string macro = inputText.Substring(index1, index2 - index1 + 1);
                        string content = macro.Substring(MacroBegin.Length + token.Name.Length, macro.Length - MacroBegin.Length - token.Name.Length - 1);
                        if (string.IsNullOrWhiteSpace(content)) break;

                        if (context.Level.HasFlag(ContextLevel.Transition))
                        {
                            outputText = inputText.Replace(macro, context.Objects.Transition.GetCondition("", content));
                            text.Append(RenderMacroLine(context, outputText));
                        }
                        return text.ToString();
                    }
                    case "Condition":
                    {
                        if (context.Level.HasFlag(ContextLevel.Transition))
                        {
                            outputText = RenderMacro(inputText, token, context.Objects.Transition.GetCondition("", "t"), ContextLevel.Project, context, out RenderingContext newContext);
                        }
                        else
                        {
                            outputText = "??";
                        }
                        break;
                    }
                    case "Index":
                    {
                        outputText = RenderMacro(inputText, token, context.Index.ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        break;
                    }
                    case "Count":
                    {
                        outputText = RenderMacro(inputText, token, (context.Index + 1).ToString(), ContextLevel.Project, context, out RenderingContext newContext);
                        break;
                    }
                    case "nf(":
                        //index1 = inputText.IndexOf(MacroBegin + token.Name);
                        //while (index1 > 0)
                        //{
                        //    index2 = inputText.IndexOf(MacroBegin + ")", index1);
                        //    if (index2 == -1) break;
                        //    string macro = inputText.Substring(index1, index2 - index1 + MacroBegin.Length + 1);
                        //    string content = macro.Substring(MacroBegin.Length + token.Name.Length, macro.Length - MacroBegin.Length * 2 - token.Name.Length - 1);
                        //    if (!context.First) outputText = inputText.Replace(macro, content);
                        //    else outputText = inputText.Replace(macro, "");
                        //    index1 = inputText.IndexOf(MacroBegin + token.Name, index2);
                        //}
                        outputText = RenderGenericCondition(context, token, inputText, () => !context.First);
                        break;
                    case "nl(":
                        //index1 = inputText.IndexOf(MacroBegin + token.Name);
                        //while (index1 > 0)
                        //{
                        //    index2 = inputText.IndexOf(MacroBegin + ")", index1);
                        //    if (index2 == -1) break;
                        //    string macro = inputText.Substring(index1, index2 - index1 + MacroBegin.Length + 1);
                        //    string content = macro.Substring(MacroBegin.Length + token.Name.Length, macro.Length - MacroBegin.Length * 2 - token.Name.Length - 1);
                        //    if (!context.Last) outputText = inputText.Replace(macro, content);
                        //    else outputText = inputText.Replace(macro, "");
                        //    index1 = inputText.IndexOf(MacroBegin + token.Name, index2);
                        //}
                        outputText = RenderGenericCondition(context, token, inputText, () => !context.Last);
                        break;
                    case "of(":
                        //index1 = inputText.IndexOf(MacroBegin + token.Name);
                        //while (index1 > 0)
                        //{
                        //    index2 = inputText.IndexOf(MacroBegin + ")", index1);
                        //    if (index2 == -1) break;
                        //    string macro = inputText.Substring(index1, index2 - index1 + MacroBegin.Length + 1);
                        //    string content = macro.Substring(MacroBegin.Length + token.Name.Length, macro.Length - MacroBegin.Length * 2 - token.Name.Length - 1);
                        //    if (context.First) outputText = inputText.Replace(macro, content);
                        //    else outputText = inputText.Replace(macro, "");
                        //    index1 = inputText.IndexOf(MacroBegin + token.Name, index2);
                        //}
                        outputText = RenderGenericCondition(context, token, inputText, () => context.First);
                        break;
                    case "ol(":
                        //index1 = inputText.IndexOf(MacroBegin + token.Name);
                        //while (index1 > 0)
                        //{
                        //    index2 = inputText.IndexOf(MacroBegin + ")", index1);
                        //    if (index2 == -1) break;


                        //    string content = macro.Substring(MacroBegin.Length + token.Name.Length, macro.Length - MacroBegin.Length * 2 - token.Name.Length - 1);
                        //    if (context.Last) outputText = inputText.Replace(macro, content);
                        //    else outputText = inputText.Replace(macro, "");
                        //    index1 = inputText.IndexOf(MacroBegin + token.Name, index2);
                        //}
                        outputText = RenderGenericCondition(context, token, inputText, () => context.Last);
                        break;
                }
            }
            if (appendNewLine) text.AppendLine(outputText);
            else text.Append(outputText);

            return text.ToString();
        }

        string RenderGenericCondition(RenderingContext context, MacroToken token, string inputText, Func<bool> check)
        {
            string outputText;
            int tokenIndex = inputText.IndexOf(MacroBegin + token.Name);
            int afterTokenIndex = tokenIndex + MacroBegin.Length + token.Name.Length;

            inputText = inputText.Substring(0, afterTokenIndex) + RenderMacroLine(context, inputText.Substring(afterTokenIndex), false);
            int closeIndex = inputText.IndexOf(MacroBegin + ")", afterTokenIndex);
            if (closeIndex >= 0)
            {
                int endIndex = closeIndex + MacroBegin.Length + ")".Length;
                int fullContentCount = closeIndex - afterTokenIndex;
                int divisorIndex = inputText.IndexOf(MacroBegin + "|", afterTokenIndex, fullContentCount);
                string trueValue, falseValue;
                if (divisorIndex >= 0)
                {
                    int divisorLength = MacroBegin.Length + "|".Length;
                    int otherContentCount = closeIndex - divisorIndex - divisorLength;
                    int contentCount = fullContentCount - otherContentCount - divisorLength;
                    trueValue = inputText.Substring(afterTokenIndex, contentCount);
                    falseValue = inputText.Substring(divisorIndex + divisorLength, otherContentCount);
                }
                else
                {
                    trueValue = inputText.Substring(afterTokenIndex, fullContentCount);
                    falseValue = "";
                }
                int fullMacroCount = endIndex - tokenIndex;
                string macro = inputText.Substring(tokenIndex, fullMacroCount);
                if (check()) outputText = inputText.Replace(macro, trueValue);
                else outputText = inputText.Replace(macro, falseValue);
            }
            else
            {
                return inputText;
            }
            return outputText;
        }

        void RenderGenericLine<T>(StringBuilder text, RenderingContext context, MacroToken token, string inputText,
            ContextLevel newLevel, IEnumerable<T> list, Action<RenderingContext, T> load, Func<T, string> value)
        {
            int index = 0;
            foreach (T obj in list)
            {
                string outputText = RenderMacro(inputText, token, value(obj), newLevel, context, out RenderingContext newContext);
                load(newContext, obj);
                newContext.First = obj.Equals(list.First());
                newContext.Last = obj.Equals(list.Last());
                newContext.Index = index++;
                text.Append(RenderMacroLine(newContext, outputText));
            }
        }

        #region "General"

        [Description("Path relative to phases file where the files will be generated."), Category("General")]
        public string GenerationPath { get; set; } = "";

        #endregion


        #region "Primary Cottle Tokens"

        [Description("Enable Cottle block tokens."), Category("Primary Cottle tokens")]
        public bool EnableCottle { get; set; } = true;

        [Description("Cottle block begin token."), Category("Primary Cottle tokens")]
        public string BlockBegin { get; set; } = "{";

        [Description("Cottle block continue token, for 'if' instructions."), Category("Primary Cottle tokens")]
        public string BlockContinue { get; set; } = "|";

        [Description("Cottle block end token."), Category("Primary Cottle tokens")]
        public string BlockEnd { get; set; } = "}";

        [Description("Cottle escape token."), Category("Primary Cottle tokens")]
        public char Escape { get; set; } = '\\';

        #endregion


        #region "Secondary Cottle Tokens"

        [Description("Enable alternative Cottle block tokens, these cannot be mixed with the primary tokens."), Category("Secondary Cottle tokens")]
        public bool EnableAlt { get; set; } = false;

        [Description("Alternative Cottle block begin token."), Category("Secondary Cottle tokens")]
        public string AltBlockBegin { get; set; } = "<#";

        [Description("Alternative Cottle block continue token."), Category("Secondary Cottle tokens")]
        public string AltBlockContinue { get; set; } = "<|";

        [Description("Alternative Cottle block end token."), Category("Secondary Cottle tokens")]
        public string AltBlockEnd { get; set; } = "#>";

        [Description("Alternative Cottle escape token."), Category("Secondary Cottle tokens")]
        public char AltEscape { get; set; } = '\\';
        #endregion


        #region "Conditional operators"

        [Description("Not operator to be used when rendering transitions conditionals."), Category("Conditional operators")]
        public string Not { get; set; } = "!";

        [Description("And operator to be used when rendering transitions conditionals."), Category("Conditional operators")]
        public string And { get; set; } = "&&";

        [Description("Or operator to be used when rendering transitions conditionals."), Category("Conditional operators")]
        public string Or { get; set; } = "||";

        [Description("Xor operator to be used when rendering transitions conditionals."), Category("Conditional operators")]
        public string Xor { get; set; } = "^";

        [Description("Grouping start symbol to be used when rendering transitions conditionals."), Category("Conditional operators")]
        public string GroupStart { get; set; } = "(";

        [Description("Grouping end symbol to be used when rendering transitions conditionals."), Category("Conditional operators")]
        public string GroupEnd { get; set; } = ")";

        #endregion


        #region "Boolean Values"

        [Description("Not operator to be used when rendering transitions conditionals."), Category("Boolean Values")]
        public string True { get; set; } = "1";

        [Description("And operator to be used when rendering transitions conditionals."), Category("Boolean Values")]
        public string False { get; set; } = "0";

        #endregion


        #region "Macro tokens"

        [Description("Macro beging token."), Category("Macro tokens")]
        public string MacroBegin { get; set; } = "@";

        [Description("Macro end token."), Category("Macro tokens")]
        public string MacroEnd { get; set; } = "";

        [Description("Macro beging token."), Category("Macro tokens")]
        public string MacroContext { get; set; } = "#";

        [Description("Macro block begin token."), Category("Macro block tokens")]
        public string MacroBlockBegin { get; set; } = "<@";

        [Description("Macro block end token."), Category("Macro block tokens")]
        public string MacroBlockEnd { get; set; } = "@>";

        #endregion

    }
}
