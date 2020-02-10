using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Phases.DrawableObjects;
using Phases.Actions;

namespace Phases
{
    class DrawableCollection
    {
        public DrawingSheet OwnerSheet;
        public List<DrawableObject> objects;
        public List<DrawableObject> Shadow { get; private set; }    //image shadow for undo/redo
        public TreeNode transTree, statesTree, linksTree, textsTree;

        public DrawableCollection(DrawingSheet ownerSheet)
        {
            OwnerSheet = ownerSheet;
            objects = new List<DrawableObject>();
            Shadow = new List<DrawableObject>();

            transTree = OwnerSheet.sheetTree.Nodes.Add("Transitions", "Transitions", Constants.ImageIndex.Transitions, Constants.ImageIndex.Transitions);
            statesTree = OwnerSheet.sheetTree.Nodes.Add("States", "States", Constants.ImageIndex.States, Constants.ImageIndex.States);
            linksTree = OwnerSheet.sheetTree.Nodes.Add("Links", "Links", Constants.ImageIndex.Links, Constants.ImageIndex.Links);
            textsTree = OwnerSheet.sheetTree.Nodes.Add("Texts", "Texts", Constants.ImageIndex.Texts, Constants.ImageIndex.Texts);

            OwnerSheet.sheetTree.Expand();
        }

        public List<DrawableObject> Objects => objects;
        public List<Link> Links => objects.FindAll(obj => obj is Link).ConvertAll(obj => obj as Link);
        public List<Origin> Origins => objects.FindAll(obj => obj is Origin).ConvertAll(obj => obj as Origin);
        public List<Alias> Aliases => objects.FindAll(obj => obj is Alias).ConvertAll(obj => obj as Alias);
        public List<State> States => objects.FindAll(obj => obj is State).ConvertAll(obj => obj as State);
        public List<SuperState> SuperStates => objects.FindAll(obj => obj is SuperState).ConvertAll(obj => obj as SuperState);
        public List<Nested> Nesteds => objects.FindAll(obj => obj is Nested).ConvertAll(obj => obj as Nested);

        public void Paint(Graphics g, DrawAttributes att)
        {
            //Drawing objects back
            Objects.FindAll(obj => obj is Origin).ForEach(obj => obj.DrawBack(g));
            Objects.FindAll(obj => !(obj is Origin)).ForEach(obj => obj.DrawBack(g));
            
            //Drawing objects
            Objects.FindAll(obj => !(obj is Origin)).ForEach(obj => obj.Draw(g, att.Scale));
            Objects.FindAll(obj => obj is Origin).ForEach(obj => obj.Draw(g, att.Scale));
        }

        public void PaintShadow(Graphics g, DrawAttributes att)
        {
            //Drawing objects
            Shadow.FindAll(obj => !(obj is Origin)).ForEach(obj => obj.DrawShadow(g, att));
            Shadow.FindAll(obj => obj is Origin).ForEach(obj => obj.DrawShadow(g, att));
        }

        private TreeNode GetNode(DrawableObject @object)
        {
            if (@object is Transition) return transTree;
            if (@object is State) return statesTree;
            if (@object is Link) return linksTree;
            if (@object is Text || @object is Equation) return textsTree;

            throw new Exception("Object without correspondend tree node.");
        }

        private void AddObjectTree(DrawableObject @object)
        {
            var tree = GetNode(@object);
            tree.Nodes.Add(@object.Node);
            tree.Expand();
        }

        private void RemoveObjectTree(DrawableObject @object)
        {
            var tree = GetNode(@object);
            tree.Nodes.Remove(@object.Node);
        }

        public void AddObject(DrawableObject @object)
        {
            Objects.Add(@object);
            Shadow.Add(@object.GetClone());
            AddObjectTree(@object);
            if(@object is IGlobal global)
            {
                if(global is Origin origin && origin.Father == null) OwnerSheet.Globals.Add(origin);
                else OwnerSheet.Globals.Add(global);
            }
        }

