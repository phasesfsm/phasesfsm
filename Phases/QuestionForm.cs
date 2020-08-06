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
    public partial class QuestionForm : Form
    {
        public new event EventHandler TextChanged;
        public new event KeyPressEventHandler KeyPress;

        public string Value => tbValue.Text;

        public QuestionForm(string title, string message, string value = "")
        {
            InitializeComponent();
            Text = title;
            lbMessage.Text = message;
            tbValue.Text = value;
            tbValue.TextChanged += TbValue_TextChanged;
            tbValue.KeyPress += TbValue_KeyPress;
        }

        private void TbValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPress?.Invoke(sender, e);
        }

        private void TbValue_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(sender, e);
        }
    }
}
