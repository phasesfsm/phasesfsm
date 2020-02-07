using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Phases.DrawableObjects;
using Phases.Variables;
using Phases.Actions;
using Phases.Importers;
using Phases.Importers.StateCad;
using Phases.Utils;
using System.IO;

namespace Phases
{
    class PhasesBook: IDisposable
    {
        public const string ChildSheetNamePrefix = "Sheet";
        public const int FileVersion = 2;

        private AppInterface controls;
        private List<DrawingSheet> sheets;
        public List<IGlobal> Globals { get; private set; }
        private VariableCollection variables;
        public List<RecordableAction> Actions;
        private int ActionIndex = 0, SavedIndex = 0;
        public string Language { get; set; } = "";
        public string ScriptsFolder { get; set; } = "";
        public string TargetLanguage { get; set; } = "";
        public string ExecAfter { get; set; } = "";
        private Protected Protected;

        public PhasesBook(AppInterface appInterface, Size size) //, MouseTool mouseTool)
        {
            variables = new VariableCollection(this);
            sheets = new List<DrawingSheet>();
            Actions = new List<RecordableAction>();
            Globals = new List<IGlobal>();
            defaultNewSheetSize = size;
            activeSheet = new DrawingSheet(this, "Main Sheet", defaultNewSheetSize, Constants.ImageIndex.Sheet);
            controls = appInterface;
            Protected = new Protected();

            //controls.mouse.ClearSelection();
            controls.view.Nodes.Clear();
            AddSheet(activeSheet);
        }

        public VariableCollection Variables
        {
            get
            {
                return variables;
            }
        }

        public List<DrawingSheet> Sheets
        {
            get
            {
                return sheets;
            }
        }

        public DrawingSheet MainSheet
        {
            get
            {
                return sheets[0];
            }
        }

        private DrawingSheet activeSheet;
        public DrawingSheet SelectedSheet
        {
            get
            {
                return activeSheet;
            }
            set
            {
                if (!sheets.Contains(value)) throw new Exception("Non founded sheet.");
                activeSheet = value;
            }
        }

        private Size defaultNewSheetSize;
        public Size DefaultNewSheetSize
        {
            get
            {
                return defaultNewSheetSize;
            }
            set
            {
                defaultNewSheetSize = value;
            }
        }

        private void AddSheet(DrawingSheet sheet)
        {
            sheet.sheetTree.Tag = sheet;
            controls.view.Nodes.Add(sheet.sheetTree);
            sheets.Add(sheet);
        }

        public DrawingSheet CreateChildSheet()
        {
            string sheetName = NextChildSheetName(ChildSheetNamePrefix);
            var sheet = new DrawingSheet(this, sheetName, defaultNewSheetSize);
            AddSheet(sheet);
            //Actions.Add(new SheetAction(RecordableAction.ActionTypes.AddSheet, sheet.Serialize()));
            //ActionIndex++;
            return sheet;
        }

        public void DeleteActiveChildSheet()
        {
            //Actions.Add(new SheetAction(RecordableAction.ActionTypes.DeleteSheet, SelectedSheet.Serialize()));
            //ActionIndex++;
            controls.view.Nodes.Remove(SelectedSheet.sheetTree);
            sheets.Remove(SelectedSheet);
            controls.view.SelectedNode = controls.view.Nodes[0];
        }

        private string NextChildSheetName(string prefix)
        {
            int i = 1;
            while (sheets.Exists(sh => sh.Name == prefix + i)) i++;
            return prefix + i;
        }

        public string NextObjectName(string prefix)
        {
            int i = 1;
            while (sheets.Exists(sh => sh.draw.Objects.Exists(obj => obj.Name == prefix + i)))
            {
                i++;
            }
            return prefix + i;
        }

        public string NextObjectName(string prefix, List<DrawableObject> list)
        {
            int i = 1;
            while (sheets.Exists(sh => sh.draw.Objects.Exists(obj => obj.Name == prefix + i)) || list.Exists(obj => obj.Name == prefix + i))
            {
                i++;
            }
            return prefix + i;
        }

