using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phases.DrawableObjects;

namespace Phases.Importers.StateCad
{
    class Instruction
    {
        public string Head { get; private set; }
        public int[] Parameter { get; private set; }
        public Point Point(int index) => Util.ScalePoint(new Point(Parameter[index * 2], Parameter[index * 2 + 1]), Owner.Transform);
        public string[] Text { get; private set; }
        public StateCadImporter Owner { get; private set; }

        public Instruction(string line, StateCadImporter owner)
        {
            Owner = owner;
            string[] sections = line.Split(new char[] { ' ' }, 17);
            Head = sections[0] + " " + sections[1];

            Parameter = new int[14];
            for (int i = 0; i < Parameter.Length; i++)
            {
                Parameter[i] = Convert.ToInt32(sections[i + 2]);
            }

            Text = sections[16].Split('\x02');
        }

        private int minParameter(int[] indexes)
        {
            int min = Parameter[indexes[0]];
            for(int i=1; i < indexes.Length; i++)
            {
                min = Math.Min(min, Parameter[indexes[i]]);
            }
            return min;
        }

        private int maxParameter(int[] indexes)
        {
            int max = Parameter[indexes[0]];
            for (int i = 1; i < indexes.Length; i++)
            {
                max = Math.Max(max, Parameter[indexes[i]]);
            }
            return max;
        }

        public bool GetDrawRectangle(out Rectangle rect)
        {
            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            bool res = true;
            switch (Head)
            {
                case "text add":
                    x1 = minParameter(new int[] { 0, 2 });
                    x2 = maxParameter(new int[] { 0, 2 });
                    y1 = minParameter(new int[] { 1, 3 });
                    y2 = maxParameter(new int[] { 1, 3 });
                    break;
                case "state add":
                case "transition add":
                    x1 = minParameter(new int[] { 0, 6 });
                    x2 = maxParameter(new int[] { 0, 6 });
                    y1 = minParameter(new int[] { 1, 7 });
                    y2 = maxParameter(new int[] { 1, 7 });
                    break;
                case "graphic add":
                    x1 = Parameter[0] - 5;
                    x2 = Parameter[0] + 5;
                    y1 = Parameter[1] - 5;
                    y2 = Parameter[1] + 5;
                    break;
                default:
                    res = false;
                    break;
            }
            rect = Util.GetPositiveRectangle(x1, y1, x2, y2);
            return res;
        }

        public SimpleState GetState()
        {
            var state = new SimpleState(Owner.OwnerDraw, Util.ScaleRectangle(Rectangle, Owner.Transform));
            state.Name = Text[1];
            Owner.OwnerDraw.AddObject(state);
            return state;
        }

        public Text GetText()
        {
            var text = new Text(Owner.OwnerDraw, Util.ScaleRectangle(TextRectangle, Owner.Transform));
            text.Name = Owner.OwnerDraw.OwnerSheet.NextObjectName(text.GetFormName());
            Owner.OwnerDraw.AddObject(text);
            text.Description = Text[1];
            return text;
        }

        public Equation GetEcuation()
        {
            var equation = new Equation(Owner.OwnerDraw, Util.ScaleRectangle(TextRectangle, Owner.Transform));
            equation.Name = Owner.OwnerDraw.OwnerSheet.NextObjectName(equation.GetFormName());
            Owner.OwnerDraw.AddObject(equation);
            equation.Operation = Text[1];
            equation.AssignTo = Text[3];
            return equation;
        }

        public StateAlias GetAlias()
        {
            var alias = new StateAlias(Owner.OwnerDraw, Util.ScaleRectangle(Rectangle, Owner.Transform));
            alias.Name = alias.GetFormName();
            alias.PointingTo = Text[1];
            Owner.OwnerDraw.AddObject(alias);
            return alias;
        }

        public Origin GetOrigin()
        {
            Rectangle rect = Util.GetPositiveRectangle(Parameter[0], Parameter[1], Parameter[6], 2 * Parameter[1] - Parameter[7]);
            Origin origin = new Origin(Owner.OwnerDraw, Util.Center(Util.ScaleRectangle(rect, Owner.Transform)));
            origin.Name = Owner.OwnerDraw.OwnerSheet.NextObjectName(origin.GetFormName());
            Owner.OwnerDraw.AddObject(origin);
            return origin;
        }

        public SimpleTransition GetTransition(List<Instruction> inst, DrawableObject select, Dictionary<int, DrawableObject> list)
        {
            Point[] pts = new Point[4] {
                inst[1].Point(0),
                inst[2].Point(0),
                inst[3].Point(0),
                inst[4].Point(0)
            };
            SimpleTransition trans = new SimpleTransition(Owner.OwnerDraw, pts, null);
            trans.Name = Owner.OwnerDraw.OwnerSheet.NextObjectName(trans.GetFormName());
            Owner.OwnerDraw.AddObject(trans);
            trans.StartObject = select;
            Point point = trans.StartPoint;
            trans.StartObject.Intersect(trans.StartPoint, ref point, ref trans.StartAngle);
            trans.EndObject = list[StateId];
            point = trans.EndPoint;
            trans.EndObject.Intersect(trans.EndPoint, ref point, ref trans.EndAngle);
            trans.Condition = Text[1];
            trans.OutputsList.AddRange(Text[3].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            Point textPoint = Util.Center(new Point[] { Point(0), Point(3) });
            trans.SetTextPoint(textPoint);
            return trans;
        }

        Rectangle Rectangle => Util.GetPositiveRectangle(Parameter[0], Parameter[1], Parameter[6], 2 * Parameter[1] - Parameter[7]);
        Rectangle TextRectangle => Util.GetPositiveRectangle(Parameter[0], Parameter[1], Parameter[2], 2 * Parameter[1] - Parameter[3]);
        public int StateId => Parameter[9];
        public string Name => Text[1];

    }
}
