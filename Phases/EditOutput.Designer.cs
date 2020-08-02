namespace Phases
{
    partial class EditOutput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditOutput));
            this.listAll = new System.Windows.Forms.ListBox();
            this.listOut = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioIncrement = new System.Windows.Forms.RadioButton();
            this.radioDecrement = new System.Windows.Forms.RadioButton();
            this.radioClear = new System.Windows.Forms.RadioButton();
            this.radioMin = new System.Windows.Forms.RadioButton();
            this.radioToggle = new System.Windows.Forms.RadioButton();
            this.radioMax = new System.Windows.Forms.RadioButton();
            this.radioSend = new System.Windows.Forms.RadioButton();
            this.radioFalse = new System.Windows.Forms.RadioButton();
            this.radioTrue = new System.Windows.Forms.RadioButton();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btRemove = new System.Windows.Forms.Button();
            this.btAdd = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listAll
            // 
            this.listAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listAll.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listAll.FormattingEnabled = true;
            this.listAll.IntegralHeight = false;
            this.listAll.ItemHeight = 18;
            this.listAll.Location = new System.Drawing.Point(12, 12);
            this.listAll.Name = "listAll";
            this.listAll.Size = new System.Drawing.Size(190, 291);
            this.listAll.TabIndex = 0;
            this.listAll.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listAll_DrawItem);
            this.listAll.DoubleClick += new System.EventHandler(this.listAll_DoubleClick);
            // 
            // listOut
            // 
            this.listOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listOut.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listOut.FormattingEnabled = true;
            this.listOut.IntegralHeight = false;
            this.listOut.ItemHeight = 18;
            this.listOut.Location = new System.Drawing.Point(266, 12);
            this.listOut.Name = "listOut";
            this.listOut.Size = new System.Drawing.Size(185, 291);
            this.listOut.TabIndex = 1;
            this.listOut.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listOut_DrawItem);
            this.listOut.SelectedIndexChanged += new System.EventHandler(this.listOut_SelectedIndexChanged);
            this.listOut.DoubleClick += new System.EventHandler(this.listOut_DoubleClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioIncrement);
            this.groupBox1.Controls.Add(this.radioDecrement);
            this.groupBox1.Controls.Add(this.radioClear);
            this.groupBox1.Controls.Add(this.radioMin);
            this.groupBox1.Controls.Add(this.radioToggle);
            this.groupBox1.Controls.Add(this.radioMax);
            this.groupBox1.Controls.Add(this.radioSend);
            this.groupBox1.Controls.Add(this.radioFalse);
            this.groupBox1.Controls.Add(this.radioTrue);
            this.groupBox1.Location = new System.Drawing.Point(457, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(165, 244);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Modifier";
            // 
            // radioIncrement
            // 
            this.radioIncrement.AutoSize = true;
            this.radioIncrement.Location = new System.Drawing.Point(21, 189);
            this.radioIncrement.Name = "radioIncrement";
            this.radioIncrement.Size = new System.Drawing.Size(72, 17);
            this.radioIncrement.TabIndex = 8;
            this.radioIncrement.TabStop = true;
            this.radioIncrement.Text = "Increment";
            this.radioIncrement.UseVisualStyleBackColor = true;
            this.radioIncrement.Click += new System.EventHandler(this.Options_Click);
            // 
            // radioDecrement
            // 
            this.radioDecrement.AutoSize = true;
            this.radioDecrement.Location = new System.Drawing.Point(21, 212);
            this.radioDecrement.Name = "radioDecrement";
            this.radioDecrement.Size = new System.Drawing.Size(77, 17);
            this.radioDecrement.TabIndex = 7;
            this.radioDecrement.TabStop = true;
            this.radioDecrement.Text = "Decrement";
            this.radioDecrement.UseVisualStyleBackColor = true;
            this.radioDecrement.Click += new System.EventHandler(this.Options_Click);
            // 
            // radioClear
            // 
            this.radioClear.AutoSize = true;
            this.radioClear.Location = new System.Drawing.Point(21, 120);
            this.radioClear.Name = "radioClear";
            this.radioClear.Size = new System.Drawing.Size(49, 17);
            this.radioClear.TabIndex = 6;
            this.radioClear.TabStop = true;
            this.radioClear.Text = "Clear";
            this.radioClear.UseVisualStyleBackColor = true;
            this.radioClear.Click += new System.EventHandler(this.Options_Click);
            // 
            // radioMin
            // 
            this.radioMin.AutoSize = true;
            this.radioMin.Location = new System.Drawing.Point(21, 166);
            this.radioMin.Name = "radioMin";
            this.radioMin.Size = new System.Drawing.Size(66, 17);
            this.radioMin.TabIndex = 5;
            this.radioMin.TabStop = true;
            this.radioMin.Text = "Minimum";
            this.radioMin.UseVisualStyleBackColor = true;
            this.radioMin.Click += new System.EventHandler(this.Options_Click);
            // 
            // radioToggle
            // 
            this.radioToggle.AutoSize = true;
            this.radioToggle.Location = new System.Drawing.Point(21, 97);
            this.radioToggle.Name = "radioToggle";
            this.radioToggle.Size = new System.Drawing.Size(58, 17);
            this.radioToggle.TabIndex = 4;
            this.radioToggle.TabStop = true;
            this.radioToggle.Text = "Toggle";
            this.radioToggle.UseVisualStyleBackColor = true;
            this.radioToggle.Click += new System.EventHandler(this.Options_Click);
            // 
            // radioMax
            // 
            this.radioMax.AutoSize = true;
            this.radioMax.Location = new System.Drawing.Point(21, 143);
            this.radioMax.Name = "radioMax";
            this.radioMax.Size = new System.Drawing.Size(69, 17);
            this.radioMax.TabIndex = 3;
            this.radioMax.TabStop = true;
            this.radioMax.Text = "Maximum";
            this.radioMax.UseVisualStyleBackColor = true;
            this.radioMax.Click += new System.EventHandler(this.Options_Click);
            // 
            // radioSend
            // 
            this.radioSend.AutoSize = true;
            this.radioSend.Location = new System.Drawing.Point(21, 28);
            this.radioSend.Name = "radioSend";
            this.radioSend.Size = new System.Drawing.Size(53, 17);
            this.radioSend.TabIndex = 2;
            this.radioSend.TabStop = true;
            this.radioSend.Text = "Send.";
            this.radioSend.UseVisualStyleBackColor = true;
            this.radioSend.Click += new System.EventHandler(this.Options_Click);
            // 
            // radioFalse
            // 
            this.radioFalse.AutoSize = true;
            this.radioFalse.Location = new System.Drawing.Point(21, 74);
            this.radioFalse.Name = "radioFalse";
            this.radioFalse.Size = new System.Drawing.Size(105, 17);
            this.radioFalse.TabIndex = 1;
            this.radioFalse.TabStop = true;
            this.radioFalse.Text = "Change to False.";
            this.radioFalse.UseVisualStyleBackColor = true;
            this.radioFalse.Click += new System.EventHandler(this.Options_Click);
            // 
            // radioTrue
            // 
            this.radioTrue.AutoSize = true;
            this.radioTrue.Location = new System.Drawing.Point(21, 51);
            this.radioTrue.Name = "radioTrue";
            this.radioTrue.Size = new System.Drawing.Size(102, 17);
            this.radioTrue.TabIndex = 0;
            this.radioTrue.TabStop = true;
            this.radioTrue.Text = "Change to True.";
            this.radioTrue.UseVisualStyleBackColor = true;
            this.radioTrue.Click += new System.EventHandler(this.Options_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(466, 282);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 23);
            this.btOk.TabIndex = 3;
            this.btOk.Text = "Save";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(547, 282);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 4;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btRemove
            // 
            this.btRemove.Image = global::Phases.Properties.Resources.Left_32;
            this.btRemove.Location = new System.Drawing.Point(208, 86);
            this.btRemove.Name = "btRemove";
            this.btRemove.Size = new System.Drawing.Size(52, 43);
            this.btRemove.TabIndex = 6;
            this.btRemove.UseVisualStyleBackColor = true;
            this.btRemove.Click += new System.EventHandler(this.btRemove_Click);
            // 
            // btAdd
            // 
            this.btAdd.Image = global::Phases.Properties.Resources.Right_32;
            this.btAdd.Location = new System.Drawing.Point(208, 37);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(52, 43);
            this.btAdd.TabIndex = 5;
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "boolean_input.png");
            this.imageList.Images.SetKeyName(1, "event.png");
            this.imageList.Images.SetKeyName(2, "trigger.png");
            this.imageList.Images.SetKeyName(3, "boolean_output.png");
            this.imageList.Images.SetKeyName(4, "event_output.png");
            this.imageList.Images.SetKeyName(5, "booleanflag.png");
            this.imageList.Images.SetKeyName(6, "flipflop.png");
            this.imageList.Images.SetKeyName(7, "counter.png");
            // 
            // EditOutput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 317);
            this.Controls.Add(this.btRemove);
            this.Controls.Add(this.btAdd);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listOut);
            this.Controls.Add(this.listAll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "EditOutput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Outputs";
            this.Load += new System.EventHandler(this.EditOutput_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listAll;
        private System.Windows.Forms.ListBox listOut;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioMin;
        private System.Windows.Forms.RadioButton radioToggle;
        private System.Windows.Forms.RadioButton radioMax;
        private System.Windows.Forms.RadioButton radioFalse;
        private System.Windows.Forms.RadioButton radioTrue;
        private System.Windows.Forms.RadioButton radioClear;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Button btRemove;
        public System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.RadioButton radioIncrement;
        private System.Windows.Forms.RadioButton radioDecrement;
        private System.Windows.Forms.RadioButton radioSend;
    }
}