        public bool ExistsName(string name)
        {
            if (Variables.All.Exists(obj => obj.Name == name)) return true;
            foreach(DrawingSheet sheet in Sheets)
            {
                if (sheet.draw.Objects.Exists(obj => obj.Name == name)) return true;
            }
            return false;
        }

        public List<DrawableObject> GetFullObjectsList()
        {
            var list = new List<DrawableObject>();
            foreach (DrawingSheet sheet in Sheets)
            {
                list.AddRange(sheet.draw.Objects);
            }
            return list;
        }

        #region "Actions undo/redo"

        public void VariablesChanged(byte[] before)
        {
            VariablesAction action = new VariablesAction(RecordableAction.ActionTypes.VariablesChanged, before, Variables.Serialize());
            Actions.Add(action);
            ActionIndex++;
        }

        public string UndoText()
        {
            if (ActionIndex > 0)
            {
                return "Undo " + RecordableAction.ActionName(Actions[ActionIndex - 1].ActionType);
            }
            else
            {
                return "Undo";
            }
        }

        public string RedoText()
        {
            if (ActionIndex < Actions.Count)
            {
                return "Redo " + RecordableAction.ActionName(Actions[ActionIndex].ActionType);
            }
            else
            {
                return "Redo";
            }
        }

        public bool IsSavedAction()
        {
            return SavedIndex == ActionIndex;
        }

        public void MarkSavedAction()
        {
            SavedIndex = ActionIndex;
        }

        //Add an action to the actions collection
        public void AddDrawAction(RecordableAction.ActionTypes actionType, DrawingSheet sheet, List<DrawableObject> changingObjects, List<DrawableObject> selectedObjects, int focusIndex)
        {
            //Erase redo actions
            if (ActionIndex < Actions.Count)
            {
                if (SavedIndex > ActionIndex) SavedIndex = -1;
                Actions.RemoveRange(ActionIndex, Actions.Count - ActionIndex);
            }
            //Create the action object
            var action = new DrawAction(actionType, sheet, changingObjects, selectedObjects, focusIndex);
            sheet.draw.PerformAction(action);
            //Add new action
            Actions.Add(action);
            ActionIndex++;
        }

        public bool Undo(MouseTool mouse)
        {
            ActionIndex--;
            RecordableAction action = Actions[ActionIndex];
            int index = 0;
            if (action is DrawAction daction)
            {
                controls.view.SelectedNode = daction.Sheet.sheetTree;
                SelectedSheet.draw.Undo(mouse, daction);
            }
            else if (action is VariablesAction vaction)
            {
                Variables.Deserialize(vaction.Before, ref index);
            }
            else if(action is SheetAction saction)
            {
                if (saction.ActionType == RecordableAction.ActionTypes.AddSheet)
                {
                    
                }
                else if (saction.ActionType == RecordableAction.ActionTypes.DeleteSheet)
                {
                    
                }
                else
                {
                    throw new Exception("Unknown sheet action to undo.");
                }
            }
            else
            {
                throw new Exception("Not supported action to undo.");
            }
            return ActionIndex > 0;
        }

        public bool Redo(MouseTool mouse)
        {
            RecordableAction action = Actions[ActionIndex];
            int index = 0;
            if (action is DrawAction daction)
            {
                SelectedSheet = daction.Sheet;
                SelectedSheet.draw.Redo(mouse, daction);
            }
            else if (action is VariablesAction vaction)
            {
                Variables.Deserialize(vaction.After, ref index);
            }
            else if (action is SheetAction saction)
            {
                if (saction.ActionType == RecordableAction.ActionTypes.AddSheet)
                {

                }
                else if (saction.ActionType == RecordableAction.ActionTypes.DeleteSheet)
                {

                }
                else
                {
                    throw new Exception("Unknown sheet action to redo.");
                }
            }
            else
            {
                throw new Exception("Not supported action to redo.");
            }
            ActionIndex++;
            return ActionIndex < Actions.Count;
        }

        #endregion

        #region "Serialization"

