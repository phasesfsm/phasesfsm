namespace Phases
{
    partial class ActionsViewer
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
            this.pDrawRef = new System.Windows.Forms.PictureBox();
            this.dgActions = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.pShadow = new System.Windows.Forms.PictureBox();
            this.pAfterAction = new System.Windows.Forms.PictureBox();
            this.dgObjects = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.clObject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clObjectNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clzInstance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clActionType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clShadowObjects = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clAfterObjects = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clDrawObjects = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clSelection = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clFocus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.pDrawRef)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgActions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pShadow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pAfterAction)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgObjects)).BeginInit();
            this.SuspendLayout();
            // 
            // pDrawRef
            // 
            this.pDrawRef.BackColor = System.Drawing.Color.White;
            this.pDrawRef.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pDrawRef.Location = new System.Drawing.Point(792, 25);
            this.pDrawRef.Name = "pDrawRef";
            this.pDrawRef.Size = new System.Drawing.Size(384, 330);
            this.pDrawRef.TabIndex = 0;
            this.pDrawRef.TabStop = false;
            this.pDrawRef.Paint += new System.Windows.Forms.PaintEventHandler(this.pDrawRef_Paint);
            // 
            // dgActions
            // 
            this.dgActions.AllowUserToAddRows = false;
            this.dgActions.AllowUserToDeleteRows = false;
            this.dgActions.AllowUserToResizeRows = false;
            this.dgActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dgActions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgActions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clActionType,
            this.clShadowObjects,
            this.clAfterObjects,
            this.clDrawObjects,
            this.clSelection,
            this.clFocus});
            this.dgActions.Location = new System.Drawing.Point(12, 361);
            this.dgActions.MultiSelect = false;
            this.dgActions.Name = "dgActions";
            this.dgActions.ReadOnly = true;
            this.dgActions.RowHeadersVisible = false;
            this.dgActions.Size = new System.Drawing.Size(486, 301);
            this.dgActions.TabIndex = 1;
            this.dgActions.SelectionChanged += new System.EventHandler(this.dgActions_SelectionChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(792, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "DrawRef:";
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.Location = new System.Drawing.Point(1182, 9);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(273, 653);
            this.propertyGrid1.TabIndex = 3;
            // 
            // pShadow
            // 
            this.pShadow.BackColor = System.Drawing.Color.White;
            this.pShadow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pShadow.Location = new System.Drawing.Point(12, 25);
            this.pShadow.Name = "pShadow";
            this.pShadow.Size = new System.Drawing.Size(384, 330);
            this.pShadow.TabIndex = 4;
            this.pShadow.TabStop = false;
            this.pShadow.Paint += new System.Windows.Forms.PaintEventHandler(this.pShadow_Paint);
            // 
            // pAfterAction
            // 
            this.pAfterAction.BackColor = System.Drawing.Color.White;
            this.pAfterAction.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pAfterAction.Location = new System.Drawing.Point(402, 25);
            this.pAfterAction.Name = "pAfterAction";
            this.pAfterAction.Size = new System.Drawing.Size(384, 330);
            this.pAfterAction.TabIndex = 5;
            this.pAfterAction.TabStop = false;
            this.pAfterAction.Paint += new System.Windows.Forms.PaintEventHandler(this.pAfterAction_Paint);
            // 
            // dgObjects
            // 
            this.dgObjects.AllowUserToAddRows = false;
            this.dgObjects.AllowUserToDeleteRows = false;
            this.dgObjects.AllowUserToResizeRows = false;
            this.dgObjects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgObjects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgObjects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clObject,
            this.clObjectNumber,
            this.clzInstance});
            this.dgObjects.Location = new System.Drawing.Point(504, 361);
            this.dgObjects.MultiSelect = false;
            this.dgObjects.Name = "dgObjects";
            this.dgObjects.ReadOnly = true;
            this.dgObjects.RowHeadersVisible = false;
            this.dgObjects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgObjects.Size = new System.Drawing.Size(672, 301);
            this.dgObjects.TabIndex = 6;
            this.dgObjects.SelectionChanged += new System.EventHandler(this.dgObjects_SelectionChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Shadow:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(399, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "AfterAction:";
            // 
            // clObject
            // 
            this.clObject.HeaderText = "Object";
            this.clObject.Name = "clObject";
            this.clObject.ReadOnly = true;
            // 
            // clObjectNumber
            // 
            this.clObjectNumber.HeaderText = "Number";
            this.clObjectNumber.Name = "clObjectNumber";
            this.clObjectNumber.ReadOnly = true;
            // 
            // clzInstance
            // 
            this.clzInstance.HeaderText = "zInstance";
            this.clzInstance.Name = "clzInstance";
            this.clzInstance.ReadOnly = true;
            this.clzInstance.Width = 120;
            // 
            // clActionType
            // 
            this.clActionType.HeaderText = "ActionType";
            this.clActionType.Name = "clActionType";
            this.clActionType.ReadOnly = true;
            this.clActionType.Width = 150;
            // 
            // clShadowObjects
            // 
            this.clShadowObjects.HeaderText = "Shadow";
            this.clShadowObjects.Name = "clShadowObjects";
            this.clShadowObjects.ReadOnly = true;
            this.clShadowObjects.Width = 60;
            // 
            // clAfterObjects
            // 
            this.clAfterObjects.HeaderText = "After";
            this.clAfterObjects.Name = "clAfterObjects";
            this.clAfterObjects.ReadOnly = true;
            this.clAfterObjects.Width = 60;
            // 
            // clDrawObjects
            // 
            this.clDrawObjects.HeaderText = "Draw";
            this.clDrawObjects.Name = "clDrawObjects";
            this.clDrawObjects.ReadOnly = true;
            this.clDrawObjects.Width = 60;
            // 
            // clSelection
            // 
            this.clSelection.HeaderText = "Selection";
            this.clSelection.Name = "clSelection";
            this.clSelection.ReadOnly = true;
            this.clSelection.Width = 60;
            // 
            // clFocus
            // 
            this.clFocus.HeaderText = "Focus";
            this.clFocus.Name = "clFocus";
            this.clFocus.ReadOnly = true;
            this.clFocus.Width = 60;
            // 
            // ActionsViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1467, 674);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dgObjects);
            this.Controls.Add(this.pAfterAction);
            this.Controls.Add(this.pShadow);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgActions);
            this.Controls.Add(this.pDrawRef);
            this.Name = "ActionsViewer";
            this.Text = "ActionsViewer";
            this.Load += new System.EventHandler(this.ActionsViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pDrawRef)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgActions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pShadow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pAfterAction)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgObjects)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pDrawRef;
        private System.Windows.Forms.DataGridView dgActions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.PictureBox pShadow;
        private System.Windows.Forms.PictureBox pAfterAction;
        private System.Windows.Forms.DataGridView dgObjects;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridViewTextBoxColumn clObject;
        private System.Windows.Forms.DataGridViewTextBoxColumn clObjectNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn clzInstance;
        private System.Windows.Forms.DataGridViewTextBoxColumn clActionType;
        private System.Windows.Forms.DataGridViewTextBoxColumn clShadowObjects;
        private System.Windows.Forms.DataGridViewTextBoxColumn clAfterObjects;
        private System.Windows.Forms.DataGridViewTextBoxColumn clDrawObjects;
        private System.Windows.Forms.DataGridViewTextBoxColumn clSelection;
        private System.Windows.Forms.DataGridViewTextBoxColumn clFocus;
    }
}