        public void AddObjects(List<DrawableObject> objects, List<DrawableObject> shadowState)
        {
            objects.ForEach(obj => shadowState.First(sobj => sobj.Equals(obj)).CopyTo(obj));
            Objects.AddRange(objects);
            objects.ForEach(obj => Shadow.Add(obj.GetClone()));
            objects.ForEach(obj => AddObjectTree(obj));
            objects.FindAll(obj => obj is Origin).ForEach(obj => OwnerSheet.Globals.Add(obj as Origin));
        }

        public void RemoveObjects(List<DrawableObject> list)
        {
            list.ForEach(obj => RemoveObjectTree(obj));
            foreach (DrawableObject obj in list)
            {
                //get shadow object
                if (obj is Transition)
                {
                    //remove references in draw
                    var trans = (Transition)obj;
                    if (trans.StartObject != null && trans.StartObject.outTransitions.Contains(trans))
                    {
                        trans.StartObject.outTransitions.Remove(trans);
                        GetShadow(trans.StartObject).outTransitions.Remove(trans);
                    }
                    if (trans.EndObject != null && trans.EndObject.inTransitions.Contains(trans))
                    {
                        trans.EndObject.inTransitions.Remove(trans);
                        GetShadow(trans.EndObject).inTransitions.Remove(trans);
                    }
                    trans.StartObject = null;
                    trans.EndObject = null;
                    //remove references in shadow
                    trans = (Transition)GetShadow(trans);
                    trans.StartObject = null;
                    trans.EndObject = null;
                }
                else
                {
                    foreach (Transition trans in obj.InTransitions)
                    {
                        //remove transition reference in draw
                        trans.EndObject = null;
                        //remove transition reference in shadow
                        ((Transition)GetShadow(trans)).EndObject = null;
                    }
                    foreach (Transition trans in obj.OutTransitions)
                    {
                        //remove transition reference in draw
                        trans.StartObject = null;
                        //remove transition reference in shadow
                        ((Transition)GetShadow(trans)).StartObject = null;
                    }
                }
                if (obj is State || obj is SuperState)
                {
                    foreach (DrawableObject obj2 in Objects)
                    {
                        if (obj2 is Alias && ((Alias)obj2).PointingTo == obj.Name)
                        {
                            //remove reference in draw
                            ((Alias)obj2).PointingTo = null;
                            //remove reference in shadow
                            ((Alias)GetShadow(obj2)).PointingTo = null;
                        }
                    }
                }
                else if(obj is Origin origin)
                {
                    OwnerSheet.Globals.Remove(origin);
                }
                else if (obj is Relation relation)
                {
                    OwnerSheet.Globals.Remove(relation);
                }
                else if (obj is Equation equation)
                {
                    OwnerSheet.Globals.Remove(equation);
                }
                Objects.Remove(obj);
                Shadow.Remove(obj);
            }
        }

        public DrawableObject GetShadow(DrawableObject @object)
        {
            return Shadow.FirstOrDefault(sha => sha.Equals(@object));
        }

        public void Clear(TreeView tree)
        {
            Objects.Clear();
            Shadow.Clear();
            OwnerSheet.Globals.Clear();
        }

        public DrawableObject GetOnObject(Point position)
        {
            var list = new List<DrawableObject>(Objects);
            list.Reverse();
            return list.FirstOrDefault(obj => obj != null && !(obj is Transition) && obj.IsSelectablePoint(position));
        }

        public Transition OnTransition(Point position)
        {
            var list = new List<DrawableObject>(Objects);
            list.Reverse();
            return (Transition)list.FirstOrDefault(obj => obj != null && (obj is Transition) && obj.IsSelectablePoint(position));
        }

