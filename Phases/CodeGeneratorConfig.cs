﻿using Phases.BasicObjects;
using Phases.CodeGeneration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Phases.CodeGeneration.CodeGeneratorProperties;

namespace Phases
{
    public partial class CodeGeneratorConfig : Form
    {
        private GeneratorData gData;
        private CodeGeneratorProperties gProps;
        private string rootPath;

        internal CodeGeneratorConfig(GeneratorData generatorData, string configPath = "")
        {
            gData = generatorData;
            InitializeComponent();
            if (!string.IsNullOrEmpty(configPath))
            {
                string filePath = Path.Combine(configPath, "cottle.ini");
                if (File.Exists(filePath)) LoadConfiguration(filePath);
            }
            treeView.NodeMouseClick += (sender, args) => treeView.SelectedNode = args.Node;
        }

        private void BtOpenFolder_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadConfiguration(openFileDialog.FileName);
            }
        }

        private class NodeTag
        {
            public List<RenderingContext> Renderings { get; set; }
            public string FilePath { get; set; }

            public NodeTag(string filePath, RenderingContext rendering)
            {
                FilePath = filePath;
                Renderings = new List<RenderingContext>();
                Renderings.Add(rendering);
            }

            public NodeTag(string filePath, List<RenderingContext> renderings)
            {
                FilePath = filePath;
                Renderings = renderings;
            }

            public RenderingContext First()
            {
                return Renderings.First();
            }
        }

        private void LoadConfiguration()
        {
            if (gData != null) gData.Profile.Properties = gProps;
            propertyGrid1.SelectedObject = gProps;

            var folderName = Path.GetFileName(rootPath);
            var rootNode = new TreeNode(folderName, 0, 0);
            treeView.Nodes.Clear();
            treeView.Nodes.Add(rootNode);
            string iniPath = Path.Combine(rootPath, "cottle.ini");
            rootNode.Tag = new NodeTag(rootPath, new RenderingContext(rootPath, ContextLevel.Project, gData, rootPath));
            rootNode.Nodes.Add("cottle.ini", "cottle.ini", 2, 2).Tag = new NodeTag(iniPath, new RenderingContext(iniPath, ContextLevel.NoContext, gData, "cottle.ini"));
            rootNode.ExpandAll();
        }

        private void LoadConfiguration(string path)
        {
            gProps = new CodeGeneratorProperties(path);
            if (gData != null) gData.Profile.Properties = gProps;
            propertyGrid1.SelectedObject = gProps;

            string scriptsFolder = Path.GetDirectoryName(path);
            rootPath = scriptsFolder;
            var folderName = Path.GetFileName(scriptsFolder);
            var rootNode = new TreeNode(folderName, 0, 0);
            rootNode.Tag = new NodeTag(scriptsFolder, new RenderingContext(scriptsFolder, ContextLevel.Project, gData, scriptsFolder));
            treeView.Nodes.Clear();
            treeView.Nodes.Add(rootNode);

            ProccessDirs(scriptsFolder, rootNode);
            foreach (string filePath in Directory.EnumerateFiles(scriptsFolder))
            {
                string fileName = Path.GetFileName(filePath);
                string fileExt = Path.GetExtension(filePath);
                if (fileExt == ".pha")
                {
                    continue;
                }
                else if (fileName == "cottle.ini")
                {
                    rootNode.Nodes.Add(fileName, fileName, 2, 2).Tag = new NodeTag(filePath, new RenderingContext(filePath, ContextLevel.NoContext, gData, fileName));
                }
                else
                {
                    ProccessScript(filePath, rootNode);
                }
            }
            rootNode.ExpandAll();
        }

        private void ProccessDirs(string scriptsPath, TreeNode treeNode)
        {
            foreach (string subdir in Directory.GetDirectories(scriptsPath))
            {
                string dirName = Path.GetFileName(subdir);
                var newNode = new TreeNode(dirName, 0, 0);

                if (newNode.Text.Contains(gProps.MacroBegin))
                {
                    newNode.Tag = new NodeTag(subdir, gProps.RenderMacroDirectory(dirName, (treeNode.Tag as NodeTag).First()));
                }
                else
                {
                    newNode.Tag = new NodeTag(subdir, new RenderingContext(subdir, dirName, (treeNode.Tag as NodeTag).First()));
                }
                treeNode.Nodes.Add(newNode);
                ProccessDirs(subdir, newNode);
                foreach (string filePath in Directory.GetFiles(subdir))
                {
                    string fileName = Path.GetFileName(filePath);
                    ProccessScript(filePath, newNode);
                }
            }
        }

        private void ProccessScript(string file, TreeNode treeNode)
        {
            var fileName = Path.GetFileName(file);
            var newNode = treeNode.Nodes.Add(fileName, fileName, 1, 1);

            newNode.Tag = new NodeTag(file, gProps.RenderMacroFile(fileName, (treeNode.Tag as NodeTag).Renderings));
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btEditCottle.Enabled = false;
            NodeTag tag = e.Node.Tag as NodeTag;
            stBar.Text = tag.FilePath;
            listView.Items.Clear();
            listView.Groups.Clear();
            btAddFile.Enabled = true;
            btAddFolder.Enabled = true;
            if (e.Node == treeView.Nodes[0] || e.Node.Text == "cottle.ini")
            {
                listView.Visible = false;
                btDelete.Enabled = false;
            }
            else
            {
                btDelete.Enabled = true;
                listView.Visible = true;
                if (gData != null)
                {
                    Dictionary<string, ListViewGroup> groups = new Dictionary<string, ListViewGroup>();
                    if (tag.First().Level.HasFlag(ContextLevel.Machine))
                    {
                        foreach (BasicObjectsTree machine in gData.Trees)
                        {
                            groups.Add(machine.Name, listView.Groups.Add(machine.Name, machine.Name));
                        }
                    }
                    foreach (RenderingContext context in tag.Renderings)
                    {
                        ListViewItem item;
                        if (tag.First().Level.HasFlag(ContextLevel.Machine))
                            item = new ListViewItem(context.Value, e.Node.ImageIndex, groups[context.Objects.Machine.Name]);
                        else
                            item = new ListViewItem(context.Value, e.Node.ImageIndex);
                        listView.Items.Add(item);
                        item.SubItems.Add(context.Level.ToString());
                        item.Tag = context;
                        //item.Tag = new RenderingContext(context.Value, context);
                    }
                }
                else
                {
                    var item = listView.Items.Add(tag.First().Value, e.Node.ImageIndex);
                    item.SubItems.Add(tag.Renderings.First().Level.ToString());
                    item.Tag = tag.Renderings.First();
                }
            }
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            btEditCottle.Enabled = !(listView.SelectedItems.Count == 0);
        }

        private void BtEditCottle_Click(object sender, EventArgs e)
        {
            RenderingContext context = listView.SelectedItems[0].Tag as RenderingContext;
            if (context.Objects.Data == null)
            {
                MessageBox.Show("Please save the current project or open an exiting project to get a code generation context.",
                    "Cannot edit file without example state machine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string source = stBar.Text;
            CottleEditor cottleEditor = new CottleEditor(rootPath, source, listView.SelectedItems[0].Text, context);
            cottleEditor.ShowDialog();
            cottleEditor.Dispose();
        }

        private void btNewConfig_Click(object sender, EventArgs e)
        {
            if (gData == null)
            {
                SaveFileDialog folderBrowser = new SaveFileDialog()
                {
                    ValidateNames = false,
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "Folder Selection."
                };

                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = Path.GetDirectoryName(folderBrowser.FileName);
                    CreateNewConfig(selectedPath);
                }
            }
            else
            {
                CreateNewConfig(gData.Profile.Path);
            }
        }

        private void CreateNewConfig(string selectedPath)
        {
            CreateConfig frm = new CreateConfig();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                gProps = frm.Properties;
                rootPath = Path.Combine(selectedPath, string.Format("{0}.cottle", frm.ConfigName));
                Directory.CreateDirectory(rootPath);
                btSaveIni_Click(null, null);
                LoadConfiguration();
            }
        }

        private void btSaveIni_Click(object sender, EventArgs e)
        {
            StringBuilder st = new StringBuilder();

            // Declaring and intializing object of Type 
            Type objType = typeof(CodeGeneratorProperties);

            // using GetProperties() Method 
            PropertyInfo[] type = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo propertyInfo in type)
            {
                st.AppendFormat("{0}={1}{2}", propertyInfo.Name, propertyInfo.GetValue(gProps), Environment.NewLine);
            }

            File.WriteAllText(Path.Combine(rootPath, "cottle.ini"), st.ToString());
            btSaveIni.Enabled = false;
        }

        private void btAddFolder_Click(object sender, EventArgs e)
        {
            QuestionForm frm = new QuestionForm("Create folder", "Enter the new folder name:");
            frm.KeyPress += Frm_KeyPress;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                string dirName = frm.Value;

                NodeTag tag = treeView.SelectedNode.Tag as NodeTag;
                TreeNode treeNode;
                string path;
                if (treeView.SelectedNode.ImageIndex != 0)  //file
                {
                    path = Path.GetDirectoryName(tag.FilePath);
                    treeNode = treeView.SelectedNode.Parent;
                }
                else    // folder
                {
                    path = tag.FilePath;
                    treeNode = treeView.SelectedNode;
                }
                string subdir = Path.Combine(path, dirName);
                Directory.CreateDirectory(subdir);
                var newNode = new TreeNode(frm.Value, 0, 0);
                if (newNode.Text.Contains(gProps.MacroBegin))
                {
                    newNode.Tag = new NodeTag(subdir, gProps.RenderMacroDirectory(dirName, (treeNode.Tag as NodeTag).First()));
                }
                else
                {
                    newNode.Tag = new NodeTag(subdir, new RenderingContext(subdir, dirName, (treeNode.Tag as NodeTag).First()));
                }
                treeNode.Nodes.Add(newNode);
                treeView.SelectedNode = newNode;
            }
            frm.Dispose();
        }

        private void Frm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\b') return;
            if (e.KeyChar == ' ' || Path.GetInvalidFileNameChars().Any(ch => ch == e.KeyChar))
            {
                e.KeyChar = '\0';
            }
        }

        private void btAddFile_Click(object sender, EventArgs e)
        {
            QuestionForm frm = new QuestionForm("Create file", "Enter the new file name:");
            frm.KeyPress += Frm_KeyPress;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                string fileName = frm.Value;

                NodeTag tag = treeView.SelectedNode.Tag as NodeTag;
                TreeNode treeNode;
                string path;
                if (treeView.SelectedNode.ImageIndex != 0)  //file
                {
                    path = Path.GetDirectoryName(tag.FilePath);
                    treeNode = treeView.SelectedNode.Parent;
                }
                else    // folder
                {
                    path = tag.FilePath;
                    treeNode = treeView.SelectedNode;
                }
                string subdir = Path.Combine(path, fileName);
                File.WriteAllText(subdir, "");
                var newNode = new TreeNode(frm.Value, 1, 1);
                newNode.Tag = new NodeTag(subdir, gProps.RenderMacroFile(fileName, (treeNode.Tag as NodeTag).Renderings));
                treeNode.Nodes.Add(newNode);
                treeView.SelectedNode = newNode;
            }
            frm.Dispose();
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This action cannot be reverted, delete the file?", "Delete file", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                NodeTag tag = treeView.SelectedNode.Tag as NodeTag;
                if (treeView.SelectedNode.ImageIndex == 0)  //folder
                {
                    Directory.Delete(tag.FilePath, true);
                }
                else
                {
                    File.Delete(tag.FilePath);
                }
                treeView.SelectedNode.Remove();
            }
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            TreeNode treeNode = treeView.SelectedNode;
            NodeTag tag = treeNode.Tag as NodeTag;

            if (string.IsNullOrEmpty(e.Label) || e.Label.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                e.CancelEdit = true;
                return;
            }

            string path = Path.GetDirectoryName(tag.FilePath);
            string newName = Path.Combine(path, e.Label);
            
            if (treeView.SelectedNode.ImageIndex != 0)  //file
            {
                try
                {
                    File.Move(tag.FilePath, newName);
                }
                catch
                {
                    e.CancelEdit = true;
                    return;
                }
            }
            else    // folder
            {
                try
                {
                    Directory.Move(tag.FilePath, newName);
                }
                catch
                {
                    e.CancelEdit = true;
                    return;
                }
            }
            tag.FilePath = newName;
            tag.Renderings = gProps.RenderMacroFile(e.Label, (treeNode.Parent.Tag as NodeTag).Renderings);
            TreeView_AfterSelect(treeView, new TreeViewEventArgs(e.Node));
        }

        private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Level == 0 || e.Node.Text == "cottle.ini") e.CancelEdit = true;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView.SelectedNode.BeginEdit();
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level == 0 || treeView.SelectedNode.Text == "cottle.ini") e.Cancel = true;
        }

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;
            BtEditCottle_Click(btEditCottle, new EventArgs());
        }

        private void treeView_DoubleClick(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null || listView.Items.Count == 0) return;
            if (listView.SelectedItems.Count == 0) listView.Items[0].Selected = true;
            if (btEditCottle.Enabled) BtEditCottle_Click(btEditCottle, new EventArgs());
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            btSaveIni.Enabled = true;
        }
    }
}
