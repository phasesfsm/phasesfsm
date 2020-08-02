namespace DualText
{
    partial class DualTextBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.leftLabel = new System.Windows.Forms.Label();
            this.vScrollL = new System.Windows.Forms.VScrollBar();
            this.rightLabel = new System.Windows.Forms.Label();
            this.hScrollL = new System.Windows.Forms.HScrollBar();
            this.vScrollR = new System.Windows.Forms.VScrollBar();
            this.hScrollR = new System.Windows.Forms.HScrollBar();
            this.SuspendLayout();
            // 
            // leftLabel
            // 
            this.leftLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.leftLabel.AutoSize = true;
            this.leftLabel.BackColor = System.Drawing.SystemColors.Control;
            this.leftLabel.Location = new System.Drawing.Point(115, 348);
            this.leftLabel.MaximumSize = new System.Drawing.Size(200, 16);
            this.leftLabel.MinimumSize = new System.Drawing.Size(85, 16);
            this.leftLabel.Name = "leftLabel";
            this.leftLabel.Size = new System.Drawing.Size(104, 16);
            this.leftLabel.TabIndex = 11;
            this.leftLabel.Text = "    Ln: 1    Ch: 1        ";
            this.leftLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.leftLabel.MouseEnter += new System.EventHandler(this.ScrollLabel_MouseEnter);
            this.leftLabel.Resize += new System.EventHandler(this.leftLabel_Resize);
            // 
            // vScrollL
            // 
            this.vScrollL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.vScrollL.Enabled = false;
            this.vScrollL.Location = new System.Drawing.Point(202, 0);
            this.vScrollL.Name = "vScrollL";
            this.vScrollL.Size = new System.Drawing.Size(17, 348);
            this.vScrollL.TabIndex = 9;
            this.vScrollL.ValueChanged += new System.EventHandler(this.vScrollL_ValueChanged);
            this.vScrollL.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Scrolls_KeyDown);
            this.vScrollL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Scrolls_KeyPress);
            this.vScrollL.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Scrolls_KeyUp);
            this.vScrollL.MouseEnter += new System.EventHandler(this.ScrollLabel_MouseEnter);
            this.vScrollL.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Scrolls_PreviewKeyDown);
            // 
            // rightLabel
            // 
            this.rightLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rightLabel.AutoSize = true;
            this.rightLabel.BackColor = System.Drawing.SystemColors.Control;
            this.rightLabel.Location = new System.Drawing.Point(332, 348);
            this.rightLabel.MaximumSize = new System.Drawing.Size(200, 16);
            this.rightLabel.MinimumSize = new System.Drawing.Size(85, 16);
            this.rightLabel.Name = "rightLabel";
            this.rightLabel.Size = new System.Drawing.Size(104, 16);
            this.rightLabel.TabIndex = 8;
            this.rightLabel.Text = "    Ln: 1    Ch: 1        ";
            this.rightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rightLabel.MouseEnter += new System.EventHandler(this.ScrollLabel_MouseEnter);
            this.rightLabel.Resize += new System.EventHandler(this.rightLabel_Resize);
            // 
            // hScrollL
            // 
            this.hScrollL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hScrollL.Enabled = false;
            this.hScrollL.Location = new System.Drawing.Point(0, 347);
            this.hScrollL.Name = "hScrollL";
            this.hScrollL.Size = new System.Drawing.Size(115, 17);
            this.hScrollL.TabIndex = 7;
            this.hScrollL.ValueChanged += new System.EventHandler(this.hScrollL_ValueChanged);
            this.hScrollL.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Scrolls_KeyDown);
            this.hScrollL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Scrolls_KeyPress);
            this.hScrollL.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Scrolls_KeyUp);
            this.hScrollL.MouseEnter += new System.EventHandler(this.ScrollLabel_MouseEnter);
            this.hScrollL.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Scrolls_PreviewKeyDown);
            // 
            // vScrollR
            // 
            this.vScrollR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vScrollR.Enabled = false;
            this.vScrollR.Location = new System.Drawing.Point(419, 0);
            this.vScrollR.Name = "vScrollR";
            this.vScrollR.Size = new System.Drawing.Size(17, 348);
            this.vScrollR.TabIndex = 6;
            this.vScrollR.ValueChanged += new System.EventHandler(this.vScrollR_ValueChanged);
            this.vScrollR.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Scrolls_KeyDown);
            this.vScrollR.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Scrolls_KeyPress);
            this.vScrollR.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Scrolls_KeyUp);
            this.vScrollR.MouseEnter += new System.EventHandler(this.ScrollLabel_MouseEnter);
            this.vScrollR.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Scrolls_PreviewKeyDown);
            // 
            // hScrollR
            // 
            this.hScrollR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.hScrollR.Enabled = false;
            this.hScrollR.Location = new System.Drawing.Point(219, 347);
            this.hScrollR.Name = "hScrollR";
            this.hScrollR.Size = new System.Drawing.Size(115, 17);
            this.hScrollR.TabIndex = 10;
            this.hScrollR.ValueChanged += new System.EventHandler(this.hScrollR_ValueChanged);
            this.hScrollR.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Scrolls_KeyDown);
            this.hScrollR.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Scrolls_KeyPress);
            this.hScrollR.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Scrolls_KeyUp);
            this.hScrollR.MouseEnter += new System.EventHandler(this.ScrollLabel_MouseEnter);
            this.hScrollR.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Scrolls_PreviewKeyDown);
            // 
            // DualTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.leftLabel);
            this.Controls.Add(this.vScrollL);
            this.Controls.Add(this.rightLabel);
            this.Controls.Add(this.hScrollL);
            this.Controls.Add(this.vScrollR);
            this.Controls.Add(this.hScrollR);
            this.Name = "DualTextBox";
            this.Size = new System.Drawing.Size(436, 364);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label leftLabel;
        private System.Windows.Forms.VScrollBar vScrollL;
        private System.Windows.Forms.Label rightLabel;
        private System.Windows.Forms.HScrollBar hScrollL;
        private System.Windows.Forms.VScrollBar vScrollR;
        private System.Windows.Forms.HScrollBar hScrollR;
    }
}
