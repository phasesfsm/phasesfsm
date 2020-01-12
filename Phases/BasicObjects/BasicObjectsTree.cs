using Cottle;
using Phases.DrawableObjects;
using Phases.Expresions;
using Phases.Simulation;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    class BasicObjectsTree : BasicObject, IMachine, IBasicGlobal
    {
        public BasicRoot Root { get; private set; }
        public Dictionary<DrawableObject, BasicState> usedStates;
        public Dictionary<DrawableObject, BasicObject> UsedObjects { get; private set; }
        public List<BasicTransition> BasicTransitionsList { get; private set; }
        public IReadOnlyList<BasicState> BasicStatesList => usedStates.Values.ToList();
        public List<Variable> ConditionalVariables { get; private set; }
        public List<Variable> OutputVariables { get; private set; }
        public List<CheckMessage> Messages { get; private set; }

        public override string Name => Root.Name;
        public override string Alias => Name;
        public override SimulationMark SimulationMark { get => Root.SimulationMark; set => Root.SimulationMark = value; }
        public override List<DrawableObject> ObjectList => Root.ObjectList;
        public List<BasicState> States { get; private set; } = new List<BasicState>();
        public Origin Origin => Root.Origin;
        public BasicTransition Transition => Root.Transition;
        public List<BasicTransition> Transitions => Root.Transitions;
        public DrawingSheet Sheet { get; private set; }

        public BasicObjectsTree(DrawingSheet sheet, Origin origin)
        {
            Sheet = sheet;
            ConditionalVariables = new List<Variable>();
            OutputVariables = new List<Variable>();
            usedStates = new Dictionary<DrawableObject, BasicState>();
            UsedObjects = new Dictionary<DrawableObject, BasicObject>();
            BasicTransitionsList = new List<BasicTransition>();
            Messages = new List<CheckMessage>();
            Root = new BasicRoot(this, origin);
            UsedObjects.Add(origin, Root);
            var strans = origin.OutTransitions.First() as SimpleTransition;
            Root.Transition = AddTransition(States, strans);
        }

        public IGlobal RootObject => Root.Origin;

        public IReadOnlyList<BasicState> StatesList()
        {
            return usedStates.Values.ToList().FindAll(st => !(st is BasicMachine));
        }

        public IReadOnlyList<BasicMachine> SuperStatesList()
        {
            return usedStates.Values.ToList().FindAll(st => st is BasicMachine).ConvertAll(st => st as BasicMachine);
        }

        public bool AddStatesToList(List<BasicState> list)
        {
            if (usedStates.Values.Intersect(list).Count() > 0) return false;
            list.AddRange(usedStates.Values);
            return true;
        }

        private BasicTransition AddTransition(List<BasicState> dest, SimpleTransition trans)
        {
            var btrans = new BasicTransition(trans, this);
            UsedObjects.Add(trans, btrans);
            BasicTransitionsList.Add(btrans);
            AddVariables(trans);
            btrans.Pointing = AddObject(dest, trans.EndObject);
            if((trans.EndObject is End || trans.EndObject is Abort) && btrans.Pointing != null &&
                btrans.Pointing.State is SuperState && !btrans.Transition.StartObject.HasFather(btrans.Pointing.State as SuperState))
            {
                var link = trans.EndObject as Link;
                if (btrans.Transition.StartObject.Father != link.Father)
                {
                    Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Object cannot be pointed from other machine.", trans.EndObject));
                }
            }
            return btrans;
        }

        private BasicState AddObject(List<BasicState> dest, DrawableObject @object)
        {
            if(usedStates.TryGetValue(@object, out BasicState bobj))
            {
                return bobj;
            }
            switch (@object)
            {
                case SimpleState simpleState:
                    var bstate = AddState(dest, simpleState);
                    return bstate;
                case StateAlias stateAlias:
                    if(stateAlias.Pointing != null)
                    {
                        var sAlias = AddObject(dest, stateAlias.Pointing);
                        return sAlias;
                    }
                    Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Alias is not pointing to any object.", stateAlias));
                    return null;
                case SuperState superState:
                    var bmach = AddMachine(dest, superState);
                    return bmach;
                case Alias alias:
                    if (alias.Pointing != null)
                    {
                        var pstate = AddObject(dest, alias.Pointing);
                        return pstate;
                    }
                    Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Alias is not pointing to any object.", alias));
                    return null;
                case End end:
                    var estate = end.Father;
                    if (estate != null) return AddMachine(dest, estate);
                    return null;
                case Abort abort:
                    var astate = abort.Father;
                    if (astate != null)
                    {
                        return AddMachine(dest, astate);
                    }
                    Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Warning, "Abort is outside a super state.", abort));
                    return null;
            }
            throw new Exception("Non handled pointing object.");
        }

        private BasicState AddState(List<BasicState> dest, State state)
        {
            var bstate = new BasicState(state);
            UsedObjects.Add(state, bstate);
            usedStates.Add(state, bstate);
            AddVariables(state);
            if (state.Father == null)
            {
                dest.Add(bstate);
                bstate.SetFather(this);
            }
            else if (usedStates.TryGetValue(state.Father, out BasicState bobj))
            {
                var bmach = bobj as BasicMachine;
                bmach.States.Add(bstate);
                bstate.SetFather(bmach);
            }
            else
            {
                var bmach = AddMachine(dest, state.Father);
                bmach.States.Add(bstate);
                bstate.SetFather(bmach);
            }
            foreach (Transition trans in state.AllOutTransitions)
            {
                var btrans = AddTransition(dest, trans as SimpleTransition);
                bstate.Transitions.Add(btrans);
            }
            return bstate;
        }

        private BasicMachine AddMachine(List<BasicState> dest, SuperState superState)
        {
            if(usedStates.TryGetValue(superState, out BasicState bstate))
            {
                return bstate as BasicMachine;
            }
            var bmach = new BasicMachine(superState);
            UsedObjects.Add(superState, bmach);
            usedStates.Add(superState, bmach);
            AddVariables(superState);
            BasicTransition btrans;
            if (superState.Father == null)
            {
                dest.Add(bmach);
                bmach.SetFather(this);
            }
            else if (usedStates.TryGetValue(superState.Father, out BasicState bobj))
            {
                var bmachF = bobj as BasicMachine;
                bmachF.States.Add(bmach);
                bmach.SetFather(bmachF);
            }
            else
            {
                var bmachF = AddMachine(dest, superState.Father);
                bmachF.States.Add(bmach);
                bmach.SetFather(bmachF);
            }
            if (superState.Origin != null)
            {
                BasicRoot mroot = new BasicRoot(bmach, superState.Origin);
                bmach.Root = mroot;
                UsedObjects.Add(bmach.Origin, mroot);
                btrans = AddTransition(bmach.States, bmach.Origin.OutTransitions.FirstOrDefault() as SimpleTransition);
                mroot.Transition = btrans;
                if (btrans != null)
                {
                    bmach.Transition = btrans;
                    if (!(btrans.Transition.EndObject is Link) && !btrans.Pointing.State.HasFather(bmach.State as SuperState))
                    {
                        Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Origin is not pointing to state inside the same super state.", bmach.Origin));
                    }
                }
                else
                {
                    Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Origin does not have a transition.", bmach.Origin));
                }
            }
            else if (bmach.HasInputs())
            {
                Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Super state with incoming transitions must have an origin.", superState));
            }
            foreach (Transition trans in superState.AllOutTransitions)
            {
                btrans = AddTransition(dest, trans as SimpleTransition);
                bmach.Transitions.Add(btrans);
            }
            return bmach;
        }

        public void AddVariables(SimpleTransition trans)
        {
            var lex = new LexicalAnalyzer();
            lex.Source = trans.Condition;
            Variable variable;
            foreach (Token token in lex.Tokens)
            {
                if (token.Type == Token.Types.Id && !ConditionalVariables.Exists(var => var.Name == token.Text))
                {
                    variable = Sheet.OwnerBook.Variables.All.FirstOrDefault(var => var.Name == token.Text);
                    if (variable == null)
                    {
                        Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Not found condition variable: " + token.Text, trans));
                    }
                    else if (!ConditionalVariables.Contains(variable))
                    {
                        ConditionalVariables.Add(variable);
                    }
                }
            }
            foreach (string output in trans.OutputsList)
            {
                string outputName = LexicalAnalyzer.GetOutputId(output);
                variable = Sheet.OwnerBook.Variables.All.FirstOrDefault(var => var.Name == outputName);
                if (variable == null)
                {
                    Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Not found output variable: " + outputName, trans));
                }
                else if (!OutputVariables.Contains(variable))
                {
                    OutputVariables.Add(variable);
                }
            }
        }

        public void AddVariables(State state)
        {
            Variable variable;
            string outputName;
            foreach (string output in state.EnterOutputsList)
            {
                outputName = LexicalAnalyzer.GetOutputId(output);
                variable = Sheet.OwnerBook.Variables.All.FirstOrDefault(var => var.Name == outputName);
                if (variable == null)
                {
                    Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Not found output variable: " + outputName, state));
                }
                else if (!OutputVariables.Contains(variable))
                {
                    OutputVariables.Add(variable);
                }
            }
            foreach (string output in state.ExitOutputsList)
            {
                outputName = LexicalAnalyzer.GetOutputId(output);
                variable = Sheet.OwnerBook.Variables.All.FirstOrDefault(var => var.Name == outputName);
                if (variable == null)
                {
                    Messages.Add(new CheckMessage(CheckMessage.MessageTypes.Error, "Not found output variable: " + outputName, state));
                }
                else if (!OutputVariables.Contains(variable))
                {
                    OutputVariables.Add(variable);
                }
            }
        }

        public Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                { "Name", Name },
                { "Description", Origin.Description },
                { "Transition", Transition.GetDictionary() },
                { "SuperStates",  SuperStatesList().ToList().ConvertAll(state => (Value)state.Name) },
                { "States",  StatesList().ToList().ConvertAll(state => (Value)state.Name) }
            };
        }

        public bool HasFirstPriority()
        {
            return ((IMachine)Root).HasFirstPriority();
        }

        public bool HasLastPriority()
        {
            return ((IMachine)Root).HasLastPriority();
        }

        public bool HasEntryOutputs() => false;
        public bool HasExitOutputs() => false;
    }
}
