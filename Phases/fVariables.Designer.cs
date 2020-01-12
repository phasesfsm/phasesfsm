namespace Phases
{
    partial class fVariables
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
            System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("Booleans", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup6 = new System.Windows.Forms.ListViewGroup("Events", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fVariables));
            System.Windows.Forms.ListViewGroup listViewGroup7 = new System.Windows.Forms.ListViewGroup("Booleans", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup8 = new System.Windows.Forms.ListViewGroup("Counters", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup9 = new System.Windows.Forms.ListViewGroup("Messages", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Booleans", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Events", System.Windows.Forms.HorizontalAlignment.Left);
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabInputs = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btAddInput = new System.Windows.Forms.ToolStripDropDownButton();
            this.btAddInputBoolean = new System.Windows.Forms.ToolStripMenuItem();
            this.btAddInputEvent = new System.Windows.Forms.ToolStripMenuItem();
            this.btDeleteInput = new System.Windows.Forms.ToolStripButton();
            this.inputsList = new System.Windows.Forms.ListView();
            this.coInputName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tabFlags = new System.Windows.Forms.TabPage();
            this.flagsList = new System.Windows.Forms.ListView();
            this.coFlagName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.btAddFlag = new System.Windows.Forms.ToolStripDropDownButton();
            this.btAddFlagBoolean = new System.Windows.Forms.ToolStripMenuItem();
            this.btAddFlagCounter = new System.Windows.Forms.ToolStripMenuItem();
            this.btAddFlagMessage = new System.Windows.Forms.ToolStripMenuItem();
            this.btDeleteFlag = new System.Windows.Forms.ToolStripButton();
            this.tabOutputs = new System.Windows.Forms.TabPage();
            this.outputsList = new System.Windows.Forms.ListView();
            this.coOutputName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.btAddOutput = new System.Windows.Forms.ToolStripDropDownButton();
            this.btAddOutputBoolean = new System.Windows.Forms.ToolStripMenuItem();
            this.btAddOutputEvent = new System.Windows.Forms.ToolStripMenuItem();
            this.btDeleteOutput = new System.Windows.Forms.ToolStripButton();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabInputs.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabFlags.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.tabOutputs.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabInputs);
            this.tabControl.Controls.Add(this.tabFlags);
            this.tabControl.Controls.Add(this.tabOutputs);
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(189, 366);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabInputs
            // 
            this.tabInputs.Controls.Add(this.inputsList);
            this.tabInputs.Controls.Add(this.toolStrip1);
            this.tabInputs.Location = new System.Drawing.Point(4, 27);
            this.tabInputs.Name = "tabInputs";
            this.tabInputs.Padding = new System.Windows.Forms.Padding(3);
            this.tabInputs.Size = new System.Drawing.Size(181, 335);
            this.tabInputs.TabIndex = 0;
            this.tabInputs.Text = "Inputs";
            this.tabInputs.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btAddInput,
            this.btDeleteInput});
            this.toolStrip1.Location = new System.Drawing.Point(3, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(175, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btAddInput
            // 
            this.btAddInput.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btAddInputBoolean,
            this.btAddInputEvent});
            this.btAddInput.Image = global::Phases.Properties.Resources.add16;
            this.btAddInput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btAddInput.Name = "btAddInput";
            this.btAddInput.Size = new System.Drawing.Size(58, 22);
            this.btAddInput.Text = "Add";
            // 
            // btAddInputBoolean
            // 
            this.btAddInputBoolean.Image = global::Phases.Properties.Resources.boolean_input;
            this.btAddInputBoolean.Name = "btAddInputBoolean";
            this.btAddInputBoolean.Size = new System.Drawing.Size(117, 22);
            this.btAddInputBoolean.Text = "Boolean";
            this.btAddInputBoolean.Click += new System.EventHandler(this.btAddInputBoolean_Click);
            // 
            // btAddInputEvent
            // 
            this.btAddInputEvent.Image = global::Phases.Properties.Resources.event1;
            this.btAddInputEvent.Name = "btAddInputEvent";
            this.btAddInputEvent.Size = new System.Drawing.Size(117, 22);
            this.btAddInputEvent.Text = "Event";
            this.btAddInputEvent.Click += new System.EventHandler(this.btAddInputEvent_Click);
            // 
            // btDeleteInput
            // 
            this.btDeleteInput.Image = global::Phases.Properties.Resources.delete16;
            this.btDeleteInput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btDeleteInput.Name = "btDeleteInput";
            this.btDeleteInput.Size = new System.Drawing.Size(60, 22);
            this.btDeleteInput.Text = "Delete";
            this.btDeleteInput.Click += new System.EventHandler(this.btDeleteInput_Click);
            // 
            // inputsList
            // 
            this.inputsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.coInputName});
            this.inputsList.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup5.Header = "Booleans";
            listViewGroup5.Name = "grInputBooleans";
            listViewGroup6.Header = "Events";
            listViewGroup6.Name = "grInputEvents";
            this.inputsList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup5,
            listViewGroup6});
            this.inputsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.inputsList.HideSelection = false;
            this.inputsList.Location = new System.Drawing.Point(3, 28);
            this.inputsList.MultiSelect = false;
            this.inputsList.Name = "inputsList";
            this.inputsList.Size = new System.Drawing.Size(175, 304);
            this.inputsList.SmallImageList = this.imageList;
            this.inputsList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.inputsList.TabIndex = 0;
            this.inputsList.UseCompatibleStateImageBehavior = false;
            this.inputsList.View = System.Windows.Forms.View.Details;
            this.inputsList.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // coInputName
            // 
            this.coInputName.Text = "Name";
            this.coInputName.Width = 145;
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
            // tabFlags
            // 
            this.tabFlags.Controls.Add(this.flagsList);
            this.tabFlags.Controls.Add(this.toolStrip3);
            this.tabFlags.Location = new System.Drawing.Point(4, 27);
            this.tabFlags.Name = "tabFlags";
            this.tabFlags.Padding = new System.Windows.Forms.Padding(3);
            this.tabFlags.Size = new System.Drawing.Size(181, 335);
            this.tabFlags.TabIndex = 5;
            this.tabFlags.Text = "Flags";
            this.tabFlags.UseVisualStyleBackColor = true;
            // 
            // flagsList
            // 
            this.flagsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.coFlagName});
            this.flagsList.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup7.Header = "Booleans";
            listViewGroup7.Name = "grFlagBooleans";
            listViewGroup8.Header = "Counters";
            listViewGroup8.Name = "grFlagCounters";
            listViewGroup9.Header = "Messages";
            listViewGroup9.Name = "grFlagMessages";
            this.flagsList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup7,
            listViewGroup8,
            listViewGroup9});
            this.flagsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.flagsList.HideSelection = false;
            this.flagsList.Location = new System.Drawing.Point(3, 28);
            this.flagsList.MultiSelect = false;
            this.flagsList.Name = "flagsList";
            this.flagsList.Size = new System.Drawing.Size(175, 304);
            this.flagsList.SmallImageList = this.imageList;
            this.flagsList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.flagsList.TabIndex = 1;
            this.flagsList.UseCompatibleStateImageBehavior = false;
            this.flagsList.View = System.Windows.Forms.View.Details;
            this.flagsList.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // coFlagName
            // 
            this.coFlagName.Text = "Name";
            this.coFlagName.Width = 145;
            // 
            // toolStrip3
            // 
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btAddFlag,
            this.btDeleteFlag});
            this.toolStrip3.Location = new System.Drawing.Point(3, 3);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Size = new System.Drawing.Size(175, 25);
            this.toolStrip3.TabIndex = 0;
            this.toolStrip3.Text = "toolStrip3";
            // 
            // btAddFlag
            // 
            this.btAddFlag.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btAddFlagBoolean,
            this.btAddFlagCounter,
            this.btAddFlagMessage});
            this.btAddFlag.Image = global::Phases.Properties.Resources.add16;
            this.btAddFlag.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btAddFlag.Name = "btAddFlag";
            this.btAddFlag.Size = new System.Drawing.Size(58, 22);
            this.btAddFlag.Text = "Add";
            // 
            // btAddFlagBoolean
            // 
            this.btAddFlagBoolean.Image = global::Phases.Properties.Resources.booleanflag;
            this.btAddFlagBoolean.Name = "btAddFlagBoolean";
            this.btAddFlagBoolean.Size = new System.Drawing.Size(152, 22);
            this.btAddFlagBoolean.Text = "Boolean";
            this.btAddFlagBoolean.Click += new System.EventHandler(this.btAddFlagBoolean_Click);
            // 
            // btAddFlagCounter
            // 
            this.btAddFlagCounter.Image = global::Phases.Properties.Resources.counter;
            this.btAddFlagCounter.Name = "btAddFlagCounter";
            this.btAddFlagCounter.Size = new System.Drawing.Size(152, 22);
            this.btAddFlagCounter.Text = "Counter";
            this.btAddFlagCounter.Click += new System.EventHandler(this.btAddFlagCounter_Click);
            // 
            // btAddFlagMessage
            // 
            this.btAddFlagMessage.Image = global::Phases.Properties.Resources.trigger;
            this.btAddFlagMessage.Name = "btAddFlagMessage";
            this.btAddFlagMessage.Size = new System.Drawing.Size(152, 22);
            this.btAddFlagMessage.Text = "Message";
            this.btAddFlagMessage.Click += new System.EventHandler(this.btAddFlagMessage_Click);
            // 
            // btDeleteFlag
            // 
            this.btDeleteFlag.Image = global::Phases.Properties.Resources.delete16;
            this.btDeleteFlag.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btDeleteFlag.Name = "btDeleteFlag";
            this.btDeleteFlag.Size = new System.Drawing.Size(60, 22);
            this.btDeleteFlag.Text = "Delete";
            this.btDeleteFlag.Click += new System.EventHandler(this.btDeleteFlag_Click);
            // 
            // tabOutputs
            // 
            this.tabOutputs.Controls.Add(this.outputsList);
            this.tabOutputs.Controls.Add(this.toolStrip2);
            this.tabOutputs.Location = new System.Drawing.Point(4, 27);
            this.tabOutputs.Name = "tabOutputs";
            this.tabOutputs.Padding = new System.Windows.Forms.Padding(3);
            this.tabOutputs.Size = new System.Drawing.Size(181, 335);
            this.tabOutputs.TabIndex = 1;
            this.tabOutputs.Text = "Outputs";
            this.tabOutputs.UseVisualStyleBackColor = true;
            // 
            // outputsList
            // 
            this.outputsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.coOutputName});
            this.outputsList.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header = "Booleans";
            listViewGroup1.Name = "grOutputBooleans";
            listViewGroup2.Header = "Events";
            listViewGroup2.Name = "grOutputEvents";
            this.outputsList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.outputsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.outputsList.HideSelection = false;
            this.outputsList.Location = new System.Drawing.Point(3, 28);
            this.outputsList.MultiSelect = false;
            this.outputsList.Name = "outputsList";
            this.outputsList.Size = new System.Drawing.Size(175, 304);
            this.outputsList.SmallImageList = this.imageList;
            this.outputsList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.outputsList.TabIndex = 1;
            this.outputsList.UseCompatibleStateImageBehavior = false;
            this.outputsList.View = System.Windows.Forms.View.Details;
            this.outputsList.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // coOutputName
            // 
            this.coOutputName.Text = "Name";
            this.coOutputName.Width = 145;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btAddOutput,
            this.btDeleteOutput});
            this.toolStrip2.Location = new System.Drawing.Point(3, 3);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(175, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // btAddOutput
            // 
            this.btAddOutput.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btAddOutputBoolean,
            this.btAddOutputEvent});
            this.btAddOutput.Image = global::Phases.Properties.Resources.add16;
            this.btAddOutput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btAddOutput.Name = "btAddOutput";
            this.btAddOutput.Size = new System.Drawing.Size(58, 22);
            this.btAddOutput.Text = "Add";
            // 
            // btAddOutputBoolean
            // 
            this.btAddOutputBoolean.Image = global::Phases.Properties.Resources.boolean_output;
            this.btAddOutputBoolean.Name = "btAddOutputBoolean";
            this.btAddOutputBoolean.Size = new System.Drawing.Size(117, 22);
            this.btAddOutputBoolean.Text = "Boolean";
            this.btAddOutputBoolean.Click += new System.EventHandler(this.btAddOutputBoolean_Click);
            // 
            // btAddOutputEvent
            // 
            this.btAddOutputEvent.Image = global::Phases.Properties.Resources.event_output;
            this.btAddOutputEvent.Name = "btAddOutputEvent";
            this.btAddOutputEvent.Size = new System.Drawing.Size(117, 22);
            this.btAddOutputEvent.Text = "Event";
            this.btAddOutputEvent.Click += new System.EventHandler(this.btAddOutputEvent_Click);
            // 
            // btDeleteOutput
            // 
            this.btDeleteOutput.Image = global::Phases.Properties.Resources.delete16;
            this.btDeleteOutput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btDeleteOutput.Name = "btDeleteOutput";
            this.btDeleteOutput.Size = new System.Drawing.Size(60, 22);
            this.btDeleteOutput.Text = "Delete";
            this.btDeleteOutput.Click += new System.EventHandler(this.btDeleteOutput_Click);
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(207, 12);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(293, 366);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(344, 384);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 23);
            this.btOk.TabIndex = 2;
            this.btOk.Text = "Save";
            this.btOk.UseVisualStyleBackColor = true;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(425, 384);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 3;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // fVariables
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 419);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fVariables";
            this.Text = "Variables";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.fVariables_FormClosed);
            this.Load += new System.EventHandler(this.fVariables_Load);
            this.tabControl.ResumeLayout(false);
            this.tabInputs.ResumeLayout(false);
            this.tabInputs.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabFlags.ResumeLayout(false);
            this.tabFlags.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.tabOutputs.ResumeLayout(false);
            this.tabOutputs.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabInputs;
        private System.Windows.Forms.TabPage tabOutputs;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.TabPage tabFlags;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ListView inputsList;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ColumnHeader coInputName;
        private System.Windows.Forms.ListView outputsList;
        private System.Windows.Forms.ColumnHeader coOutputName;
        private System.Windows.Forms.ListView flagsList;
        private System.Windows.Forms.ColumnHeader coFlagName;
        private System.Windows.Forms.ToolStripDropDownButton btAddInput;
        private System.Windows.Forms.ToolStripMenuItem btAddInputBoolean;
        private System.Windows.Forms.ToolStripMenuItem btAddInputEvent;
        private System.Windows.Forms.ToolStripDropDownButton btAddOutput;
        private System.Windows.Forms.ToolStripMenuItem btAddOutputBoolean;
        private System.Windows.Forms.ToolStripMenuItem btAddOutputEvent;
        private System.Windows.Forms.ToolStripDropDownButton btAddFlag;
        private System.Windows.Forms.ToolStripMenuItem btAddFlagBoolean;
        private System.Windows.Forms.ToolStripMenuItem btAddFlagCounter;
        private System.Windows.Forms.ToolStripButton btDeleteInput;
        private System.Windows.Forms.ToolStripButton btDeleteOutput;
        private System.Windows.Forms.ToolStripButton btDeleteFlag;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        public System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ToolStripMenuItem btAddFlagMessage;
    }
}