        public static Rectangle GetObjectsRectangle(List<DrawableObject> list, int margin = 0)
        {
            if (list == null || list.Count == 0) return Rectangle.Empty;
            int x1, y1, x2, y2;
            Rectangle rect = list[0].GetSelectionRectangle();
            x1 = rect.X;
            y1 = rect.Y;
            x2 = x1 + rect.Width;
            y2 = y1 + rect.Height;
            for (int i = 1; i < list.Count; i++)
            {
                rect = list[i].GetSelectionRectangle();
                if (rect.X < x1) x1 = rect.X;
                if (rect.Y < y1) y1 = rect.Y;
                if (rect.X + rect.Width > x2) x2 = rect.X + rect.Width;
                if (rect.Y + rect.Height > y2) y2 = rect.Y + rect.Height;
            }
            x1 -= margin;
            y1 -= margin;
            x2 += margin;
            y2 += margin;
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        public static Point GetRectangleCenter(Rectangle rect)
        {
            return new Point((rect.X + rect.Width) / 2, (rect.Y + rect.Height) / 2);
        }

        public void PerformAction(DrawAction action)
        {
            DrawableObject shadow;
            //Save before
            //action.ShadowState = action.DrawRef.ConvertAll(obj => obj.GetClone());
            action.ShadowState = Shadow.FindAll(obj => action.DrawRef.Contains(obj)).ConvertAll(obj => obj.GetClone());

            //Make Add/Remove and copy action to Shadow list
            switch (action.ActionType)
            {
                case RecordableAction.ActionTypes.Remove:
                case RecordableAction.ActionTypes.Cut:
                    RemoveObjects(action.Selection);
                    break;
                default:
                    foreach (DrawableObject obj in action.DrawRef)
                    {
                        shadow = Shadow.FirstOrDefault(sha => sha.Equals(obj));
                        if (shadow == null)
                        {
                            AddObject(obj);
                        }
                        else
                        {
                            obj.CopyTo(shadow);
                        }
                    }
                    break;
            }
            //Save after
            action.AfterAction = action.DrawRef.FindAll(obj => Shadow.Contains(obj)).ConvertAll(obj => obj.GetClone());
        }

        private void ChangeObjects(List<DrawableObject> list, List<DrawableObject> afterList)
        {
            foreach (DrawableObject obj in list)
            {
                var after = afterList.First(o => o.Equals(obj));
                var shadow = Shadow.First(o => o.Equals(obj));
                after.CopyTo(obj);
                after.CopyTo(shadow);
            }
        }

        public void Undo(MouseTool mouse, DrawAction action)
        {
            mouse.ClearSelection();
            var toRemoveList = action.DrawRef.FindAll(obj => !action.ShadowState.Contains(obj) && action.AfterAction.Contains(obj));
            var toAddList = action.DrawRef.FindAll(obj => action.ShadowState.Contains(obj) && !action.AfterAction.Contains(obj));
            var toChangeList = action.DrawRef.FindAll(obj => action.ShadowState.Contains(obj) && action.AfterAction.Contains(obj));
            RemoveObjects(toRemoveList);
            AddObjects(toAddList, action.ShadowState);
            ChangeObjects(toChangeList, action.ShadowState);
            mouse.SetSelection(action.Selection, action.FocusSelectionIndex);
        }

        public void Redo(MouseTool mouse, DrawAction action)
        {
            mouse.ClearSelection();
            var toRemoveList = action.DrawRef.FindAll(obj => action.ShadowState.Contains(obj) && !action.AfterAction.Contains(obj));
            var toAddList = action.DrawRef.FindAll(obj => !action.ShadowState.Contains(obj) && action.AfterAction.Contains(obj));
            var toChangeList = action.DrawRef.FindAll(obj => action.ShadowState.Contains(obj) && action.AfterAction.Contains(obj));
            RemoveObjects(toRemoveList);
            AddObjects(toAddList, action.AfterAction);
            ChangeObjects(toChangeList, action.AfterAction);
            mouse.SetSelection(action.Selection, action.FocusSelectionIndex);
        }

        public void CancelAction(RecordableAction.ActionTypes actionType, List<DrawableObject> selectedObjects)
        {
            switch (actionType)
            {
                case RecordableAction.ActionTypes.Create:
                case RecordableAction.ActionTypes.Remove:
                    break;
                default:
                    for (int Index = 0; Index < selectedObjects.Count; Index++)
                    {
                        int drawIndex = Objects.IndexOf(selectedObjects[Index]);
                        Shadow[drawIndex].CopyTo(Objects[drawIndex]);
                    }
                    break;
            }
        }

        #region "Serialization"

        public static byte[] SerializeList(List<DrawableObject> list)
        {
            var data = new List<byte>();
            //Objects definitions
            foreach (DrawableObject obj in list)
            {
                data.AddRange(obj.SerializeDefinition());
            }

            //Objects specific settings
            foreach (DrawableObject obj in list)
            {
                data.Add(Serialization.Token.StartObjectParameters);
                data.AddRange(obj.SerializeSpecifics());
                data.Add(Serialization.Token.EndObjectParameters);
            }

            //Objects relations
            foreach (DrawableObject obj in list)
            {
                if (obj is Transition)
                {
                    var trans = (Transition)obj;
                    data.Add(Serialization.Token.StartObjectRelations);
                    data.AddRange(trans.SerializeRelations());
                    data.Add(Serialization.Token.EndObjectRelations);
                }
                else if (obj is Alias)
                {
                    var trans = (Alias)obj;
                    data.Add(Serialization.Token.StartObjectRelations);
                    data.AddRange(trans.SerializeRelations());
                    data.Add(Serialization.Token.EndObjectRelations);
                }
                else if (obj is StateAlias)
                {
                    var trans = (StateAlias)obj;
                    data.Add(Serialization.Token.StartObjectRelations);
                    data.AddRange(trans.SerializeRelations());
                    data.Add(Serialization.Token.EndObjectRelations);
                }
            }
            return data.ToArray();
        }

        public byte[] Serialize()
        {
            var data = new List<byte>();
            data.Add(Serialization.Token.StartDrawObjects);
            data.AddRange(SerializeList(Objects));
            data.Add(Serialization.Token.EndDrawObjects);
            return data.ToArray();
        }

        public static bool DeserializeList(DrawableCollection owner, byte[] data, ref int index, out Dictionary<int, DrawableObject> objects)
        {
            objects = new Dictionary<int, DrawableObject>();
            DrawableObject obj;
            int id;

            //Objects definitions
            while (Serialization.Token.Is(data, index, Serialization.Token.StartObjectDefinition))
            {
                obj = DrawableObject.DeserializeObjectDefinition(owner, data, ref index, out id);
                if (obj == null) return false;
                objects.Add(id, obj);
            }

            //Objects specific settings
            while (Serialization.Token.Is(data, index, Serialization.Token.StartObjectParameters))
            {
                index++;
                id = DrawableObject.DeserializeObjectId(data, ref index);
                if (!objects.TryGetValue(id, out obj)) return false;
                if (!obj.DeserializeObjectSpecifics(data, ref index)) return false;
                if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndObjectParameters)) return false;
            }