        public byte[] Serialize()
        {
            var data = new List<byte>();
            data.Add(Serialization.Token.StartFile);
            data.AddRange(Serialization.SerializeParameter(FileVersion));
            data.Add(Serialization.Token.CodeGenerationLanguage);
            data.AddRange(Serialization.SerializeParameter(Language));
            data.Add(Serialization.Token.CodeGenerationFolder);
            data.AddRange(Serialization.SerializeParameter(ScriptsFolder));
            data.Add(Serialization.Token.CodeGenerationExec);
            data.AddRange(Serialization.SerializeParameter(ExecAfter));

            //Document information
            data.Add(Serialization.Token.StartBookInformation);
            //Parameters here
            data.Add(Serialization.Token.EndBookInformation);

            //Serialize book variables
            data.AddRange(variables.Serialize());

            //Serialize sheets
            foreach (DrawingSheet sheet in sheets)
            {
                data.AddRange(sheet.Serialize());
            }

            //Serialize global objects priorities
            data.Add(Serialization.Token.StartGlobalPriorityList);
            foreach (IGlobal ig in Globals)
            {
                data.Add(Serialization.Token.ObjectName);
                data.AddRange(Serialization.SerializeParameter(ig.Name));
            }
            data.Add(Serialization.Token.EndGlobalPriorityList);

            data.Add(Serialization.Token.EndFile);

            //Compress
            var zdata = new List<byte>(Protected.Encrypt(Util.Compress(data.ToArray()), "phafsm"));
            zdata.Insert(0, Serialization.Token.CompressedData);

            //Encrypt
            //var edata = new List<byte>(Protected.Protect(zdata.ToArray()));
            //edata.Insert(0, Serialization.Token.EncryptedCompressedFile);

            return zdata.ToArray();
        }

        public bool Deserialize(byte[] data)
        {
            if (data.Length < 7) return false;
            if (sheets.Count != 1) return false;
            DrawingSheet sheet;
            int index = 0, fileVersion = 0, idx = 0;
            string str = "";

            if (data[index] == Serialization.Token.EncryptedCompressedFile)
            {
                //remove first
                var temp = new byte[data.Length - 1];
                Array.Copy(data, 1, temp, 0, temp.Length);

                //De-encrypt
                data = Protected.Unprotect(temp);

                //De-compress
                data = Util.Decompress(data);
            }
            else if (data[index] == Serialization.Token.CompressedData)
            {
                //remove first
                var temp = new byte[data.Length - 1];
                Array.Copy(data, 1, temp, 0, temp.Length);

                //De-compress
                data = Util.Decompress(Protected.Decrypt(temp, "phafsm"));
            }

            //File and file version
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.StartFile)) return false;
            if (!Serialization.DeserializeParameter(data, ref index, ref fileVersion)) return false;
            switch (fileVersion)
            {
                case 1:
                    break;
                case 2:
                    idx = index;
                    if (Serialization.Token.Deserialize(data, ref index, Serialization.Token.CodeGenerationLanguage))
                    {
                        if (!Serialization.DeserializeParameter(data, ref index, ref str)) return false;
                        Language = str;
                    }
                    else
                    {
                        index = idx;
                        Language = "";
                    }
                    idx = index;
                    if (Serialization.Token.Deserialize(data, ref index, Serialization.Token.CodeGenerationFolder))
                    {
                        if (!Serialization.DeserializeParameter(data, ref index, ref str)) return false;
                        ScriptsFolder = str;
                        string folderName = Path.GetFileName(str);
                        if (folderName.EndsWith(".cottle"))
                        {
                            TargetLanguage = folderName.Substring(0, folderName.Length - 7);
                        }
                        else
                        {
                            TargetLanguage = folderName;
                        }
                    }
                    else
                    {
                        index = idx;
                        ScriptsFolder = "";
                    }
                    idx = index;
                    if (Serialization.Token.Deserialize(data, ref index, Serialization.Token.CodeGenerationExec))
                    {
                        if (!Serialization.DeserializeParameter(data, ref index, ref str)) return false;
                        ExecAfter = "";
                    }
                    else
                    {
                        index = idx;
                        ExecAfter = "";
                    }
                    break;
                default:
                    return false;
            }

