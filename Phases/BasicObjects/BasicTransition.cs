using Cottle;
using Cottle.Documents;
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
    class BasicTransition : BasicObject
    {
        public override string Name => Transition.Name;
        public override string Alias => Transition.Name;

        public BasicObject Source { get; private set; }    //Start state of this transition
        public string TimerName { get; private set; }
        public BasicState Pointing { get; set; }    //Pointing state to jump to
        private SimpleDocument condition;

        public override SimulationMark SimulationMark { get => Transition.SimulationMark; set => Transition.SimulationMark = value; }

        //Primitive fields
        public SimpleTransition Transition { get; private set; }
        public List<BasicOutput> Outputs { get; private set; }

        public override IMachine Father => Source.Father;

        public BasicTransition(SimpleTransition transition, BasicObjectsTree tree)
        {
            Outputs = new List<BasicOutput>();
            Transition = transition;
            foreach (string outputAction in Transition.OutputsList)
            {
                string outputName = LexicalRules.GetOutputId(outputAction);
                OperationType operation = LexicalRules.GetOutputOperation(outputAction);
                if (Transition.OwnerDraw.OwnerSheet.OwnerBook.Variables.InternalOutputs.FirstOrDefault(output => output.Name == outputName) is IInternalOutput ioutput)
                {
                    Outputs.Add(new BasicOutput(operation, ioutput as Variable));
                }
            }
            if (Transition.StartObject is Alias alias)
                Source = tree.UsedObjects[alias.Pointing];
            else if (Transition.StartObject is StateAlias salias)
                Source = tree.UsedObjects[salias.Pointing];
            else
                Source = tree.UsedObjects[Transition.StartObject];
            if (Transition.StartObject.Father == null)
            {
                TimerName = Util.CounterName(tree.Root.Name);
            }
            else
            {
                TimerName = Util.CounterName(Transition.StartObject.Father.Name);
            }
        }

        public bool HasCondition() => !string.IsNullOrEmpty(Transition.Condition) || Transition.Timeout > 0;
        public bool HasOutputs() => Transition.OutputsList.Count > 0;

        public string GetCondition(string variablePrefix, string timeoutCounterName)
        {
            StringBuilder str = new StringBuilder();
            if(Transition.Timeout > 0)
            {
                str.Append(timeoutCounterName + " >= " + Transition.Timeout);
                if(Transition.Condition != "")
                {
                    if (Transition.TransitionTrigger == SimpleTransition.TransitionTriggerType.ConditionAndTimeout) str.Append(" && (");
                    else str.Append(" || (");
                }
            }
            else
            {
                if (Transition.Condition == "") str.Append("1");
            }
            if(Transition.Condition != "")
            {
                string condition = LexicalFormater.AddPrefixToVariables(Transition.Condition, variablePrefix);
                str.Append(CodeGeneration.GeneratorData.FixTo_C_Code(condition, variablePrefix.Length, Transition.OwnerDraw.OwnerSheet.OwnerBook.Variables.ConditionalVariables));
                if (Transition.Timeout > 0) str.Append(")");
            }
            return str.ToString();
        }

        public Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                { "Name", Name },
                { "Description", Transition.Description },
                { "Priority", Transition.Priority },
                { "Condition", Transition.Condition },
                { "Trigger", Transition.TransitionTrigger.ToString() },
                { "Timeout", Transition.Timeout },
                { "Outputs", Outputs.ConvertAll(output => (Value)output.GetDictionary()) },
                { "StartState", Transition.StartObject.Name },
                { "EndState", Pointing == null ? Transition.StartObject.Name : Pointing.Name }
            };
        }

        public void LoadConditionAndCounter()
        {
            if (Source == null) throw new Exception("Condition with unknown Source state.");
            condition = new SimpleDocument("{" + GetCondition("", TimerName) + "}");
        }

        public bool Evaluate(IStore store)
        {
            if (Transition.TransitionTrigger == SimpleTransition.TransitionTriggerType.ConditionAndTimeout
                && (Transition.Condition == "" && Transition.Timeout == 0)) return true;
            if (Transition.TransitionTrigger == SimpleTransition.TransitionTriggerType.ConditionOrTimeout
                && (Transition.Condition == "" || Transition.Timeout == 0)) return true;
            string result = condition.Render(store);
            if (result == "true" || result == "{1}" || result == "1") return true;
            return false;
        }
    }
}
