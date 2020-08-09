namespace Phases
{
    partial class CottleEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CottleEditor));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btSave = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.stBar = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbNoData = new System.Windows.Forms.Label();
            this.dualTextBox1 = new DualText.DualTextBox(this.components);
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.lbFileName = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbScriptName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btSave});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(964, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btSave
            // 
            this.btSave.Image = global::Phases.Properties.Resources.save_as;
            this.btSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(51, 22);
            this.btSave.Text = "Save";
            this.btSave.Click += new System.EventHandler(this.BtSave_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 488);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(964, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // stBar
            // 
            this.stBar.Name = "stBar";
            this.stBar.Size = new System.Drawing.Size(949, 17);
            this.stBar.Spring = true;
            this.stBar.Text = "Ready.";
            this.stBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbNoData
            // 
            this.lbNoData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbNoData.Location = new System.Drawing.Point(0, 47);
            this.lbNoData.Name = "lbNoData";
            this.lbNoData.Size = new System.Drawing.Size(964, 441);
            this.lbNoData.TabIndex = 5;
            this.lbNoData.Text = "Render preview is not available.\r\n\r\nThis could be because there is not a project " +
    "opened or because the current project has errors.";
            this.lbNoData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dualTextBox1
            // 
            this.dualTextBox1.BackColor = System.Drawing.Color.White;
            this.dualTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dualTextBox1.Location = new System.Drawing.Point(0, 47);
            this.dualTextBox1.Name = "dualTextBox1";
            this.dualTextBox1.Size = new System.Drawing.Size(964, 441);
            this.dualTextBox1.TabIndex = 6;
            // 
            // statusStrip2
            // 
            this.statusStrip2.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbScriptName,
            this.lbFileName});
            this.statusStrip2.Location = new System.Drawing.Point(0, 25);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(964, 22);
            this.statusStrip2.SizingGrip = false;
            this.statusStrip2.TabIndex = 7;
            this.statusStrip2.Text = "statusStrip2";
            // 
            // lbFileName
            // 
            this.lbFileName.AutoSize = false;
            this.lbFileName.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lbFileName.Name = "lbFileName";
            this.lbFileName.Size = new System.Drawing.Size(480, 17);
            this.lbFileName.Text = "[FileName]";
            // 
            // lbScriptName
            // 
            this.lbScriptName.AutoSize = false;
            this.lbScriptName.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lbScriptName.Name = "lbScriptName";
            this.lbScriptName.Size = new System.Drawing.Size(480, 17);
            this.lbScriptName.Text = "[ScriptName]";
            // 
            // CottleEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 510);
            this.Controls.Add(this.dualTextBox1);
            this.Controls.Add(this.lbNoData);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.statusStrip2);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CottleEditor";
            this.Text = "Cottle Templates Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.CottleEditor_Load);
            this.Resize += new System.EventHandler(this.CottleEditor_Resize);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripButton btSave;
        private System.Windows.Forms.ToolStripStatusLabel stBar;
        private System.Windows.Forms.Label lbNoData;
        private DualText.DualTextBox dualTextBox1;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel lbScriptName;
        private System.Windows.Forms.ToolStripStatusLabel lbFileName;
    }
}