using Phases.CodeGeneration;
using Phases.CodeGeneration.Interpreter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Phases.CodeGeneration.CodeGeneratorProperties;

namespace Phases
{
    public partial class CottleEditor : Form
    {
        GeneratorData Data;
        RenderingContext Context;
        string RootPath, Source, DestFile;
        Project project;

        internal CottleEditor(string rootPath, string source, string destFile, RenderingContext context)
        {
            RootPath = rootPath;
            Context = context;
            Data = context.Objects.Data;
            Source = source;
            DestFile = destFile;
            project = new Project(Data, RootPath);

            InitializeComponent();

            dualTextBox1.SourceFormat.Color = Color.DarkGreen;
            dualTextBox1.SourceFormat.Style = FontStyle.Regular;
            string[] macros_keywords = { "project", "machine" };
            var macros = dualTextBox1.ResultFormat.AddGroup("macros", @"@\w+", Color.Black, FontStyle.Regular);
            macros.AddKeywords(macros_keywords, Color.DarkGreen, FontStyle.Bold | FontStyle.Italic);

            string[] cottle_keywords = { "echo", "if", "for", "set", "while", "dump" };
            string[] cottle_functions = { "and", "cmp", "default", "defined", "eq", "ge", "gt", "has", "le", "lt", "ne", "not", "or", "xor", "when",
                    "abs", "add", "ceil", "cos", "div", "floor", "max", "min", "mod", "mul", "pow", "rand", "round", "sin", "sub",
                    "char", "format", "lcase", "match", "ord", "split", "token", "ucase",
                    "cast", "type", "call"
            };

            var cottle_code = dualTextBox1.SourceFormat.AddGroup("cottle", @"\{\{[^:\n]*:|\{\{[^\}:]*\}\}|\}\}", Color.Black, FontStyle.Regular);
            cottle_code.AddKeywords(cottle_keywords, Color.Blue, FontStyle.Bold);
            cottle_code.AddKeywords(cottle_functions, Color.Brown, FontStyle.Bold);

            string[] c_keywords = { "typedef", "void", "union", "static", "enum", "const", "extern", "int", "short", "char", "bool", "unsigned", "signed", "long" };
            dualTextBox1.ResultFormat.AddGroup("macro_processor", @"^#\w+", Color.Brown, FontStyle.Bold);
            dualTextBox1.ResultFormat.AddGroup("doc", @"\/\*\*(\*(?!\/)|[^*])*\*\/", Color.SteelBlue, FontStyle.Regular);
            dualTextBox1.ResultFormat.AddGroup("line_comment", @"\/\/[^\n\r]+?(?:\*\)|[\n\r])", Color.Green, FontStyle.Regular);
            dualTextBox1.ResultFormat.AddKeywords(c_keywords, Color.Blue, FontStyle.Bold);

            dualTextBox1.ProcessText += DualTextBox1_ProcessText;
            dualTextBox1.SetText(File.ReadAllText(Source));

        }

        private string DualTextBox1_ProcessText(string source)
        {
            string render = project.RenderScript(source, DestFile);
            if (render == null)
            {
                stBar.Text = project.Errors;
                int ln = project.ErrorLine - 1;
                int cl = project.ErrorColumn;
                render = new string(source.TakeWhile(ch =>
                {
                    if (ln == 0) cl--;
                    else if (ch == '\r') ln--;
                    return cl > 0;
                }).ToArray());
            }
            else
            {
                stBar.Text = "";
            }

            return render;
        }

        private void CottleEditor_Load(object sender, EventArgs e)
        {
            
        }
        private void BtSave_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Source, dualTextBox1.GetSourceText());
        }

    }
}
