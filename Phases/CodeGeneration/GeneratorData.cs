using Cottle;
using Cottle.Stores;
using Phases.BasicObjects;
using Phases.DrawableObjects;
using Phases.Expresions;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phases.CodeGeneration
{
    class GeneratorData
    {
        //Lists of procesed objects
        public List<DrawingSheet> UsedSheets { get; private set; }
        public List<ModelSheet> UsedModels { get; private set; }
        public List<DrawableObject> ObjectsTable { get; private set; }
        public List<BasicRelation> RelationsList { get; private set; }
        public List<BasicEquation> EquationsList { get; private set; }

        //Data origins
        public PhasesBook Book { get; private set; }
        public CodeGenerationProfile Profile { get; private set; }
        public VariableCollection Variables => Book.Variables;

        //Objects tree
        public List<BasicObjectsTree> Trees { get; private set; }
        public List<IBasicGlobal> GlobalObjects { get; private set; }

        //List of objects
        public List<Transition> TransitionsList { get; private set; }
        public List<BasicState> BasicStatesList { get; private set; }

        //Variables cottle store
        public IStore Store { get; private set; }

        //Errors and warnings
        public List<CheckMessage> MessagesList { get; private set; }
        public int ErrorsCount => MessagesList.Count(msg => msg.MessageType == CheckMessage.MessageTypes.Error);
        public int WarningsCount => MessagesList.Count(msg => msg.MessageType == CheckMessage.MessageTypes.Warning);
        public RichTextBox Log { get; private set; }
        public int MasterCounter { get; private set; } = 1;
        public void ResetMasterCounter() => MasterCounter = 1;
        public void IncrementMasterCounter() => MasterCounter++;

        public IReadOnlyList<BasicTransition> BasicTransitionsList()
        {
            var list = new List<BasicTransition>();
            foreach(BasicObjectsTree tree in Trees)
            {
                list.AddRange(tree.BasicTransitionsList);
            }
            return list;
        }

        public IReadOnlyList<BasicState> StatesList()
        {
            return BasicStatesList.FindAll(st => !(st is BasicMachine));
        }

        public IReadOnlyList<BasicMachine> SuperStatesList()
        {
            return BasicStatesList.FindAll(st => st is BasicMachine).ConvertAll(st => st as BasicMachine);
        }

        public GeneratorData(PhasesBook book, CodeGenerationProfile profile, RichTextBox log)
        {
            Book = book;
            Profile = profile;
            Log = log;

            CheckBook();
        }

        private void CheckBook()
        {
            BuildObjectsTable();
            CheckVariablesUsage();
            if (ErrorsCount == 0 && WarningsCount == 0)
            {
                BuildBasicObjectsTrees();
                CheckObjectsInterMachines();
                Trees.ForEach(bot => MessagesList.AddRange(bot.Messages));
                if (ErrorsCount == 0 && WarningsCount == 0) BuildVariablesStore();
            }
        }

        private void BuildVariablesStore()
        {
            Store = new BuiltinStore();
            foreach(Variable var in Variables.All)
            {
                Store[var.Name] = var.Default;
            }
            foreach(BasicMachine mach in SuperStatesList())
            {
                Store[mach.Name] = 0;
            }
            foreach(BasicTransition btrans in BasicTransitionsList())
            {
                btrans.LoadConditionAndCounter();
            }
            foreach(BasicEquation beq in EquationsList)
            {
                beq.LoadExpression();
            }
        }

        private void BuildObjectsTable()
        {
            UsedSheets = new List<DrawingSheet>();
            UsedModels = new List<ModelSheet>();
            TransitionsList = new List<Transition>();
            ObjectsTable = new List<DrawableObject>();
            RelationsList = new List<BasicRelation>();
            EquationsList = new List<BasicEquation>();
            MessagesList = new List<CheckMessage>();

            //check for objects
            if (Book.MainSheet.Sketch.Objects.Count == 0)
            {
                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "The current diagram does not represent a valid state machine.", null));
                return;
            }
            //get only used objects from main sheet
            Book.GlobalSheets.ForEach(gsh => GetUsedObjects(gsh, Book.Variables));
        }

        private void CheckObjectsInterMachines()
        {
            for (int i = 0; i < Trees.Count - 1; i++)
            {
                BasicObjectsTree tree1 = Trees[i];
                for (int j = i + 1; j < Trees.Count; j++)
                {
                    BasicObjectsTree tree2 = Trees[j];
                    var list = tree1.UsedObjects.Keys.Intersect(tree2.UsedObjects.Keys);
                    foreach(DrawableObject obj in list)
                    {
                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, string.Format("Object '{0}' is shared between '{1}' and '{2}' machines.", obj.Name, tree1.Root.Name, tree2.Root.Name), obj));
                    }
                }
            }
        }

        private void CheckVariablesUsage()
        {
            LexicalAnalyzer lexAnalyzer = new LexicalAnalyzer();
            SyntaxAnalyzer syntaxAnalyzer;
            foreach (Transition trans in TransitionsList)
            {
                if (trans is SimpleTransition strans)
                {
                    lexAnalyzer.Source = strans.Condition;
                    syntaxAnalyzer = new SyntaxAnalyzer(lexAnalyzer, VariableCollection.GetConditionDictionary(trans.OwnerDraw.OwnerSheet).Keys.ToList());
                    foreach (SyntaxToken token in syntaxAnalyzer.Tokens)
                    {
                        if (token.Qualifier != SyntaxToken.Qualifiers.Correct)
                        {
                            MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, string.Format("{0}: {1}.", trans.Name, token.ToString()), trans));
                        }
                    }
                    foreach (string outputOperation in strans.OutputsList)
                    {
                        string outputName = LexicalRules.GetOutputId(outputOperation);
                        if (!trans.OwnerDraw.OwnerSheet.Variables.InternalOutputs.Exists(output => output.Name == outputName))
                        {
                            MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Output " + outputName + " is not defined in variables list.", trans));
                        }
                    }
                }
            }
            foreach (DrawableObject obj in ObjectsTable)
            {
                if (obj is State state)
                {
                    //state.EnterOutputsList.RemoveAll(str => !state.OwnerDraw.OwnerSheet.Variables.InternalOutputs.Exists(var => var.Name == LexicalAnalyzer.GetId(str)));
                    foreach (string outputOperation in state.EnterOutputsList)
                    {
                        string outputName = LexicalRules.GetOutputId(outputOperation);
                        if (!state.OwnerDraw.OwnerSheet.Variables.InternalOutputs.Exists(var => var.Name == LexicalAnalyzer.GetId(outputName)))
                        {
                            MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Output " + outputName + " is not defined in variables list.", state));
                        }
                    }
                    //state.ExitOutputsList.RemoveAll(str => !state.OwnerDraw.OwnerSheet.Variables.InternalOutputs.Exists(var => var.Name == LexicalAnalyzer.GetId(str)));
                    foreach (string outputOperation in state.ExitOutputsList)
                    {
                        string outputName = LexicalRules.GetOutputId(outputOperation);
                        if (!state.OwnerDraw.OwnerSheet.Variables.InternalOutputs.Exists(var => var.Name == LexicalAnalyzer.GetId(outputName)))
                        {
                            MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Output " + outputName + " is not defined in variables list.", state));
                        }
                    }
                }
            }
        }

        private void BuildBasicObjectsTrees()
        {
            Trees = new List<BasicObjectsTree>();
            GlobalObjects = new List<IBasicGlobal>();
            BasicStatesList = new List<BasicState>();
            foreach (IGlobal global in Book.Globals)
            {
                switch (global)
                {
                    case Origin origin:
                        if (origin.Father == null)
                        {
                            var basicObjectsTree = new BasicObjectsTree(origin.OwnerDraw.OwnerSheet, origin);
                            Trees.Add(basicObjectsTree);
                            if (!basicObjectsTree.AddStatesToList(BasicStatesList))
                            {
                                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "There is one or more states used in diferent state machines.", null));
                            }
                            GlobalObjects.Add(basicObjectsTree);
                        }
                        break;
                    case Relation indir:
                        GlobalObjects.Add(RelationsList.Find(bi => bi.Relation == indir));
                        break;
                    case Equation eq:
                        GlobalObjects.Add(EquationsList.Find(be => be.Equation == eq));
                        break;
                }
            }
            if (Trees.Count == 0)
            {
                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "There is not a valid state machine to check.", null));
            }
        }

        private void GetUsedObjects(DrawingSheet sheet, VariableCollection variables)
        {
            if (sheet is ModelSheet model) UsedModels.Add(model);
            else UsedSheets.Add(sheet);
            foreach (DrawableObject obj in sheet.Sketch.Objects)
            {
                if(obj is Relation indir)
                {
                    Variable input = VariableCollection.GetIndirectInput(sheet, indir.Trigger);
                    Variable output = VariableCollection.GetIndirectOutput(sheet, indir.Output);
                    if(input == null)
                    {
                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Relation trigger variable not found.", indir));
                    }
                    if(output == null)
                    {
                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Relation output variable not found.", indir));
                    }
                    if(input != null && output != null)
                    {
                        BasicRelation ibindir = RelationsList.FirstOrDefault(ind => ind.Input.Name == input.Name);
                        BasicRelation obindir = RelationsList.FirstOrDefault(ind => ind.Action.Output.Name == input.Name);
                        if (ibindir != null)
                        {
                            MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Relation has same input than other one.", indir, ibindir.Relation));
                        }
                        if(obindir != null)
                        {
                            MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Relation has same output than other one.", indir, obindir.Relation));
                        }
                        if(ibindir == null && obindir == null)
                        {
                            BasicOutput action = indir.Action == null ? null : new BasicOutput((OperationType)Enum.Parse(typeof(OperationType), indir.Action), output);
                            BasicRelation bindir = new BasicRelation(indir, input, action);
                            RelationsList.Add(bindir);
                        }
                    }
                }
                else if(obj is Equation eq)
                {
                    IInternalOutput output = variables.IndirectOutputs.FirstOrDefault(io => io.Name == eq.AssignTo);
                    if (output == null)
                    {
                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Relation trigger variable not found.", eq));
                    }
                    if (eq.Operation == "")
                    {
                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Equation does not have operation.", eq));
                    }
                    LexicalAnalyzer lexAnalyzer = new LexicalAnalyzer();
                    SyntaxAnalyzer syntaxAnalyzer;
                    lexAnalyzer.Source = eq.Operation;
                    syntaxAnalyzer = new SyntaxAnalyzer(lexAnalyzer, VariableCollection.GetConditionDictionary(eq.OwnerDraw.OwnerSheet).Keys.ToList());
                    foreach (SyntaxToken token in syntaxAnalyzer.Tokens)
                    {
                        if (token.Qualifier != SyntaxToken.Qualifiers.Correct)
                        {
                            MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, string.Format("{0}: {1}.", eq.Name, token.ToString()), eq));
                        }
                    }
                    EquationsList.Add(new BasicEquation(eq));
                }
                else if (obj is Transition trans)
                {
                    TransitionsList.Add(trans);
                    if (trans.StartObject == null)
                    {
                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Transition without start connection.", trans));
                    }
                    if (trans.EndObject == null)
                    {
                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Transition without end connection.", trans));
                    }
                    if (trans is SimpleTransition strans && strans.Condition == "" && strans.Timeout == 0)
                    {
                        if (strans.StartObject is Origin origin)
                        {
                            if (strans.EndObject is End || strans.EndObject is Abort)
                            {
                                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "One shot machine Transition should have a condition.", trans));
                            }
                        }
                        else if (!strans.DefaultTransition)
                        {
                            MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Transition does not have condition or timeout value.", trans));
                        }
                    }
                    else if (trans is SuperTransition sptrans && sptrans.Links == "")
                    {
                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Transition is not linking any object.", trans));
                    }
                }
                else
                {
                    ObjectsTable.Add(obj);
                    switch (obj)
                    {
                        case Origin origin:
                            if (origin.OutTransitions.Length == 0)
                            {
                                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Origin without output transition.", origin));
                            }
                            break;
                        case Nested nested:
                            if (nested.PointingTo != "")
                            {
                                if (Book.Models.Exists(sh => sh.Name == nested.PointingTo))
                                {
                                    if (!UsedModels.Contains(nested.PointedSheet))
                                    {
                                        GetUsedObjects(nested.PointedSheet, nested.PointedSheet.Variables);
                                    }
                                }
                                else
                                {
                                    MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Nested state is pointing to an unexistent sheet.", nested));
                                }
                            }
                            else
                            {
                                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Nested state is not pointing to any sheet.", nested));
                            }
                            break;
                        case Alias alias:
                            if (alias.PointingTo != "")
                            {
                                if (!sheet.Sketch.States.Exists(state => state.Name == alias.PointingTo))
                                {
                                    MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Alias state is pointing to an unexistent state.", alias));
                                }
                            }
                            else
                            {
                                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Alias state is not pointing to any state.", alias));
                            }
                            break;
                        case StateAlias alias:
                            if (alias.PointingTo != "")
                            {
                                if (!sheet.Sketch.States.Exists(state => state.Name == alias.PointingTo))
                                {
                                    MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Alias state is pointing to an unexistent state.", alias));
                                }
                            }
                            else
                            {
                                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Alias state is not pointing to any state.", alias));
                            }
                            break;
                        case SuperState sobj:
                            foreach (DrawableObject other in obj.OwnerDraw.Objects)
                            {
                                if (other is State sup && other != sobj)
                                {
                                    if (sobj.rect.IntersectsWith(sup.rect) && !(sobj.rect.Contains(sup.rect) || sup.rect.Contains(sobj.rect)))
                                    {
                                        MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Error, string.Format("Super state cannot be intersected for other state: {0} <=> {1}", sobj.Name, sup.Name), sobj, sup));
                                    }
                                }
                            }
                            break;
                        case State state:
                            if (state.OutTransitions.Count() == 0)
                            {
                                MessagesList.Add(new CheckMessage(CheckMessage.MessageTypes.Warning, string.Format("State '{0}' does not have out transitions.", state.Name), state));
                            }
                            break;
                    }
                }
            }
        }

        public void LogStateChange(string previous, string current)
        {
            Log.AppendText(string.Format("{0}: State change from ", MasterCounter));
            Log.SelectionColor = Color.Blue;
            Log.AppendText(previous);
            Log.SelectionColor = Color.Black;
            Log.AppendText(" to ");
            Log.SelectionColor = Color.Blue;
            Log.AppendText(current);
            Log.SelectionColor = Color.Black;
            Log.AppendText("." + Environment.NewLine);
        }

        public static string FixTo_C_Code(string condition, int prefixLength, List<Variable> conditionalVariables)
        {
            string[] words = condition.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == "&")
                {
                    words[i] = "&&";
                }
                else if (words[i] == "|")
                {
                    words[i] = "||";
                }
                else if (words[i].StartsWith("."))
                {
                    string name = words[i].Substring(prefixLength + 1);
                    IIntegerValue ivar = conditionalVariables.Find(var => var.Name == name) as IIntegerValue;
                    words[i] = string.Format("{0} = {1}", words[i].Substring(1), ivar.MinimumValue);
                }
                else if (words[i].StartsWith("'"))
                {
                    string name = words[i].Substring(prefixLength + 1);
                    IIntegerValue ivar = conditionalVariables.Find(var => var.Name == name) as IIntegerValue;
                    words[i] = string.Format("{0} = {1}", words[i].Substring(1), ivar.MaximumValue);
                }
            }

            return String.Join(" ", words);
        }
    }
}
