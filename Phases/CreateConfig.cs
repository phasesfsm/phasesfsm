using Phases.CodeGeneration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phases
{
    public partial class CreateConfig : Form
    {
        internal CodeGeneratorProperties Properties { get; private set; }
        internal string ConfigName => tbName.Text;

        public CreateConfig()
        {
            Properties = new CodeGeneratorProperties();
            InitializeComponent();
        }

        private void rbDefault_CheckedChanged(object sender, EventArgs e)
        {
            Properties = new CodeGeneratorProperties();
        }

        private void rbC_CheckedChanged(object sender, EventArgs e)
        {
            Properties = new CodeGeneratorProperties()
            {
                BlockBegin = "/*@",
                BlockContinue = "//@",
                BlockEnd = "//*/",

                AltBlockBegin = "{{",
                AltBlockContinue = "|>",
                AltBlockEnd = "}}",
                EnableAlt = true,

                And = "&&",
                Or = "||",
                Xor = "^",

                True = "1",
                False = "0"
            };
        }

        private void rbCpp_CheckedChanged(object sender, EventArgs e)
        {
            Properties = new CodeGeneratorProperties()
            {
                BlockBegin = "/*@",
                BlockContinue = "//@",
                BlockEnd = "//*/",

                AltBlockBegin = "{{",
                AltBlockContinue = "|>",
                AltBlockEnd = "}}",
                EnableAlt = true,

                And = "&&",
                Or = "||",
                Xor = "^",

                True = "true",
                False = "false"
            };
        }

        private void rbJson_CheckedChanged(object sender, EventArgs e)
        {
            Properties = new CodeGeneratorProperties()
            {
                BlockBegin = "\"@",
                BlockContinue = "\">",
                BlockEnd = "@\"",

                AltBlockBegin = "{",
                AltBlockContinue = "|>",
                AltBlockEnd = "}",
                EnableAlt = true,

                And = "&",
                Or = "|",
                Xor = "^",

                True = "True",
                False = "False"
            };
        }

        private void rbPython_CheckedChanged(object sender, EventArgs e)
        {
            Properties = new CodeGeneratorProperties()
            {
                BlockBegin = "#@{",
                BlockContinue = "#@>",
                BlockEnd = "#@}",

                AltBlockBegin = "{{",
                AltBlockContinue = "|>",
                AltBlockEnd = "}}",
                EnableAlt = true,

                And = "and",
                Or = "or",
                Xor = "xor",

                True = "True",
                False = "False"
            };
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            btCreate.Enabled = tbName.TextLength > 0;
        }
    }
}
