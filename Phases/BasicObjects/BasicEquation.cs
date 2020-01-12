using Cottle;
using Cottle.Documents;
using Phases.DrawableObjects;
using Phases.Expresions;
using Phases.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.BasicObjects
{
    class BasicEquation : BasicObject, IBasicGlobal
    {
        public override string Name => Equation.Name;
        public override string Alias => Equation.Name;
        private SimpleDocument expression;

        public Equation Equation { get; private set; }
        public override List<DrawableObject> ObjectList => new List<DrawableObject> { Equation };

        public override SimulationMark SimulationMark { get => Equation.SimulationMark; set => Equation.SimulationMark = value; }

        public BasicEquation(Equation equation)
        {
            Equation = equation;
        }

        public IGlobal RootObject => Equation;


        public string GetExpression(string variablePrefix)
        {
            StringBuilder str = new StringBuilder();
            if (Equation.Operation != "")
            {
                string operation = LexicalFormater.AddPrefixToVariables(Equation.Operation, variablePrefix);
                str.Append(CodeGeneration.GeneratorData.FixTo_C_Code(operation, variablePrefix.Length, Equation.OwnerDraw.OwnerSheet.OwnerBook.Variables.ConditionalVariables));
            }
            return str.ToString();
        }

        public void LoadExpression()
        {
            expression = new SimpleDocument("{set " + Equation.AssignTo + " to " + GetExpression("") + "}");
        }

        public void Evaluate(IStore store)
        {
            expression.Render(store);
        }

        public Dictionary<Value, Value> GetDictionary()
        {
            return new Dictionary<Value, Value>
            {
                { "Name", Name },
                { "AssignTo", Equation.AssignTo },
                { "Operation", Equation.Operation }
            };
        }
    }
}
