namespace Phases
{
    partial class DrawStateViewer
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
            this.pDraw = new System.Windows.Forms.PictureBox();
            this.pSelection = new System.Windows.Forms.PictureBox();
            this.clzInstance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clObjectNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clObject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dgShadow = new System.Windows.Forms.DataGridView();
            this.pShadow = new System.Windows.Forms.PictureBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.label1 = new System.Windows.Forms.Label();
            this.dgDraw = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgSelection = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.pDraw)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pSelection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgShadow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pShadow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDraw)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgSelection)).BeginInit();
            this.SuspendLayout();
            // 
            // pDraw
            // 
            this.pDraw.BackColor = System.Drawing.Color.White;
            this.pDraw.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pDraw.Location = new System.Drawing.Point(403, 27);
            this.pDraw.Name = "pDraw";
            this.pDraw.Size = new System.Drawing.Size(384, 330);
            this.pDraw.TabIndex = 14;
            this.pDraw.TabStop = false;
            this.pDraw.Paint += new System.Windows.Forms.PaintEventHandler(this.pDraw_Paint);
            // 
            // pSelection
            // 
            this.pSelection.BackColor = System.Drawing.Color.White;
            this.pSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pSelection.Location = new System.Drawing.Point(793, 27);
            this.pSelection.Name = "pSelection";
            this.pSelection.Size = new System.Drawing.Size(384, 330);
            this.pSelection.TabIndex = 9;
            this.pSelection.TabStop = false;
            this.pSelection.Paint += new System.Windows.Forms.PaintEventHandler(this.pSelection_Paint);
            // 
            // clzInstance
            // 
            this.clzInstance.HeaderText = "zInstance";
            this.clzInstance.Name = "clzInstance";
            this.clzInstance.ReadOnly = true;
            this.clzInstance.Width = 120;
            // 
            // clObjectNumber
            // 
            this.clObjectNumber.HeaderText = "Number";
            this.clObjectNumber.Name = "clObjectNumber";
            this.clObjectNumber.ReadOnly = true;
            // 
            // clObject
            // 
            this.clObject.HeaderText = "Object";
            this.clObject.Name = "clObject";
            this.clObject.ReadOnly = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(400, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Draw:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Shadow:";
            // 
            // dgShadow
            // 
            this.dgShadow.AllowUserToAddRows = false;
            this.dgShadow.AllowUserToDeleteRows = false;
            this.dgShadow.AllowUserToResizeRows = false;
            this.dgShadow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgShadow.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgShadow.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clObject,
            this.clObjectNumber,
            this.clzInstance});
            this.dgShadow.Location = new System.Drawing.Point(13, 363);
            this.dgShadow.MultiSelect = false;
            this.dgShadow.Name = "dgShadow";
            this.dgShadow.ReadOnly = true;
            this.dgShadow.RowHeadersVisible = false;
            this.dgShadow.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgShadow.Size = new System.Drawing.Size(384, 301);
            this.dgShadow.TabIndex = 15;
            this.dgShadow.Click += new System.EventHandler(this.dgShadow_Click);
            // 
            // pShadow
            // 
            this.pShadow.BackColor = System.Drawing.Color.White;
            this.pShadow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pShadow.Location = new System.Drawing.Point(13, 27);
            this.pShadow.Name = "pShadow";
            this.pShadow.Size = new System.Drawing.Size(384, 330);
            this.pShadow.TabIndex = 13;
            this.pShadow.TabStop = false;
            this.pShadow.Paint += new System.Windows.Forms.PaintEventHandler(this.pShadow_Paint);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.Location = new System.Drawing.Point(1183, 11);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(273, 653);
            this.propertyGrid1.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(790, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Selection:";
            // 
            // dgDraw
            // 
            this.dgDraw.AllowUserToAddRows = false;
            this.dgDraw.AllowUserToDeleteRows = false;
            this.dgDraw.AllowUserToResizeRows = false;
            this.dgDraw.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgDraw.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgDraw.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3});
            this.dgDraw.Location = new System.Drawing.Point(403, 363);
            this.dgDraw.MultiSelect = false;
            this.dgDraw.Name = "dgDraw";
            this.dgDraw.ReadOnly = true;
            this.dgDraw.RowHeadersVisible = false;
            this.dgDraw.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgDraw.Size = new System.Drawing.Size(384, 301);
            this.dgDraw.TabIndex = 18;
            this.dgDraw.Click += new System.EventHandler(this.dgDraw_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Object";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Number";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "zInstance";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 120;
            // 
            // dgSelection
            // 
            this.dgSelection.AllowUserToAddRows = false;
            this.dgSelection.AllowUserToDeleteRows = false;
            this.dgSelection.AllowUserToResizeRows = false;
            this.dgSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgSelection.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgSelection.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6});
            this.dgSelection.Location = new System.Drawing.Point(793, 363);
            this.dgSelection.MultiSelect = false;
            this.dgSelection.Name = "dgSelection";
            this.dgSelection.ReadOnly = true;
            this.dgSelection.RowHeadersVisible = false;
            this.dgSelection.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgSelection.Size = new System.Drawing.Size(384, 301);
            this.dgSelection.TabIndex = 19;
            this.dgSelection.Click += new System.EventHandler(this.dgSelection_Click);
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "Object";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "Number";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "zInstance";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            this.dataGridViewTextBoxColumn6.Width = 120;
            // 
            // DrawStateViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1467, 674);
            this.Controls.Add(this.dgSelection);
            this.Controls.Add(this.dgDraw);
            this.Controls.Add(this.pDraw);
            this.Controls.Add(this.pSelection);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dgShadow);
            this.Controls.Add(this.pShadow);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.label1);
            this.Name = "DrawStateViewer";
            this.Text = "DrawStateViewer";
            this.Load += new System.EventHandler(this.DrawStateViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pDraw)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pSelection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgShadow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pShadow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDraw)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgSelection)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pDraw;
        private System.Windows.Forms.PictureBox pSelection;
        private System.Windows.Forms.DataGridViewTextBoxColumn clzInstance;
        private System.Windows.Forms.DataGridViewTextBoxColumn clObjectNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn clObject;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dgShadow;
        private System.Windows.Forms.PictureBox pShadow;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgDraw;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridView dgSelection;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
    }
}