using Cottle;
using Cottle.Documents;
using Cottle.Exceptions;
using Cottle.Functions;
using Cottle.Settings;
using Cottle.Stores;
using Cottle.Values;
using Phases.BasicObjects;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.CodeGeneration.Interpreter
{
    class Project : ICodeGenerationProject
    {
        public string Name => Data.Profile.ProjectName;

        public GeneratorData Data { get; private set; }
        private string scriptsFolder, path;
        private bool fileHead, enableAltRender = false;
        private CustomSetting pjSetting, pjAltSetting;
        private BasicObjectsTree activeMachine = null;
        private DateTime dateTime;

        public Project(GeneratorData generatorData, string scriptsFolder)
        {
            Data = generatorData;
            this.scriptsFolder = scriptsFolder;
        }

        public bool GenerateCode()
        {
            bool res;
            dateTime = DateTime.Now;
            BuildStore();
            if (LoadSettings() == false) return false;

            foreach (string filePath in Directory.EnumerateFiles(scriptsFolder))
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName == "cottle.ini") continue;
                fileHead = true;
                res = ProccessScript(filePath, path, fileName);
                if (!res) return false;
            }
            res = ProccessDirs(scriptsFolder, path);
            if (!res) return false;
            return true;
        }

        bool ProccessDirs(string scriptsPath, string destPath)
        {
            bool res;
            try
            {
                foreach (string subdir in Directory.GetDirectories(scriptsPath))
                {
                    string dirName = Path.GetFileName(subdir);
                    string finalDirName, finalPath = destPath;
                    switch (dirName)
                    {
                        case "@project":
                            finalDirName = dirName.Replace("@project", Name);
                            finalPath = Path.Combine(destPath, finalDirName);
                            Directory.CreateDirectory(finalPath);
                            break;
                        case "@machine":
                            foreach (BasicObjectsTree tree in Data.Trees)
                            {
                                activeMachine = tree;
                                finalDirName = dirName.Replace("@project", tree.Name);
                                finalPath = Path.Combine(destPath, tree.Name);
                                Directory.CreateDirectory(finalPath);
                                foreach (string filePath in Directory.GetFiles(subdir))
                                {
                                    string fileName = Path.GetFileName(filePath);
                                    res = ProccessScript(filePath, finalPath, fileName);
                                    if (!res) return false;
                                }
                            }
                            ProccessDirs(subdir, finalPath);
                            continue;
                        default:
                            finalDirName = dirName;
                            break;
                    }
                    activeMachine = null;
                    foreach (string filePath in Directory.GetFiles(subdir))
                    {
                        string fileName = Path.GetFileName(filePath);
                        res = ProccessScript(filePath, finalPath, fileName);
                        if (!res) return false;
                    }
                    ProccessDirs(subdir, finalPath);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private string ParseFileName(string fileName)
        {
            return fileName;
        }

        private bool LoadSettings()
        {
            path = Data.Profile.Path;
            pjSetting = new CustomSetting
            {
                Trimmer = CustomTrimmer
            };
            pjAltSetting = new CustomSetting
            {
                Trimmer = CustomTrimmer
            };

            if (Data.Profile.Properties == null)
            {
                Data.Profile.Properties = new CodeGeneratorProperties(Path.Combine(path, "cottle.ini"));
            }

            pjSetting.BlockBegin = Data.Profile.Properties.BlockBegin;
            pjSetting.BlockContinue = Data.Profile.Properties.BlockContinue;
            pjSetting.BlockEnd = Data.Profile.Properties.BlockEnd;

            enableAltRender = Data.Profile.Properties.EnableAlt;
            pjAltSetting.BlockBegin = Data.Profile.Properties.AltBlockBegin;
            pjAltSetting.BlockContinue = Data.Profile.Properties.AltBlockContinue;
            pjAltSetting.BlockEnd = Data.Profile.Properties.AltBlockEnd;

            path = Data.Profile.Properties.GenerationPath;

            /*if (line.StartsWith("Option="))
            {
                switch(line.Substring("Option=".Length).ToLower())
                {
                    case "renew":
                        RenewFolder();
                        break;
                }
            }*/
            return true;
        }

        private BuiltinStore BuildStore()
        {
            var store = new BuiltinStore();

            store["Project"] = Data.Profile.ProjectName;

            store["Variables"] = new Dictionary<Value, Value>
            {
                { "BooleanInputs", Data.Variables.BooleanInputs.ConvertAll(var => (Value)(var as BooleanInput).GetDictionary()) },
                { "InputEvents", Data.Variables.EventInputs.ConvertAll(var => (Value)var.Name) },
                { "BooleanFlags", Data.Variables.BooleanFlags.ConvertAll(var => (Value)(var as BooleanFlag).GetDictionary()) },
                { "CounterFlags", Data.Variables.CounterFlags.ConvertAll(var => (Value)(var as CounterFlag).GetDictionary()) },
                { "MessageFlags", Data.Variables.MessageFlags.ConvertAll(var => (Value)var.Name) },
                { "BooleanOutputs", Data.Variables.BooleanOutputs.ConvertAll(var => (Value)(var as BooleanOutput).GetDictionary()) },
                { "OutputEvents", Data.Variables.EventOutputs.ConvertAll(var => (Value)var.Name) }
            };

            store["Relations"] = Data.RelationsList.ConvertAll(indir => (Value)indir.GetDictionary());
            store["Equations"] = Data.EquationsList.ConvertAll(equation => (Value)equation.GetDictionary());
            store["Machines"] = Data.Trees.ConvertAll(tree => (Value)tree.GetDictionary());
            store["Transitions"] = Data.BasicTransitionsList().ToList().ConvertAll(trans => (Value)trans.GetDictionary());
            store["SuperStates"] = Data.SuperStatesList().ToList().ConvertAll(state => (Value)state.GetDictionary());
            store["States"] = Data.StatesList().ToList().ConvertAll(state => (Value)state.GetDictionary());

            if (activeMachine != null)
            {
                store["MACHINE"] = activeMachine.GetDictionary();
            }

            store["date"] = new NativeFunction((values, output) =>
            {
                return dateTime.ToString();
            }, 1);
            return store;
        }

        private IDocument CreateDocumentFromFile(string file)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(file);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Error pre-parsing file: {0}.{1}{2}", file, Environment.NewLine, ex.Message), "Phases",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return null;
            }

            StringBuilder text = new StringBuilder();
            foreach (string line in lines)
            {
                string textLine;
                if (line.Contains("@project"))
                {
                    textLine = line.Replace("@project", Name);
                }
                else
                {
                    textLine = line;
                }
                if (textLine.Contains("@machine"))
                {
                    foreach (BasicObjectsTree tree in Data.Trees)
                    {
                        text.AppendLine(textLine.Replace("@machine", tree.Name));
                    }
                }
                else
                {
                    text.AppendLine(textLine);
                }
            }
            IDocument document;
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(text.ToString())))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    try
                    {
                        document = new SimpleDocument(reader, pjSetting); // throws ParseException on error
                    }
                    catch (ParseException ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Error parsing {3}:\r\nLine {0}, pos {1}: {2}", ex.Line, ex.Column, ex.Message, file), "Cottle",
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        stream.Dispose();
                        return null;
                    }
                }
            }
            return document;
        }

        private IDocument CreateDocument(string templateText)
        {
            string[] lines = templateText.Split(new string[]{ Environment.NewLine, "\r", "\n" }, StringSplitOptions.None);

            StringBuilder text = new StringBuilder();
            foreach (string line in lines)
            {
                string textLine;
                if (line.Contains("@project"))
                {
                    textLine = line.Replace("@project", Name);
                }
                else
                {
                    textLine = line;
                }
                if (textLine.Contains("@machine"))
                {
                    foreach (BasicObjectsTree tree in Data.Trees)
                    {
                        text.AppendLine(textLine.Replace("@machine", tree.Name));
                    }
                }
                else
                {
                    text.AppendLine(textLine);
                }
            }

            IDocument document;
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(text.ToString())))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    try
                    {
                        document = new SimpleDocument(reader, pjSetting); // throws ParseException on error
                    }
                    catch (ParseException ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Error parsing:\r\nLine {0}, pos {1}: {2}", ex.Line, ex.Column, ex.Message), "Cottle",
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        stream.Dispose();
                        return null;
                    }
                }
            }
            return document;
        }

        private string RenderDocument(IDocument document, BuiltinStore store, string file)
        {
            string text;
            try
            {
                text = document.Render(store); // throws ParseException on error
            }
            catch (ParseException ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Error rendering {3}:\r\nLine {0}, pos {1}: {2}", ex.Line, ex.Column, ex.Message, file), "Cottle",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return null;
            }
            if (enableAltRender)
            {
                IDocument altDocument;

                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(text)))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        try
                        {
                            altDocument = new SimpleDocument(reader, pjAltSetting); // throws ParseException on error
                        }
                        catch (ParseException ex)
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Error in alt parsing {3}:\r\nLine {0}, pos {1}: {2}", ex.Line, ex.Column, ex.Message, file), "Cottle",
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                            stream.Dispose();
                            return null;
                        }
                    }
                }
                try
                {
                    text = altDocument.Render(store); // throws ParseException on error
                }
                catch (ParseException ex)
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("Error in alt rendering {3}:\r\nLine {0}, pos {1}: {2}", ex.Line, ex.Column, ex.Message, file), "Cottle",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return null;
                }
            }
            return text;
        }

        private bool ProccessScript(string file, string destPath, string destFile)
        {
            IDocument document = CreateDocumentFromFile(file);
            if (document == null) return false;

            if(destFile.StartsWith("@machine"))
            {
                if (activeMachine != null)
                {
                    RenderScriptAndSave(document, file, destPath, destFile.Replace("@machine", activeMachine.Name));
                }
                else
                {
                    foreach (BasicObjectsTree tree in Data.Trees)
                    {
                        RenderScriptAndSave(document, file, destPath, destFile.Replace("@machine", tree.Name));
                    }
                }
            }
            else if (destFile.StartsWith("@project"))
            {
                RenderScriptAndSave(document, file, destPath, destFile.Replace("@project", Name));
            }
            else
            {
                RenderScriptAndSave(document, file, destPath, destFile);
            }
            return true;
        }

        private void loadFileMacros(BuiltinStore store, string file)
        {
            store["FILENAME"] = Path.GetFileNameWithoutExtension(file);
            store["FILE"] = file;
        }

        private void loadFunctions(BuiltinStore store, string destFile)
        {
            string fileExt = Path.GetExtension(destFile);
            store["filename"] = new NativeFunction((values, output) =>
            {
                string newNameExt = Path.GetExtension(values[0].AsString);
                if (newNameExt == "")
                {
                    destFile = values[0].AsString + fileExt;
                }
                else
                {
                    destFile = values[0].AsString;
                }
                loadFileMacros(store, destFile);
                return "";
            }, 1);
        }

        private bool RenderScriptAndSave(IDocument document, string file, string destPath, string destFile)
        {
            BuiltinStore store = BuildStore();

            loadFunctions(store, destFile);
            loadFileMacros(store, destFile);

            string text = RenderDocument(document, store, file);
            if (text == null) return false;

            try
            {
                File.WriteAllText(Path.Combine(destPath, destFile), text);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Error saving output file {0}: {1}", destFile, ex.Message));
                return false;
            }
            return true;
        }

        public string RenderScript(string scriptText, string fileName)
        {
            dateTime = DateTime.Now;
            
            BuildStore();
            if (LoadSettings() == false) return null;

            IDocument document = CreateDocument(scriptText);
            if (document == null) return null;

            BuiltinStore store = BuildStore();

            loadFunctions(store, fileName);
            loadFileMacros(store, fileName);

            return RenderDocument(document, store, fileName);
        }

        public bool RenderDual(string scriptText, string fileName, out string renderedInput, out string renderedOutput, out List<int> sectionsLines)
        {
            string[] sourceLines = scriptText.Split(Environment.NewLine);
            var sourceText = new StringBuilder();
            int index = 0;
            foreach (string line in sourceLines)
            {
                sourceText.Append(index);
                sourceText.Append('\t');
                sourceText.AppendLine(line);
                index++;
            }
            string linedInput = sourceText.ToString();
            string linedOutput = RenderScript(linedInput, fileName);
            if (linedOutput == null)
            {
                renderedInput = null;
                renderedOutput = null;
                sectionsLines = null;
                return false;
            }

            string[] resultLines = linedOutput.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var input = new StringBuilder();
            var output = new StringBuilder();
            sectionsLines = new List<int>();
            index = 0;
            int ridx = 0, num = 0, left, lines = 0;
            sectionsLines.Add(lines);
            bool sect1 = false;
            foreach (string line in sourceLines)
            {
                lines = input.ToString().Split(Environment.NewLine).Count() - 1;
                input.AppendLine(line);
                left = 0;
                for (int i = ridx; i < resultLines.Length; i++)
                {
                    int tindex = resultLines[i].IndexOf('\t');
                    if (resultLines[i].Contains('\t') && int.TryParse(resultLines[i].Substring(0, tindex), out int lnum))
                    {
                        if (lnum <= index)
                        {
                            output.AppendLine(resultLines[i].Substring(tindex + 1));
                            num = lnum;
                            left++;
                        }
                        else
                        {
                            ridx = i;
                            break;
                        }
                    }
                }
                bool sect = false;
                bool sect2 = false;
                if (index > num)
                {
                    output.AppendLine();
                    num++;
                    sect2 = true;
                }
                if (!sect1 && sect2)
                {
                    sect1 = true;
                    if (!sectionsLines.Contains(lines - 1)) sectionsLines.Add(lines - 1);
                }
                else if (sect1 && !sect2)
                {
                    sect1 = false;
                    if (!sectionsLines.Contains(lines)) sectionsLines.Add(lines);
                }
                while (left > 1)
                {
                    input.AppendLine();
                    left--;
                    sect = true;
                }
                if (sect)
                {
                    if (!sectionsLines.Contains(lines)) sectionsLines.Add(lines);
                    lines = input.ToString().Split(Environment.NewLine).Count() - 1;
                    if (!sectionsLines.Contains(lines)) sectionsLines.Add(lines);
                }
                index++;
            }
            lines = input.ToString().Split(Environment.NewLine).Count();
            if (!sectionsLines.Contains(lines)) sectionsLines.Add(lines);
            renderedInput = input.ToString();
            renderedOutput = output.ToString();
            return true;
        }

        private void RenewFolder()
        {
            string renewPath = Path.Combine(path, Name);

            if (Directory.Exists(renewPath))
            {
                //if (MessageBox.Show("The selected path already has a directory with the same project name, all the contents will be erased and reemplazed with the new code, continue?", "Reeplace existing directory", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                //{
                Directory.Delete(renewPath, true);
                //}
            }

            //Create Project folder
            if (!Directory.Exists(renewPath))
            {
                Directory.CreateDirectory(renewPath);
            }
        }

        private string CustomTrimmer(string text)
        {
            StringBuilder sb = new StringBuilder();
            string part;
            int index;

            //line break at the end, means text before the block
            index = text.Length - 1;
            while (index > 0 && (text[index] == ' ' || text[index] == '\t')) index--;
            index--;
            if (index >= 0 && text.Substring(index, 2) == "\r\n")
            {
                while (fileHead && index > 0 && (text[index - 1] == ' ' || text[index - 1] == '\t')) index--;
                part = text.Substring(0, index);
            }
            else if (fileHead && !text.Contains("\r\n"))
            {
                part = "";
            }
            else
            {
                part = text;
            }

            if (fileHead && part != "")
            {
                fileHead = false;
                index = 0;
                //line break at the start, means text after block
                while (index < part.Length - 2 && (part[index] == ' ' || part[index] == '\t')) index++;
                if (index + 2 <= part.Length && part.Substring(index, 2) == "\r\n")
                {
                    part = part.Substring(index + 2);
                }
            }
            sb.Insert(0, part);

            return sb.ToString();
        }
    }
}
