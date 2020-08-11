using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.ComponentModel;
using System.Linq;
using Phases.Simulation;
using System.Drawing.Drawing2D;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Phases.DrawableObjects
{
    public struct DrawAttributes
    {
        public Pen Pen { get; private set; }
        public bool IsShadow { get; set; }
        public float Scale { get; private set; }

        public DrawAttributes(Pen pen, float scale, bool isShadow = false)
        {
            Pen = pen;
            Scale = scale;
            IsShadow = isShadow;
        }
    }

    abstract class DrawableObject : ICloneable
    {
        public enum ObjectType : byte
        {
            NoObject,
            SimpleTransition,
            Origin,
            Abort,
            SimpleState,
            SubDiagram,
            End,
            Alias,
            SuperState,
            SuperTransition,
            Nested,
            Relation,
            Text,
            StateAlias,
            Equation
        }

        internal virtual List<Transition> outTransitions { get; private set; }

        internal void FixTransitionsPriorities()
        {
            outTransitions = outTransitions.OrderBy(trans => trans.SavedPriority).ToList();
        }

        internal List<Transition> inTransitions;
        public static Font font = new Font("Arial", 10f);
        public TreeNode Node;
        [Browsable(false)]
        public DrawableCollection OwnerDraw { get; private set; }
        protected virtual StringFormat TextFormat { get; } = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        private static int objCount = 0;
        private int objNumber;
#if !DEBUG
        [Browsable(false)]
#endif
        public int ObjNumber
        {
            get
            {
                return objNumber;
            }
        }

        public abstract Point Center { get; }

        protected static void ChangeStartObject(Transition trans, DrawableObject value)
        {
            if (trans.StartObject != null && trans.StartObject != value)
            {
                trans.StartObject.outTransitions.Remove(trans);
            }
            if (value != null)
            {
                if (!(value is Origin) && value.outTransitions.Count > 0 && value.outTransitions.Last() is SimpleTransition strans && strans.DefaultTransition)
                {
                    value.outTransitions.Insert(value.outTransitions.Count - 1, trans);
                }
                else
                {
                    value.outTransitions.Add(trans);
                }
                if (value is Alias alias && alias.Pointing != null) alias.AliasOutTransitions.Add(trans);
                else if (value is StateAlias salias && salias.Pointing != null) salias.AliasOutTransitions.Add(trans);
            }
        }

        protected static void ChangeEndObject(Transition trans, DrawableObject value)
        {
            if (trans.EndObject != null && trans.EndObject != value)
            {
                trans.EndObject.inTransitions.Remove(trans);
            }
            if (value != null)
            {
                value.inTransitions.Add(trans);
            }
        }

        public override string ToString()
        {
            return name;
        }

        protected void CreateNode()
        {
            Node.ImageIndex = Constants.ImageIndex.Get(this);
            Node.SelectedImageIndex = Constants.ImageIndex.Get(this);
            Node.Tag = this;
            Node.Text = name;
        }

        private static int instanceCount = 0;
        private int newInstanceNumber()
        {
            int val = instanceCount;
            instanceCount++;
            return val;
        }

        private int instanceNumber;
#if !DEBUG
        [Browsable(false)]
#endif
        public String zInstance
        {
            get
            {
                return "0x" + instanceNumber.ToString("X").PadLeft(8, '0');
            }
        }

        public DrawableObject(DrawableCollection ownerDraw)
        {
            OwnerDraw = ownerDraw;
            instanceNumber = newInstanceNumber();
            name = "";
            description = "";
            inTransitions = new List<Transition>();
            outTransitions = new List<Transition>();
            objNumber = objCount;
            objCount++;
            Node = new TreeNode();
            CreateNode();
        }

        protected DrawableObject(DrawableCollection ownerDraw, string _name, string _description)
        {
            OwnerDraw = ownerDraw;
            instanceNumber = newInstanceNumber();
            name = _name;
            description = _description;
            inTransitions = new List<Transition>();
            outTransitions = new List<Transition>();
            objNumber = objCount;
            objCount++;
            Node = new TreeNode();
            CreateNode();
        }

#if !DEBUG
        [Browsable(false)]
#endif
        [Category("Transitions")]
        public Transition[] InTransitions
        {
            get
            {
                return inTransitions.ToArray();
            }
        }

#if !DEBUG
        [Browsable(false)]
#endif
        [Category("Transitions")]
        public Transition[] OutTransitions
        {
            get
            {
                return outTransitions.ToArray();
            }
        }

        public override bool Equals(object obj)
        {
            return ((DrawableObject)obj).objNumber == objNumber;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [Browsable(false)]
        public abstract SuperState Father { get; }

        public abstract bool HasFather(SuperState machine);

        protected abstract void DrawForm(Graphics g, DrawAttributes att);
        protected abstract void DrawText(Graphics g, Brush brush);

        [Browsable(false)]
        public virtual Pen DrawPen => Pens.Black;

        [Browsable(false)]
        public virtual Brush TextBrush => Brushes.Black;

        public virtual void Draw(Graphics g, float scale)
        {
            DrawForm(g, new DrawAttributes(DrawPen, scale, false));
            DrawText(g, TextBrush);
        }

        public virtual void DrawBack(Graphics g)
        {
            if (SimulationMark != SimulationMark.None)
            {
                DrawSimulationMark(g);
            }
        }

        public virtual void DrawSelectionBack(Graphics g) { }
        public virtual void DrawSimulationMark(Graphics g) { }

#if !DEBUG
        [Browsable(false)]
#endif
        public virtual SimulationMark SimulationMark { get; set; } = SimulationMark.None;

        public abstract List<MouseTool.SelectionRectangle> DrawSelection(Graphics g, bool focused);

        public virtual void DrawShadow(Graphics g, DrawAttributes att)
        {
            DrawForm(g, att);
        }

        public abstract void DrawingRectangle(Point startPoint, Point endPoint);

        public abstract void Move(Point offset);
        public virtual void Moved() { }

        public virtual void MoveText(Point offset) { }

        public virtual void ResizeCheck(ref Point offset, MouseTool.ResizingTypes dir) { }

        public virtual void Resize(Point offset, MouseTool.ResizingTypes dir) { }

        public virtual Point OutDir(Point position, out double angle)
        {
            angle = 0;
            return position;
        }

        public virtual void Intersect(Point position, ref Point point, ref double angle)
        {
            point = position;
        }

        protected string name;    //Name of the object
        [DisplayName("(Name)"), Description("The object name."), Category("General")]
        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                if(value != GetFormName())
                {
                    if(!Util.IsValidName(value))
                    {
                        MessageBox.Show(string.Format("Invalid name: {0}.", value), "Value error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    else if (ObjectsTypeNames.ContainsValue(value))
                    {
                        MessageBox.Show(string.Format("'{0}' is a reserved name.", value), "Value error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    else if (OwnerDraw.OwnerSheet.ExistsName(value))
                    {
                        MessageBox.Show(string.Format("The name '{0}' already exists.", value), "Value error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                Node.Text = value;
                name = value;
            }
        }

        [Browsable(false)]
        public virtual string Text
        {
            get
            {
                return name;
            }
        }

        protected string description; //Coments about the transition
        [Description("Object description."), Category("General"), Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public virtual string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        public virtual bool IsTextSelectable(Point position)
        {
            return false;
        }

        public abstract bool IsSelectablePoint(Point position);

        public abstract bool IsCovered(Rectangle area);

        public abstract bool IsSelectable(Rectangle area);

        public abstract Rectangle GetSelectionRectangle();

        public static readonly Dictionary<Type, string> ObjectsTypeNames = new Dictionary<Type, string>()
        {
            { typeof(SimpleTransition), "Transition"        },
            { typeof(SimpleState),      "State"             },
            { typeof(Origin),           "Origin"            },
            { typeof(Alias),            "Alias"             },
            { typeof(End),              "End"               },
            { typeof(Abort),            "Abort"             },
            { typeof(SuperState),       "SuperState"        },
            { typeof(SuperTransition),  "Transition"        },
            { typeof(Nested),           "Nested"            },
            { typeof(Relation),      "Relation"       },
            { typeof(Text),             "Text"              },
            { typeof(StateAlias),       "Alias"             },
            { typeof(Equation),         "Equation"          }
        };

        public static string GetFormName(Type @object)
        {
            string name;
            if (ObjectsTypeNames.TryGetValue(@object, out name)) return name;
            throw new Exception("Unhandled name.");
        }

        public string GetFormName()
        {
            string name;
            if (ObjectsTypeNames.TryGetValue(GetType(), out name)) return name;
            throw new Exception("Unhandled name.");
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public virtual DrawableObject GetClone()
        {
            var clone = (DrawableObject)Clone();
            clone.instanceNumber = newInstanceNumber();
            return clone;
        }

        public virtual void CopyTo(DrawableObject obj)
        {
            obj.name = name;
            obj.description = description;
            obj.outTransitions = new List<Transition>(outTransitions);
        }

        public abstract byte[] SerializeSpecifics();

        public byte[] SerializeObjectId()
        {
            return BitConverter.GetBytes(OwnerDraw.Objects.IndexOf(this));
        }

        public static byte[] SerializeObjectId(DrawableObject obj)
        {
            return BitConverter.GetBytes(obj.OwnerDraw.Objects.IndexOf(obj));
        }

        public static int DeserializeObjectId(byte[] data, ref int index)
        {
            int value = BitConverter.ToInt32(data, index);
            index += 4;
            return value;
        }

        public static ObjectType GetObjectType(DrawableObject @object)
        {
            if (@object is SimpleTransition) return ObjectType.SimpleTransition;
            else if (@object is SimpleState) return ObjectType.SimpleState;
            else if (@object is Origin) return ObjectType.Origin;
            else if (@object is Alias) return ObjectType.Alias;
            else if (@object is End) return ObjectType.End;
            else if (@object is Abort) return ObjectType.Abort;
            else if (@object is SuperState) return ObjectType.SuperState;
            else if (@object is SuperTransition) return ObjectType.SuperTransition;
            else if (@object is Nested) return ObjectType.Nested;
            else if (@object is Relation) return ObjectType.Relation;
            else if (@object is Text) return ObjectType.Text;
            else if (@object is StateAlias) return ObjectType.StateAlias;
            else if (@object is Equation) return ObjectType.Equation;
            else throw new Exception("Object type could not be evaluated.");
        }

        //Object definition
        public byte[] SerializeDefinition()
        {
            var data = new List<byte>();
            ObjectType objType = GetObjectType(this);
            
            //Start object definition
            data.Add(Serialization.Token.StartObjectDefinition);
            
            //ObjectType
            data.Add(Serialization.Token.ObjectType);
            data.Add((byte)objType);

            //Add object ID
            data.Add(Serialization.Token.ObjectId);
            data.AddRange(SerializeObjectId());
            
            //Add name
            data.Add(Serialization.Token.ObjectName);
            var nameS = Encoding.Unicode.GetBytes(name);
            data.Add((byte)nameS.Length);
            data.AddRange(nameS);

            //Add description
            var descriptionS = Encoding.Unicode.GetBytes(description);
            if (descriptionS.Length > 255)
            {
                data.Add(Serialization.Token.ObjectLargeDescription);
                data.AddRange(BitConverter.GetBytes((ushort)descriptionS.Length));
            }
            else
            {
                data.Add(Serialization.Token.ObjectDescription);
                data.Add((byte)descriptionS.Length);
            }
            data.AddRange(descriptionS);

            //End object definition
            data.Add(Serialization.Token.EndObjectDefinition);

            //Return data
            return data.ToArray();
        }

        protected static byte[] SerializeRelation(int id)
        {
            var data = new List<byte>();
            data.Add(Serialization.Token.RelationObject);
            data.AddRange(BitConverter.GetBytes(id));
            return data.ToArray();
        }

        protected static byte[] SerializeRelation(string objName)
        {
            var data = new List<byte>();
            data.Add(Serialization.Token.RelationName);
            data.AddRange(Serialization.SerializeParameter(objName));
            return data.ToArray();
        }

        public static string[] OptionsList(DrawableObject @object)
        {
            return @object.OwnerDraw.Objects.FindAll(obj => obj is SimpleState || obj is SuperState).ConvertAll(state => state.Name).ToArray();
        }

        public static DrawableObject Create(DrawableCollection ownerDraw, ObjectType objectType, string name, string description)
        {
            switch (objectType)
            {
                case ObjectType.SimpleTransition:
                    return new SimpleTransition(ownerDraw, name, description);
                case ObjectType.SuperTransition:
                    return new SuperTransition(ownerDraw, name, description);
                case ObjectType.SimpleState:
                    return new SimpleState(ownerDraw, name, description);
                case ObjectType.StateAlias:
                    return new StateAlias(ownerDraw, name, description);
                case ObjectType.SuperState:
                    return new SuperState(ownerDraw, name, description);
                case ObjectType.Nested:
                    return new Nested(ownerDraw, name, description);
                case ObjectType.Origin:
                    return new Origin(ownerDraw, name, description);
                case ObjectType.End:
                    return new End(ownerDraw, name, description);
                case ObjectType.Abort:
                    return new Abort(ownerDraw, name, description);
                case ObjectType.Alias:
                    return new Alias(ownerDraw, name, description);
                case ObjectType.Relation:
                    return new Relation(ownerDraw, name, description);
                case ObjectType.Text:
                    return new Text(ownerDraw, name, description);
                case ObjectType.Equation:
                    return new Equation(ownerDraw, name, description);
                default:
                    throw new Exception("Could not create object type.");
            }
        }

        public static DrawableObject DeserializeObjectDefinition(DrawableCollection ownerDraw, byte[] data, ref int index, out int id)
        {
            ObjectType objType;
            int len;
            string name, description;
            id = -1;
            //Get object type
            if (data[index++] != Serialization.Token.StartObjectDefinition) return null;
            if (data[index++] != Serialization.Token.ObjectType) return null;
            objType = (ObjectType)data[index++];
            //Get object ID
            if (data[index++] != Serialization.Token.ObjectId) return null;
            id = BitConverter.ToInt32(data, index);
            index += 4;
            //Get object name
            if (data[index++] != Serialization.Token.ObjectName) return null;
            len = data[index++];
            name = Encoding.Unicode.GetString(data, index, len);
            index += len;
            //Get object description
            if (data[index] == Serialization.Token.ObjectLargeDescription)
            {
                index++;
                len = BitConverter.ToUInt16(data, index);
                index += 2;
            }
            else if (data[index] == Serialization.Token.ObjectDescription)
            {
                index++;
                len = data[index++];
            }
            else
            {
                return null;
            }
            description = Encoding.Unicode.GetString(data, index, len);
            index += len;
            //Check end definition
            if(data[index++] != Serialization.Token.EndObjectDefinition) return null;
            //return object
            return Create(ownerDraw, objType, name, description);
        }

        public abstract bool DeserializeObjectSpecifics(byte[] data, ref int index);

        public static bool DeserializeRelation(Dictionary<int, DrawableObject> dictionary, byte[] data, ref int index, out DrawableObject obj)
        {
            obj = null;
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.RelationObject)) return false;
            int id = DeserializeObjectId(data, ref index);
            return dictionary.TryGetValue(id, out obj);
        }

        public static bool DeserializeRelation(byte[] data, ref int index, out string objName)
        {
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.RelationName)) { objName = ""; return false; }
            if (!Serialization.DeserializeParameter(data, ref index, out objName)) return false;
            return true;
        }
    }
}
