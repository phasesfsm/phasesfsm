using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Phases.Expresions;
using Phases.Variables;

namespace Phases
{
    public partial class EditCondition : Form
    {
        internal List<Variable> variables;
        private List<SyntaxToken> errors;
        private LexicalAnalyzer lexAnalyzer = new LexicalAnalyzer();
        private LexicalFormater lexFormater;
        private SyntaxAnalyzer syntaxAnalyzer;

        public EditCondition()
        {
            InitializeComponent();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            Tag = tbCondition.Text;
        }

        private void EditCondition_Load(object sender, EventArgs e)
        {
#if DEBUG
            Height = 415;
            panel1.Visible = true;
#endif
            List<string> operators = new List<string>();
            operators.Add(LexicalRules.DisposableChar.ToString());
            operators.Add(LexicalRules.GroupBeginsChar.ToString());
            operators.Add(LexicalRules.GroupEndsChar.ToString());
            operators.AddRange(LexicalRules.PrefixSymbols);
            operators.AddRange(LexicalRules.SufixSymbols);
            operators.AddRange(LexicalRules.OperatorSymbols);
            tbCondition.Operators = operators;
            tbCondition.ImageList = imageList;
        }

        private void AnalizeText()
        {
            lexAnalyzer.Source = tbCondition.Text;
            syntaxAnalyzer = new SyntaxAnalyzer(lexAnalyzer, variables);

            listLexic.Items.Clear();
            foreach(SyntaxToken token in syntaxAnalyzer.Tokens)
            {
                listLexic.Items.Add(token.ToString());
            }

            lexFormater = new LexicalFormater(lexAnalyzer);
            tbResult.Text = lexFormater.Text;
            if (syntaxAnalyzer == null) return;
            if (listMessages.Items.Count > 0) listMessages.Items.Clear();
            errors = new List<SyntaxToken>();
            foreach (SyntaxToken token in syntaxAnalyzer.Tokens)
            {
                if(token.Qualifier != SyntaxToken.Qualifiers.Correct)
                {
                    listMessages.Items.Add(token.ToString());
                    errors.Add(token);
                    if(errors.Count == 1)
                    {
                        lbMsg.Text = token.ToString();
                        lbMsg.ForeColor = Color.Red;
                    }
                }
            }
            if(errors.Count == 0)
            {
                if (syntaxAnalyzer.Tokens.Count == 0 || syntaxAnalyzer.Tokens.Last().Type == Token.Types.Id
                    || (syntaxAnalyzer.Tokens.Last().Type == Token.Types.None && (syntaxAnalyzer.Tokens.Count == 1 || syntaxAnalyzer.Tokens[syntaxAnalyzer.Tokens.Count - 2].Type == Token.Types.Id)))
                {
                    lbMsg.Text = "Ready.";
                    lbMsg.ForeColor = Color.Black;
                }
                else
                {
                    lbMsg.Text = "Incomplete condition.";
                    lbMsg.ForeColor = Color.Red;
                }
            }
            btOk.Enabled = lbMsg.ForeColor == Color.Black;
            SyntaxTree syntaxTree = new SyntaxTree();
            syntaxTree.CreateTree(syntaxAnalyzer);
        }

        private void tbCondition_TextChanged(object sender, EventArgs e)
        {
            if (btOk.Enabled) btOk.Enabled = false;
            if (listMessages.Items.Count > 0) listMessages.Items.Clear();
            AnalizeText();
        }

        private void listMessages_Click(object sender, EventArgs e)
        {
            if (listMessages.SelectedItems.Count == 0) return;
            if (listMessages.SelectedIndex >= errors.Count) return;
            tbCondition.SelectionStart = errors[listMessages.SelectedIndex].SourceIndex;
            tbCondition.SelectionLength = errors[listMessages.SelectedIndex].Text.Length;
            tbCondition.Focus();
        }

        private void EditCondition_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(tbCondition.VisibleHelp)
            {
                tbCondition.HideHelp();
                e.Cancel = true;
            }
        }

        private void tbCondition_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == '\r' && !tbCondition.VisibleHelp)
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
