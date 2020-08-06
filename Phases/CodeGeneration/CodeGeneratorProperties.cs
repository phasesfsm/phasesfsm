using Phases.BasicObjects;
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

            // Combos
            All = File | Project | Machine | Node | SuperState | State | Transition | EnterOutputs | ExitOutputs | Directory,
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
            }
        }

        public class RenderingContext
        {
            public ContextLevel Level { get; }
            public ContextObjects Objects { get; }
            public string Value { get; }

            public RenderingContext(ContextLevel level, ContextObjects objects, string value)
            {
                Level = level;
                Objects = objects;
                Value = value;
            }

            public RenderingContext(RenderingContext context, string newValue = "", ContextLevel newLevel = ContextLevel.NoContext)
            {
                if (newLevel == ContextLevel.NoContext)
                    Level = context.Level;
                else
                    Level = newLevel;
                if (newValue == "")
                    Value = context.Value;
                else
                    Value = newValue;
                Objects = new ContextObjects(context.Objects);
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
            public static readonly MacroToken File = new MacroToken("File", ContextLevel.NoFileName);
            public static readonly MacroToken FileName = new MacroToken("FileName", ContextLevel.NoFileName);
            public static readonly MacroToken FileExt = new MacroToken("FileExt", ContextLevel.NoFileName);

            public static readonly MacroToken Project = new MacroToken("Project", ContextLevel.All);
            public static readonly MacroToken Machine = new MacroToken("Machine", ContextLevel.NoDirName);
            public static readonly MacroToken Node = new MacroToken("Node", ContextLevel.NoFileName);
            public static readonly MacroToken SuperState = new MacroToken("SuperState", ContextLevel.NoFileName);
            public static readonly MacroToken State = new MacroToken("State", ContextLevel.NoFileName);
            public static readonly MacroToken Transition = new MacroToken("Transition", ContextLevel.NoOutputs);

            public static readonly MacroToken Variable = new MacroToken("Variable", ContextLevel.NoFileName);
            public static readonly MacroToken BoolInput = new MacroToken("BoolInput", ContextLevel.NoOutputs);
            public static readonly MacroToken BoolOutput = new MacroToken("BoolOutput", ContextLevel.NoFileName);
            public static readonly MacroToken BoolFlag = new MacroToken("BoolFlag", ContextLevel.NoFileName);
            public static readonly MacroToken InputEvent = new MacroToken("InputEvent", ContextLevel.NoOutputs);
            public static readonly MacroToken OutputEvent = new MacroToken("OutputEvent", ContextLevel.NoCondition);
            public static readonly MacroToken CounterFlag = new MacroToken("CounterFlag", ContextLevel.NoFileName);
            public static readonly MacroToken MessageFlag = new MacroToken("MessageFlag", ContextLevel.NoFileName);

            public static readonly MacroToken EnterOutputs = new MacroToken("EnterOutputs", ContextLevel.States);
            public static readonly MacroToken ExitOutputs = new MacroToken("ExitOutputs", ContextLevel.States);

            public static readonly MacroToken Condition = new MacroToken("Condition(", ContextLevel.Transition);
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
                    if (reference.Context.HasFlag(context) && inputText.ToLower().Contains(MacroBegin + fieldInfo.Name.ToLower() + MacroEnd))
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

        public string RenderMacro(string inputText, MacroToken token, string value)
        {
            int macroIndex = inputText.ToLower().IndexOf(MacroBegin + token.Name.ToLower() + MacroEnd);
            if (macroIndex == -1) return inputText;
            string macro = inputText.Substring(macroIndex, MacroBegin.Length + token.Name.Length + MacroEnd.Length);

            if (token.Render == MacroRender.LowerCase)
                value = value.ToLower();
            else if (token.Render == MacroRender.UpperCase)
                value = value.ToUpper();

            return inputText.Replace(macro, value);
        }

        public List<RenderingContext> RenderMacroDirectory(string inputText, RenderingContext context)
        {
            List<RenderingContext> result = new List<RenderingContext>();
            RenderingContext newContext;
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
                            inputText = RenderMacro(inputText, token, token.Name);
                        }
                        else
                        {
                            inputText = RenderMacro(inputText, token, context.Objects.Data.Profile.ProjectName);
                        }
                        break;
                    case "Machine":
                        switch (level)
                        {
                            case ContextLevel.Project:
                                if (context.Objects.Data == null)
                                {
                                    newContext = new RenderingContext(context, RenderMacro(inputText, token, token.Name), ContextLevel.Machine);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (BasicObjectsTree machine in context.Objects.Data.Trees)
                                    {
                                        newContext = new RenderingContext(context, RenderMacro(inputText, token, machine.Name), ContextLevel.Machine);
                                        newContext.Objects.Machine = machine;
                                        result.Add(newContext);
                                    }
                                }
                                level = ContextLevel.Machine;
                                break;
                            case ContextLevel.Machine:
                                newContext = new RenderingContext(context, RenderMacro(inputText, token, context.Objects.Machine.Name));
                                result.Add(newContext);
                                break;
                        }
                        inputText = result.First().Value;
                        break;
                }
            }
            if (result.Count == 0)
            {
                newContext = new RenderingContext(context, inputText);
                result.Add(newContext);
            }
            return result;
        }

        public List<RenderingContext> RenderMacroFile(string inputText, List<RenderingContext> contexts)
        {
            List<RenderingContext> result = new List<RenderingContext>();
            RenderingContext newContext;
            ContextLevel level = contexts.First().Level;
            string origText = "";

            while (origText != inputText && ContainsToken(inputText, contexts.First().Level, out MacroToken token))
            {
                origText = inputText;
                switch (token.Name)
                {
                    case "Project":
                        if (contexts.First().Objects.Data == null)
                        {
                            inputText = RenderMacro(inputText, token, token.Name);
                        }
                        else
                        {
                            inputText = RenderMacro(inputText, token, contexts.First().Objects.Data.Profile.ProjectName);
                        }
                        break;
                    case "Machine":
                        switch (level)
                        {
                            case ContextLevel.Project:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.Machine);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (BasicObjectsTree machine in contexts.First().Objects.Data.Trees)
                                    {
                                        foreach (RenderingContext context in contexts)
                                        {
                                            newContext = new RenderingContext(context, RenderMacro(inputText, token, machine.Name), ContextLevel.Machine);
                                            newContext.Objects.Machine = machine;
                                            result.Add(newContext);
                                        }
                                    }
                                }
                                level = ContextLevel.Machine;
                                break;
                            case ContextLevel.Machine:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.Machine);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (RenderingContext context in contexts)
                                    {
                                        newContext = new RenderingContext(context, RenderMacro(inputText, token, context.Objects.Machine.Name));
                                        result.Add(newContext);
                                    }
                                }
                                break;
                        }
                        inputText = result.First().Value;
                        break;
                    case "Node":
                        switch (level)
                        {
                            case ContextLevel.Project:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.State);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (BasicState state in contexts.First().Objects.Data.BasicStatesList)
                                    {
                                        newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, state.Name), contexts.First().Level | ContextLevel.State);
                                        newContext.Objects.State = state;
                                        result.Add(newContext);
                                    }
                                }
                                level = ContextLevel.Machine;
                                break;
                            case ContextLevel.Machine:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.State);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (RenderingContext context in contexts)
                                    {
                                        foreach (BasicState state in context.Objects.Machine.BasicStatesList)
                                        {
                                            newContext = new RenderingContext(context, RenderMacro(inputText, token, state.Name), context.Level | ContextLevel.State);
                                            newContext.Objects.State = state;
                                            result.Add(newContext);
                                        }
                                    }
                                }
                                break;
                        }
                        inputText = result.First().Value;
                        break;
                    case "SuperState":
                        switch (level)
                        {
                            case ContextLevel.Project:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.State);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (BasicState state in contexts.First().Objects.Data.SuperStatesList())
                                    {
                                        newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, state.Name), contexts.First().Level | ContextLevel.State);
                                        newContext.Objects.State = state;
                                        result.Add(newContext);
                                    }
                                }
                                level = ContextLevel.Machine;
                                break;
                            case ContextLevel.Machine:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.State);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (RenderingContext context in contexts)
                                    {
                                        foreach (BasicState state in context.Objects.Machine.SuperStatesList())
                                        {
                                            newContext = new RenderingContext(context, RenderMacro(inputText, token, state.Name), context.Level | ContextLevel.State);
                                            newContext.Objects.State = state;
                                            result.Add(newContext);
                                        }
                                    }
                                }
                                break;
                        }
                        inputText = result.First().Value;
                        break;
                    case "State":
                        switch (level)
                        {
                            case ContextLevel.Project:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.State);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (BasicState state in contexts.First().Objects.Data.StatesList())
                                    {
                                        newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, state.Name), contexts.First().Level | ContextLevel.State);
                                        newContext.Objects.State = state;
                                        result.Add(newContext);
                                    }
                                }
                                level = ContextLevel.Machine;
                                break;
                            case ContextLevel.Machine:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.State);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (RenderingContext context in contexts)
                                    {
                                        foreach (BasicState state in context.Objects.Machine.StatesList())
                                        {
                                            newContext = new RenderingContext(context, RenderMacro(inputText, token, state.Name), context.Level | ContextLevel.State);
                                            newContext.Objects.State = state;
                                            result.Add(newContext);
                                        }
                                    }
                                }
                                break;
                            case ContextLevel.Machine | ContextLevel.State:
                                if (contexts.First().Objects.Data == null)
                                {
                                    newContext = new RenderingContext(contexts.First(), RenderMacro(inputText, token, token.Name), ContextLevel.State);
                                    result.Add(newContext);
                                }
                                else
                                {
                                    foreach (RenderingContext context in contexts)
                                    {
                                        foreach (BasicState state in context.Objects.Machine.StatesList())
                                        {
                                            newContext = new RenderingContext(context, RenderMacro(inputText, token, state.Name), context.Level | ContextLevel.State);
                                            newContext.Objects.State = state;
                                            result.Add(newContext);
                                        }
                                    }
                                }
                                break;
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
                        newContext = new RenderingContext(context, inputText);
                        result.Add(newContext);
                    }
                }
                else
                {
                    newContext = new RenderingContext(contexts.First(), inputText);
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

        #region "General"

        [Description("Path relative to phases file where the files will be generated."), Category("General")]
        public string GenerationPath { get; set; } = "";

        #endregion


        #region "Primary Cottle Tokens"

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
