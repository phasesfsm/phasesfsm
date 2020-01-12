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
    public partial class CottleConfigForm : Form
    {
        public CottleConfigForm()
        {
            InitializeComponent();
        }

        private void CottleConfigForm_Load(object sender, EventArgs e)
        {
            if (languagesList.Items.Count == 0) btAccept.Enabled = false;
            else languagesList.SelectedIndex = 0;
        }
    }
}
