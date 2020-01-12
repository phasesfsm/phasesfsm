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
            //e.Graphics.DrawLine(Pens.Black, 10, 10, 100, 100);
        }

        private void CtbSource_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.DrawLine(Pens.Black, e.ClipRectangle.X, ctbSource.VerticalScroll, e.ClipRectangle.Right, ctbSource.VerticalScroll);

        }

        private void BtGenerate_Click(object sender, EventArgs e)
        {
            ctbSource.Text = "";
            ctbResult.Text = "";

            string[] lines = File.ReadAllLines(Source);
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (string line in lines)
            {
                sb.Append(index);
                sb.Append('\t');
                sb.AppendLine(line);
                index++;
            }
            var project = new CodeGeneration.Interpreter.Project(Data, RootPath);

            string render = project.RenderScript(sb.ToString(), DestFile);
            if (render == null) return;

            string[] rlines = render.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var rb = new StringBuilder();
            index = 0;
            int ridx = 0, num = 0, left;
            foreach (string line in lines)
            {
                ctbSource.SelectedText = line + Environment.NewLine;

                left = 0;
                for (int i = ridx; i < rlines.Length; i++)
                {
                    int tindex = rlines[i].IndexOf('\t');
                    if (rlines[i].Contains('\t') && int.TryParse(rlines[i].Substring(0, tindex), out int lnum))
                    {
                        if (lnum <= index)
                        {
                            rb.AppendLine(rlines[i].Substring(tindex + 1));
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
                if (index > num)
                {
                    rb.AppendLine();
                    num++;
                }
                while (left > 1)
                {
                    ctbSource.SelectedText = Environment.NewLine;
                    left--;
                }
                index++;
            }
            ctbSource.SelectionStart = 0;
            ctbResult.Text = rb.ToString();
            FormatText(rb.ToString(), ctbResult);
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
                m_rtb.SelectedText = match.Value;
                index += match.Value.Length;
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