            //Document information
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.StartBookInformation)) return false;
            //End document information
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndBookInformation)) return false;

            //Deserialize variables
            if (!variables.Deserialize(data, ref index)) return false;

            //Main sheet definition
            if (!Serialization.Token.Is(data, index, Serialization.Token.StartSheetDefinition)) return false;
            if(!activeSheet.Deserialize(data, ref index)) return false;

            //Sheets definitions
            while (Serialization.Token.Is(data, index, Serialization.Token.StartSheetDefinition))
            {
                sheet = CreateChildSheet();
                if (!sheet.Deserialize(data, ref index)) return false;
            }

            //Global objects priorities
            if (Serialization.Token.Deserialize(data, ref index, Serialization.Token.StartGlobalPriorityList))
            {
                var list = GetFullObjectsList();
                var prioList = new List<IGlobal>();
                string name = "";
                while (Serialization.Token.Deserialize(data, ref index, Serialization.Token.ObjectName))
                {
                    Serialization.DeserializeParameter(data, ref index, ref name);
                    if (!list.Exists(obj => obj.Name == name)) return false;
                    var gobj = list.Find(obj => obj.Name == name) as IGlobal;
                    if (!prioList.Contains(gobj))
                    {
                        prioList.Add(gobj);
                    }
                }
                if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndGlobalPriorityList)) return false;
                Globals = prioList;
            }

            return Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndFile);
        }

        public bool ImportStateCADFile(byte[] data)
        {
            for(int i=0; i< data.Length; i++)
            {
                data[i] ^= 0x80;
            }
            StateCadImporter importer = new StateCadImporter(Encoding.Default.GetString(data), MainSheet.draw, 4f);
            Dictionary<int, DrawableObject> list = new Dictionary<int, DrawableObject>();

            Rectangle sheet = importer.DrawArea;
            sheet.Inflate(400, 400);
            MainSheet.Size = Util.ScaleSize(sheet.Size, 4f);
            DrawableObject select = null;

            foreach (Instruction inst in importer.Instructions)
            {
                switch (inst.Head)
                {
                    case "variable add":
                        if(inst.Parameter[10] == 1) //Input
                        {
                            Variables.AddVariable(new BooleanInput(inst.Name));
                        }
                        else if (inst.Parameter[10] == 2) //Output
                        {
                            Variables.AddVariable(new BooleanOutput(inst.Name));
                        }
                        else if (inst.Parameter[10] == 6) //Flag
                        {
                            Variables.AddVariable(new BooleanFlag(inst.Name));
                        }
                        break;
                    case "state add":
                        if (inst.Parameter[11] == 0) //Origin
                        {
                            list.Add(inst.StateId, inst.GetOrigin());
                        }
                        else if(inst.Parameter[11] == 224) //State
                        {
                            list.Add(inst.StateId, inst.GetState());
                        }
                        else if (inst.Parameter[11] == 176) //Alias
                        {
                            list.Add(inst.StateId, inst.GetAlias());
                        }
                        break;
                    case "state select":
                        select = list[inst.StateId];
                        break;
                    case "transition add":
                        int idx = importer.Instructions.IndexOf(inst);
                        inst.GetTransition(importer.Instructions.GetRange(idx, 5), select, list);
                        break;
                    case "text add":
                        if (inst.Text[3] == "")
                        {
                            inst.GetText();
                        }
                        else
                        {
                            inst.GetEcuation();
                        }
                        break;
                }
            }

            foreach (Instruction inst in importer.Instructions)
            {
                switch (inst.Head)
                {
                    case "state add":
                        if (inst.Parameter[11] == 176) //Alias
                        {
                            var salias = list[inst.StateId] as StateAlias;
                            salias.PointingTo = inst.Text[1];
                        }
                        break;
                }
            }
            return true;
        }

        public void Dispose()
        {
            foreach(DrawingSheet sheet in sheets)
            {
                sheet.Dispose();
            }
        }

        #endregion
    }
}
