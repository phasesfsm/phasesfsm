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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CottleEditor));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btGenerate = new System.Windows.Forms.ToolStripButton();
            this.btSave = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.stBar = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lbNoData = new System.Windows.Forms.Label();
            this.ctbSource = new Phases.Controls.SyncTextBox();
            this.ctbResult = new Phases.Controls.SyncTextBox();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btGenerate,
            this.btSave});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btGenerate
            // 
            this.btGenerate.Enabled = false;
            this.btGenerate.Image = global::Phases.Properties.Resources.build;
            this.btGenerate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btGenerate.Name = "btGenerate";
            this.btGenerate.Size = new System.Drawing.Size(74, 22);
            this.btGenerate.Text = "Generate";
            this.btGenerate.Click += new System.EventHandler(this.BtGenerate_Click);
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // stBar
            // 
            this.stBar.Name = "stBar";
            this.stBar.Size = new System.Drawing.Size(785, 17);
            this.stBar.Spring = true;
            this.stBar.Text = "Ready.";
            this.stBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ctbSource);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ctbResult);
            this.splitContainer1.Size = new System.Drawing.Size(800, 403);
            this.splitContainer1.SplitterDistance = 398;
            this.splitContainer1.TabIndex = 4;
            // 
            // lbNoData
            // 
            this.lbNoData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbNoData.Location = new System.Drawing.Point(0, 25);
            this.lbNoData.Name = "lbNoData";
            this.lbNoData.Size = new System.Drawing.Size(800, 403);
            this.lbNoData.TabIndex = 5;
            this.lbNoData.Text = "Render preview is not available.\r\n\r\nThis could be because there is not a project " +
    "opened or because the current project has errors.";
            this.lbNoData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ctbSource
            // 
            this.ctbSource.AcceptsTab = true;
            this.ctbSource.AutoWordSelection = true;
            this.ctbSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctbSource.Font = new System.Drawing.Font("Courier New", 10F);
            this.ctbSource.HideSelection = false;
            this.ctbSource.Location = new System.Drawing.Point(0, 0);
            this.ctbSource.Name = "ctbSource";
            this.ctbSource.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.ctbSource.Size = new System.Drawing.Size(398, 403);
            this.ctbSource.SyncChild = this.ctbResult;
            this.ctbSource.TabIndex = 0;
            this.ctbSource.Text = "";
            this.ctbSource.WordWrap = false;
            // 
            // ctbResult
            // 
            this.ctbResult.AcceptsTab = true;
            this.ctbResult.AutoWordSelection = true;
            this.ctbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctbResult.Font = new System.Drawing.Font("Courier New", 10F);
            this.ctbResult.HideSelection = false;
            this.ctbResult.Location = new System.Drawing.Point(0, 0);
            this.ctbResult.Name = "ctbResult";
            this.ctbResult.ReadOnly = true;
            this.ctbResult.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.ctbResult.Size = new System.Drawing.Size(398, 403);
            this.ctbResult.SyncChild = null;
            this.ctbResult.TabIndex = 0;
            this.ctbResult.Text = "";
            this.ctbResult.WordWrap = false;
            // 
            // CottleEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.lbNoData);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CottleEditor";
            this.Text = "Cottle Templates Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.CottleEditor_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripButton btSave;
        private System.Windows.Forms.ToolStripStatusLabel stBar;
        private System.Windows.Forms.ToolStripButton btGenerate;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lbNoData;
        private Controls.SyncTextBox ctbSource;
        private Controls.SyncTextBox ctbResult;
    }
}