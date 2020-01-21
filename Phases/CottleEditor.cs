using Phases.CodeGeneration;
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
        List<int> sectionLines = new List<int>();

        internal CottleEditor(string rootPath, string source, string destFile, RenderingContext context)
        {
            RootPath = rootPath;
            Context = context;
            Data = context.Objects.Data;
            Source = source;
            DestFile = destFile;

            InitializeComponent();

            ctbSource.Paint += CtbSource_Paint;
            ctbResult.Paint += CtbResult_Paint;
        }

        private void CtbResult_Paint(object sender, PaintEventArgs e)
        {
            //for (int index = 2; index < sectionLines.Count; index += 2)
            //{
            //    int y = ctbResult.GetPositionFromCharIndex(ctbResult.GetFirstCharIndexFromLine(sectionLines[index - 1])).Y;
            //    int height = ctbResult.GetPositionFromCharIndex(ctbResult.GetFirstCharIndexFromLine(sectionLines[index])).Y - y;
            //    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, 200, 200, 200)), e.ClipRectangle.X, y, e.ClipRectangle.Width, height);
            //}
        }

        private void CtbSource_Paint(object sender, PaintEventArgs e)
        {
            //for (int index = 2; index < sectionLines.Count; index += 2)
            //{
            //    int y = ctbSource.GetPositionFromCharIndex(ctbSource.GetFirstCharIndexFromLine(sectionLines[index - 1])).Y;
            //    int height = ctbSource.GetPositionFromCharIndex(ctbSource.GetFirstCharIndexFromLine(sectionLines[index])).Y - y;
            //    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, 200, 200, 200)), e.ClipRectangle.X, y, e.ClipRectangle.Width, height);
            //}
            // division line
            //foreach (var lineNumber in sectionLines)
            //{
            //    int y = ctbSource.GetPositionFromCharIndex(ctbSource.GetFirstCharIndexFromLine(lineNumber)).Y - 1;
            //    e.Graphics.DrawLine(Pens.Black, e.ClipRectangle.X, y, e.ClipRectangle.Right, y);
            //}
        }


        private void BtGenerate_Click(object sender, EventArgs e)
        {
            ctbSource.Text = "";
            ctbResult.Text = "";

            var project = new CodeGeneration.Interpreter.Project(Data, RootPath);

            var result = project.RenderDual(File.ReadAllText(Source), DestFile, out string input, out string output, out sectionLines);
            if (!result) return;

            FormatText(input.ToString(), ctbSource);
            FormatText(output.ToString(), ctbResult);
        }

        private void CottleEditor_Load(object sender, EventArgs e)
        {
            if (Data != null)
            {
                ctbResult.Visible = true;
                btGenerate.Enabled = true;
            }
            BtGenerate_Click(null, null);
        }

        private void RenderTemplate(RichTextBox m_rtb, string text)
        {
            Regex r = new Regex(
                @"(?<macro>" + Regex.Escape(Data.Profile.Properties.MacroBlockBegin) +
                @"(\*(?!\/)|[^*])*" + Regex.Escape(Data.Profile.Properties.MacroBlockEnd) + @")" +
                
                @"|(?<cottle>" + Regex.Escape(Data.Profile.Properties.BlockBegin) + 
                @"(\*(?!\/)|[^*])*" + Regex.Escape(Data.Profile.Properties.BlockEnd) + ")" +

                @"|(?<alt>" + Regex.Escape(Data.Profile.Properties.AltBlockBegin) +
                @"(\*(?!\/)|[^*])*" + Regex.Escape(Data.Profile.Properties.AltBlockEnd) + ")" +

                @"|(?<other>.)",
                RegexOptions.ExplicitCapture | RegexOptions.Multiline);
            var mtokens = r.Matches(text);
            int index = 0;
            foreach (Match match in mtokens)
            {
                if (match.Groups["macro"].Success)
                {
                    m_rtb.SelectionColor = Color.Brown;
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Bold);
                }
                else if (match.Groups["cottle"].Success)
                {
                    m_rtb.SelectionColor = Color.SteelBlue;
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);
                }
                else if (match.Groups["alt"].Success || match.Groups["comment_block"].Success)
                {
                    m_rtb.SelectionColor = Color.Green;
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);
                }
                else
                {
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);
                    m_rtb.SelectionColor = Color.Black;
                }
                m_rtb.SelectedText = match.Value;
                index += match.Value.Length;
            }
        }

        private void BtSave_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Source, ctbSource.Text);
        }

        void FormatText(string text, RichTextBox rtb)
        {
            RichTextBox m_rtb = new RichTextBox();
            m_rtb.Multiline = true;
            m_rtb.WordWrap = false;
            m_rtb.AcceptsTab = true;
            m_rtb.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
            m_rtb.Dock = DockStyle.Fill;
            m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);
            m_rtb.SelectionColor = Color.Black;

            Regex r = new Regex(@"(?<macro_processor>#\w+)|(?<doc>\/\*\*(\*(?!\/)|[^*])*\*\/)|(?<comment_block>\/\*(\*(?!\/)|[^*])*\*\/)|(?<line_comment>\/\/[^\n\r]+?(?:\*\)|[\n\r]))|(?<file>\"".*?\"")|(?<standar_file>\<.*?\>)|(?<string>\<.*?\>)|(?<token>\w+)|(?<other>.)",
                RegexOptions.ExplicitCapture | RegexOptions.Multiline); //
            //string[] tokens = r.Split(text);

            var mtokens = r.Matches(text);
            int index = 0;
            string[] keywords = { "public", "void", "using", "static", "class", "const", "extern", "int", "short", "char", "bool", "unsigned", "signed" };
            //string[] macros = { "#include", "#define", "#if", "#ifdef", "#ifndef", "#else", "#endif" };
            //string[] docwords = { "@brief", "@param", "@return" };

            foreach (Match match in mtokens)
            {
                m_rtb.SelectedText = match.Value;
                m_rtb.SelectionStart = index;
                m_rtb.SelectionLength = match.Value.Length;
                if (match.Groups["macro_processor"].Success)
                {
                    m_rtb.SelectionColor = Color.Brown;
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Bold);
                }
                else if (match.Groups["doc"].Success)
                {
                    m_rtb.SelectionColor = Color.SteelBlue;
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);
                }
                else if (match.Groups["line_comment"].Success || match.Groups["comment_block"].Success)
                {
                    m_rtb.SelectionColor = Color.Green;
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);
                }
                else if (match.Groups["token"].Success && keywords.Contains(match.ToString()))
                {
                    m_rtb.SelectionColor = Color.Blue;
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Bold);
                }
                else
                {
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);
                    m_rtb.SelectionColor = Color.Black;
                }
                m_rtb.SelectionStart = m_rtb.TextLength;
                index = m_rtb.TextLength;
            }
            rtb.Rtf = m_rtb.Rtf;
        }

        private void ChangingText(RichTextBox m_rtb)
        {
            // Calculate the starting position of the current line.  
            int start = 0, end = 0;
            for (start = m_rtb.SelectionStart - 1; start > 0; start--)
            {
                if (m_rtb.Text[start] == '\n') { start++; break; }
            }
            // Calculate the end position of the current line.  
            for (end = m_rtb.SelectionStart; end < m_rtb.Text.Length; end++)
            {
                if (m_rtb.Text[end] == '\n') break;
            }
            // Extract the current line that is being edited.  
            String line = m_rtb.Text.Substring(start, end - start);
            // Backup the users current selection point.  
            int selectionStart = m_rtb.SelectionStart;
            int selectionLength = m_rtb.SelectionLength;
            // Split the line into tokens.  
            Regex r = new Regex("([ \\t{}();])(?:\\w+)");
            string[] tokens = r.Split(line);
            int index = start;
            foreach (string token in tokens)
            {
                // Set the token's default color and font.  
                m_rtb.SelectionStart = index;
                m_rtb.SelectionLength = token.Length;
                m_rtb.SelectionColor = Color.Black;
                m_rtb.SelectionFont = new Font("Courier New", 10,
                FontStyle.Regular);
                // Check whether the token is a keyword.   
                String[] keywords = { "public", "void", "using", "static", "class" };
                for (int i = 0; i < keywords.Length; i++)
                {
                    if (keywords[i] == token)
                    {
                        // Apply alternative color and font to highlight keyword.   
                        m_rtb.SelectionColor = Color.Blue;
                        m_rtb.SelectionFont = new Font("Courier New", 10,
                        FontStyle.Bold);
                        break;
                    }

                    index += token.Length;
                }
                // Restore the users current selection point.   
                m_rtb.SelectionStart = selectionStart;
                m_rtb.SelectionLength = selectionLength;
            }
        }
    }
}