            //Objects relations
            while (Serialization.Token.Is(data, index, Serialization.Token.StartObjectRelations))
            {
                index++;
                id = DrawableObject.DeserializeObjectId(data, ref index);
                if (!objects.TryGetValue(id, out obj)) return false;
                if (obj is Transition trans)
                {
                    if (!trans.DeserializeRelations(objects, data, ref index)) return false;
                }
                else if (obj is Alias alias)
                {
                    if (!alias.DeserializeRelations(objects, data, ref index)) return false;
                }
                else if (obj is StateAlias salias)
                {
                    if (!salias.DeserializeRelations(objects, data, ref index)) return false;
                }
                if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndObjectRelations)) return false;
            }
            return true;
        }

        public bool Deserialize(byte[] data, ref int index)
        {
            if (data.Length < 7) return false;
            Dictionary<int, DrawableObject> objects;
            //First the draw head
            if (!Serialization.Token.Deserialize(data, ref index, Serialization.Token.StartDrawObjects)) return false;
            if (!DeserializeList(this, data, ref index, out objects)) return false;

            foreach (DrawableObject obj in objects.Values)
            {
                AddObject(obj);
            }

            return Serialization.Token.Deserialize(data, ref index, Serialization.Token.EndDrawObjects);
        }
        #endregion

    }
}
