using Phases.Controls;

namespace Phases
{
    partial class EditCondition
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditCondition));
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.listMessages = new System.Windows.Forms.ListBox();
            this.listLexic = new System.Windows.Forms.ListBox();
            this.binaryTree = new System.Windows.Forms.TreeView();
            this.lbMsg = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbCondition = new Phases.Controls.ExpressionBox(this.components);
            this.tbResult = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(554, 12);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 41);
            this.btOk.TabIndex = 1;
            this.btOk.Text = "Acept";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(554, 75);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 41);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
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
            this.imageList.Images.SetKeyName(8, "state.ico");
            this.imageList.Images.SetKeyName(9, "superstate.ico");
            this.imageList.Images.SetKeyName(10, "nested32.ico");
            // 
            // listMessages
            // 
            this.listMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listMessages.FormattingEnabled = true;
            this.listMessages.IntegralHeight = false;
            this.listMessages.Location = new System.Drawing.Point(0, 3);
            this.listMessages.Name = "listMessages";
            this.listMessages.Size = new System.Drawing.Size(228, 243);
            this.listMessages.TabIndex = 3;
            this.listMessages.Click += new System.EventHandler(this.listMessages_Click);
            // 
            // listLexic
            // 
            this.listLexic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listLexic.FormattingEnabled = true;
            this.listLexic.IntegralHeight = false;
            this.listLexic.Location = new System.Drawing.Point(234, 3);
            this.listLexic.Name = "listLexic";
            this.listLexic.Size = new System.Drawing.Size(192, 243);
            this.listLexic.TabIndex = 4;
            // 
            // binaryTree
            // 
            this.binaryTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.binaryTree.Location = new System.Drawing.Point(432, 3);
            this.binaryTree.Name = "binaryTree";
            this.binaryTree.Size = new System.Drawing.Size(185, 243);
            this.binaryTree.TabIndex = 8;
            // 
            // lbMsg
            // 
            this.lbMsg.AutoSize = true;
            this.lbMsg.Location = new System.Drawing.Point(12, 9);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(41, 13);
            this.lbMsg.TabIndex = 10;
            this.lbMsg.Text = "Ready.";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listMessages);
            this.panel1.Controls.Add(this.listLexic);
            this.panel1.Controls.Add(this.binaryTree);
            this.panel1.Location = new System.Drawing.Point(12, 122);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(617, 249);
            this.panel1.TabIndex = 11;
            this.panel1.Visible = false;
            // 
            // tbCondition
            // 
            this.tbCondition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCondition.Dictionary = ((System.Collections.Generic.Dictionary<string, int>)(resources.GetObject("tbCondition.Dictionary")));
            this.tbCondition.ImageList = null;
            this.tbCondition.ListBoxMaxItemsCountHeight = 4;
            this.tbCondition.Location = new System.Drawing.Point(12, 25);
            this.tbCondition.Name = "tbCondition";
            this.tbCondition.Operators = null;
            this.tbCondition.Size = new System.Drawing.Size(524, 20);
            this.tbCondition.TabIndex = 0;
            this.tbCondition.TextChanged += new System.EventHandler(this.tbCondition_TextChanged);
            this.tbCondition.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbCondition_KeyPress);
            // 
            // tbResult
            // 
            this.tbResult.Location = new System.Drawing.Point(12, 51);
            this.tbResult.Multiline = true;
            this.tbResult.Name = "tbResult";
            this.tbResult.ReadOnly = true;
            this.tbResult.Size = new System.Drawing.Size(524, 65);
            this.tbResult.TabIndex = 14;
            // 
            // EditCondition
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(641, 128);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lbMsg);
            this.Controls.Add(this.tbCondition);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.tbResult);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "EditCondition";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Condition";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditCondition_FormClosing);
            this.Load += new System.EventHandler(this.EditCondition_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        public System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ListBox listMessages;
        private System.Windows.Forms.ListBox listLexic;
        private System.Windows.Forms.TreeView binaryTree;
        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbResult;
        private ExpressionBox tbCondition;
    }
}