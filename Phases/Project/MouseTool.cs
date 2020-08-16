using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Phases.DrawableObjects;

namespace Phases
{
    class MouseTool
    {
        public enum MouseDoing
        {
            Nothing,
            Drawing,
            Selecting,
            Moving,
            MovingText,
            Resizing,
            Editing,
            Zooming,
            Pasting
        }

        public enum CursorTypes
        {
            Default,
            Move,
            Resize,
            Paint
        }

        public enum ResizingTypes
        {
            None,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8,
            Left_Top = Left | Top,
            Right_Top = Right | Top,
            Left_Bottom = Left | Bottom,
            Right_Bottom = Right | Bottom,
            Spline0 = 16,
            Spline1 = 32,
            Spline2 = 64,
            Spline3 = 128
        }

        public struct SelectionRectangle
        {
            public static Size size = new Size(4, 4);
            public Point point;
            public DrawableObject obj;
            public ResizingTypes clip;
            public bool focus;

            public SelectionRectangle(DrawableObject _obj, ResizingTypes _clip, Point _point, bool _focus)
            {
                point = _point;
                obj = _obj;
                clip = _clip;
                focus = _focus;
            }

            public RectangleF Rectangle(float scale)
            {
                SizeF sizef = size;
                sizef.Width /= scale;
                sizef.Height /= scale;
                return Util.GetRectangleF(point, sizef);
            }
        }

        
        public MouseDoing Doing;
        public DrawableObject DrawingObject;
        public DrawableObject.ObjectType DrawingObjectType;
        public DrawableObject PreviousObject;
        public DrawableObject OnObject;
        public Transition OnTransition;
        public List<SelectionRectangle> SelectionRectangles;
        private List<DrawableObject> selectedObjects;
        private int selectedObjectFocusedIndex;
        private DateTime pastedTimeStamp;
        private int pastedTimes;
        public Rectangle SelectionArea;
        public Point Location, FirstPoint, MovingPoint, StartDrawPoint;
        public ResizingTypes ResizingType;
        public DrawableCollection draw;
        private PictureBox pBox;
        public bool InclusiveSelection;

        public bool Scrolling { get; set; } = false;
        public Point ScrollPoint { get; set; }

        public Point SecundaryClickPoint { get; set; }

        private CursorTypes cursorType;
        public CursorTypes CursorType
        {
            get
            {
                return cursorType;
            }
            set
            {
                switch (value)
                {
                    case CursorTypes.Default:
                        pBox.Cursor = Cursors.Default;
                        break;
                    case CursorTypes.Move:
                        break;
                    case CursorTypes.Resize:
                        break;
                    case CursorTypes.Paint:
                        pBox.Cursor = Cursors.Cross;
                        break;
                }
                cursorType = value;
            }
        }

        public void SetSelection(List<DrawableObject> objects, int focusIndex = -1)
        {
            if (focusIndex == -1 && selectedObjectFocusedIndex != -1 && selectedObjects[selectedObjectFocusedIndex] is State)
            {
                focusIndex = objects.IndexOf(selectedObjects[selectedObjectFocusedIndex]);
            }
            selectedObjects = new List<DrawableObject>();
            objects.FindAll(obj => draw.objects.Contains(obj)).ForEach(obj => selectedObjects.Add(obj));
            if (focusIndex == -1)
            {
                foreach (DrawableObject obj in selectedObjects)
                {
                    if (obj is State || obj is SuperState)
                    {
                        selectedObjectFocusedIndex = selectedObjects.IndexOf(obj);
                        break;
                    }
                }
            }
            else
            {
                selectedObjectFocusedIndex = focusIndex;
            }
        }

        public byte[] SerializeSelection()
        {
            var data = new List<byte>();

            pastedTimeStamp = DateTime.Now;
            pastedTimes = 0;
            data.AddRange(Serialization.SerializeParameter(pastedTimeStamp));
            data.AddRange(DrawableCollection.SerializeList(SelectedObjects));
            return data.ToArray();
        }

        public bool DeserializeSelection(byte[] data, bool pointerLocation, Point location, Func<string, List<DrawableObject>, string> NextName)
        {
            Dictionary<int, DrawableObject> objects;
            int index = 0;

            if (!Serialization.DeserializeParameter(data, ref index, out DateTime date)) return false;
            if (!DrawableCollection.DeserializeList(draw, data, ref index, out objects)) return false;

            Point poffset;
            if (pointerLocation)
            {
                Point center = Util.Center(objects.Values.ToList().ConvertAll(dobj => dobj.Center).ToArray());
                poffset = Location;
                poffset.Offset(-center.X, -center.Y);
            }
            else
            {
                int offset = 10;
                if (date == pastedTimeStamp)
                {
                    pastedTimes++;
                }
                else
                {
                    pastedTimes = 0;
                    pastedTimeStamp = date;
                }
                offset *= pastedTimes;
                poffset = new Point(offset, offset);
            }

            ClearSelection();
            // Changing objects names first to handle state size change separately
            foreach (DrawableObject obj in objects.Values)
            {
                if (draw.OwnerSheet.ExistsName(obj.Name))
                {
                    obj.Name = NextName(obj.GetFormName(), objects.Values.ToList());
                }
            }
            // Aplying paste offset and adding to selection
            foreach (DrawableObject obj in objects.Values)
            {
                obj.Move(poffset);
                AddToSelection(obj);
            }
            //Doing = MouseDoing.Pasting;
            return true;
        }

        public Rectangle GetSelectionRectangle(int margin) => DrawableCollection.GetObjectsRectangle(selectedObjects, margin);

        public int SelectionFocusIndex => selectedObjectFocusedIndex;

        public List<DrawableObject> SelectedObjects => selectedObjects;

        public void ClearSelection()
        {
            selectedObjects.Clear();
            selectedObjectFocusedIndex = -1;
        }

        public void AddToSelection(DrawableObject obj)
        {
            selectedObjects.Add(obj);
            if (selectedObjectFocusedIndex == -1 && (obj is State || obj is SuperState))
            {
                selectedObjectFocusedIndex = selectedObjects.IndexOf(obj);
            }
        }

        public void RemoveFromSelection(DrawableObject obj)
        {
            if (selectedObjects.Contains(obj))
            {
                selectedObjects.Remove(obj);
                selectedObjectFocusedIndex = -1;
                foreach (DrawableObject obj2 in selectedObjects)
                {
                    if (obj2 is State || obj2 is SuperState)
                    {
                        selectedObjectFocusedIndex = selectedObjects.IndexOf(obj2);
                        break;
                    }
                }
            }
            else
            {
                throw new Exception("Unhandled condition.");
            }
        }

        public void SelectionFocus(DrawableObject @object)
        {
            if (@object == null || !(@object is State || @object is SuperState)) return;
            selectedObjectFocusedIndex = selectedObjects.IndexOf(@object);
        }

        public bool SelectionResult(Point startPoint, Point endPoint)
        {
            if (startPoint.X == endPoint.X && startPoint.Y == endPoint.Y)
            {
                ClearSelection();
                //Puntual selection
                foreach (DrawableObject obj in draw.Objects)
                {
                    if (obj.IsSelectablePoint(startPoint))
                    {
                        AddToSelection(obj);
                        break;
                    }
                }
            }
            else if (startPoint.X > endPoint.X)
            {
                //Inclusive selection
                foreach (DrawableObject obj in draw.Objects)
                {
                    if (obj.IsSelectable(Util.GetRectangle(startPoint, endPoint)))
                    {
                        if(!selectedObjects.Contains(obj)) AddToSelection(obj);
                    }
                    else
                    {
                        if (selectedObjects.Contains(obj)) selectedObjects.Remove(obj);
                    }
                }
            }
            else
            {
                //Exclusive selection
                foreach (DrawableObject obj in draw.Objects)
                {
                    if (obj.IsCovered(Util.GetRectangle(startPoint, endPoint)))
                    {
                        if (!selectedObjects.Contains(obj)) AddToSelection(obj);
                    }
                    else
                    {
                        if (selectedObjects.Contains(obj)) selectedObjects.Remove(obj);
                    }
                }
            }
            return selectedObjects.Count > 0;
        }

        private void AddChangingObject(ref List<DrawableObject> list, DrawableObject obj)
        {
            if (!list.Contains(obj)) list.Add(obj);
            else return;
            foreach (Transition trans in obj.InTransitions)
            {
                if (!list.Contains(trans)) list.Add(trans);
            }
            foreach (Transition trans in obj.OutTransitions)
            {
                if (!list.Contains(trans)) list.Add(trans);
            }
            if (obj is Alias alias && alias.Pointing != null && !list.Contains(alias.Pointing)) list.Add(alias.Pointing);
            if (obj is State || obj is SuperState)
            {
                foreach (DrawableObject obj2 in draw.Objects)
                {
                    if (obj2 is Alias && ((Alias)obj2).PointingTo == obj.Name && !list.Contains(obj2)) list.Add(obj2);
                }
            }
        }

        public List<DrawableObject> ChangingObjects
        {
            get
            {
                var list = new List<DrawableObject>();
                foreach (DrawableObject obj in SelectedObjects)
                {
                    if (obj is Transition trans)
                    {
                        if (!list.Contains(obj)) list.Add(obj);
                        if (trans.StartObject != null) AddChangingObject(ref list, trans.StartObject);
                        if (trans.EndObject != null) AddChangingObject(ref list, trans.EndObject);
                        var shadow = draw.GetShadow(trans);
                        if(shadow != null)
                        {
                            trans = (Transition)shadow;
                            if (trans.StartObject != null) AddChangingObject(ref list, trans.StartObject);
                            if (trans.EndObject != null) AddChangingObject(ref list, trans.EndObject);
                        }
                    }
                    else
                    {
                        if (obj is Alias alias)
                        {
                            Alias alias_sh = draw.GetShadow(alias) as Alias;
                            if (alias_sh == null || alias.Pointing != alias_sh.Pointing)
                            {
                                if (alias_sh != null && alias_sh.Pointing != null && !list.Contains(alias_sh.Pointing)) list.Add(alias_sh.Pointing);
                                if (alias.Pointing != null && !list.Contains(alias.Pointing)) list.Add(alias.Pointing);
                            }
                        }
                        AddChangingObject(ref list, obj);
                    }
                }
                return list;
            }
        }

        public MouseTool(DrawableCollection _draw, PictureBox pictureBox)
        {
            draw = _draw;
            pBox = pictureBox;
            Doing = MouseDoing.Nothing;
            DrawingObject = null;
            SelectionRectangles = new List<SelectionRectangle>();
            InclusiveSelection = false;
            selectedObjects = new List<DrawableObject>();
            selectedObjectFocusedIndex = -1;
            pastedTimeStamp = DateTime.Now;
            pastedTimes = 0;
        }

        public void Scroll(Point offset)
        {
            
        }

        public Cursor Moving(Point position, Matrix transform)
        {
            foreach (SelectionRectangle sre in SelectionRectangles)
            {
                if (sre.Rectangle(transform.Elements.First()).Contains(position))
                {
                    CursorType = CursorTypes.Resize;
                    ResizingType = sre.clip;
                    if (sre.obj is State || sre.obj is SuperState)
                        OnObject = sre.obj;
                    else if(sre.obj is Transition)
                    {
                        OnTransition = (Transition)sre.obj;
                        if (sre.clip == ResizingTypes.Spline0 && OnTransition.StartObject != null)
                            OnObject = OnTransition.StartObject;
                        else if (sre.clip == ResizingTypes.Spline3 && OnTransition.EndObject != null)
                            OnObject = OnTransition.EndObject;
                    }
                    switch (sre.clip)
                    {
                        case ResizingTypes.Left_Top:
                        case ResizingTypes.Right_Bottom:
                            return Cursors.SizeNWSE;
                        case ResizingTypes.Left_Bottom:
                        case ResizingTypes.Right_Top:
                        case ResizingTypes.Spline0:
                        case ResizingTypes.Spline1:
                        case ResizingTypes.Spline2:
                        case ResizingTypes.Spline3:
                            return Cursors.SizeNESW;
                        case ResizingTypes.Left:
                        case ResizingTypes.Right:
                            return Cursors.SizeWE;
                        case ResizingTypes.Top:
                        case ResizingTypes.Bottom:
                            return Cursors.SizeNS;
                    }
                }
            }
            foreach (DrawableObject obj in SelectedObjects)
            {
                if (obj.IsSelectablePoint(position))
                {
                    CursorType = CursorTypes.Move;
                    return Cursors.SizeAll;
                }
            }
            CursorType = CursorTypes.Default;
            return Cursors.Default;
        }

        private void MoveObject(DrawableObject @object, Point offset)
        {
            foreach (Transition trans in @object.OutTransitions)
            {
                if (!SelectedObjects.Contains(trans) && trans.StartObject == @object)
                {
                    trans.MoveStart(offset);
                }
            }
            foreach (Transition trans in @object.InTransitions)
            {
                if (!SelectedObjects.Contains(trans))
                {
                    trans.MoveEnd(offset);
                }
            }
            @object.Move(offset);
        }

        public void MoveText(Point dest)
        {
            Point offset;
            offset = new Point(dest.X - MovingPoint.X, dest.Y - MovingPoint.Y);
            if (OnTransition != null)
            {
                OnTransition.MoveText(offset);
            }
            else
            {
                OnObject.MoveText(offset);
            }
            MovingPoint = dest;
        }

        public void MoveObjects(Point dest)
        {
            var movedList = new List<DrawableObject>();
            Point offset;
            offset = new Point(dest.X - MovingPoint.X, dest.Y - MovingPoint.Y);
            foreach (DrawableObject obj in SelectedObjects)
            {
                if (obj is Transition)
                {
                    Transition trans = (Transition)obj;
                    if (trans.StartObject == null && trans.EndObject == null)
                    {
                        if (!movedList.Contains(trans))
                        {
                            trans.Move(offset);
                            movedList.Add(trans);
                        }
                    }
                    else
                    {
                        if (trans.StartObject != null)
                        {
                            if (!SelectedObjects.Contains(trans.StartObject))
                            {
                                if (!movedList.Contains(trans.StartObject))
                                {
                                    MoveObject(trans.StartObject, offset);
                                    movedList.Add(trans.StartObject);
                                }
                            }
                            if (!movedList.Contains(trans))
                            {
                                trans.MoveStart(offset);
                            }
                        }
                        else
                        {
                            if (!SelectedObjects.Contains(trans.StartObject)) trans.MoveStart(offset);
                        }
                        if (trans.EndObject != null)
                        {
                            if (!SelectedObjects.Contains(trans.EndObject))
                            {
                                if (!movedList.Contains(trans.EndObject))
                                {
                                    MoveObject(trans.EndObject, offset);
                                    movedList.Add(trans.EndObject);
                                }
                            }
                            if (!movedList.Contains(trans))
                            {
                                trans.MoveEnd(offset);
                            }
                        }
                        else
                        {
                            if (!SelectedObjects.Contains(trans.EndObject)) trans.MoveEnd(offset);
                        }
                    }
                }
                else
                {
                    if (!movedList.Contains(obj))
                    {
                        MoveObject(obj, offset);
                        movedList.Add(obj);
                    }
                }
            }
            MovingPoint = dest;
        }

        public void ResizeObjects(Point dest)
        {
            Point offset;
            offset = new Point(dest.X - MovingPoint.X, dest.Y - MovingPoint.Y);
            foreach (DrawableObject obj in SelectedObjects)
            {
                obj.ResizeCheck(ref offset, ResizingType);
            }
            foreach (DrawableObject obj in SelectedObjects)
            {
                if (obj is Transition)
                {
                    PreviousObject = OnObject;
                    OnObject = draw.GetOnObject(dest);
                    OnTransition = (Transition)obj;
                    if (OnObject == null)
                    {
                        offset = new Point(dest.X - MovingPoint.X, dest.Y - MovingPoint.Y);
                        OnTransition.Resize(offset, ResizingType);
                        if (ResizingType == ResizingTypes.Spline0) OnTransition.StartObject = null;
                        else if (ResizingType == ResizingTypes.Spline3) OnTransition.EndObject = null;
                    }
                    else if (ResizingType == ResizingTypes.Spline0)
                    {
                        if ((obj is Transition && (OnObject is End || OnObject is Abort || OnObject is Relation)) ||
                            (obj is SuperTransition && (OnObject is Alias || OnObject is SimpleState || OnObject is StateAlias)) ||
                            (OnObject is Origin && OnObject.OutTransitions.Length != 0 && OnTransition != OnObject.OutTransitions.First()))
                        {
                            OnObject = null;
                            return;
                        }
                        else
                        {
                            double angle;
                            OnObject.Intersect(dest, ref StartDrawPoint, ref OnTransition.StartAngle);
                            offset = new Point(StartDrawPoint.X - MovingPoint.X, StartDrawPoint.Y - MovingPoint.Y);
                            OnTransition.Resize(offset, ResizingType);
                            OnTransition.OutDir(OnObject.OutDir(StartDrawPoint, out angle), 1);
                        }
                        if (PreviousObject != OnObject) OnTransition.StartObject = OnObject;
                    }
                    else if (ResizingType == ResizingTypes.Spline3)
                    {
                        if (OnObject is Origin || OnObject is Relation)
                        {
                            OnObject = null;
                            return;
                        }
                        else
                        {
                            double angle;
                            OnObject.Intersect(dest, ref StartDrawPoint, ref OnTransition.EndAngle);
                            offset = new Point(StartDrawPoint.X - MovingPoint.X, StartDrawPoint.Y - MovingPoint.Y);
                            OnTransition.Resize(offset, ResizingType);
                            OnTransition.OutDir(OnObject.OutDir(StartDrawPoint, out angle), 2);
                        }
                        if (PreviousObject != OnObject) OnTransition.EndObject = OnObject;
                    }
                }
                else if (obj is SimpleState || obj is StateAlias)
                {
                    State oState = (State)obj;
                    obj.Resize(offset, ResizingType);
                    foreach (Transition oTransition in oState.InTransitions)
                    {
                        oTransition.MoveEndTo(oState.PointFromAngle(oTransition.EndAngle));
                    }
                    foreach (Transition oTransition in oState.OutTransitions)
                    {
                        oTransition.MoveStartTo(oState.PointFromAngle(oTransition.StartAngle));
                    }
                }
                else if (obj is SuperState)
                {
                    SuperState oState = (SuperState)obj;
                    obj.Resize(offset, ResizingType);
                    foreach (Transition oTransition in oState.InTransitions)
                    {
                        oTransition.MoveEndTo(oState.PointFromOffset(ResizingType, oTransition.EndPoint, oTransition.EndAngle));
                    }
                    foreach (Transition oTransition in oState.OutTransitions)
                    {
                        oTransition.MoveStartTo(oState.PointFromOffset(ResizingType, oTransition.StartPoint, oTransition.StartAngle));
                    }
                }
                else if (obj is Nested)
                {
                    Nested oState = (Nested)obj;
                    obj.Resize(offset, ResizingType);
                    foreach (Transition oTransition in oState.InTransitions)
                    {
                        oTransition.MoveEndTo(oState.PointFromOffset(ResizingType, oTransition.EndPoint, oTransition.EndAngle));
                    }
                    foreach (Transition oTransition in oState.OutTransitions)
                    {
                        oTransition.MoveStartTo(oState.PointFromOffset(ResizingType, oTransition.StartPoint, oTransition.StartAngle));
                    }
                }
                else
                {
                    obj.Resize(offset, ResizingType);
                }
            }
            MovingPoint.Offset(offset);
        }

        public void DrawSelectionsBack(Graphics g)
        {
            foreach (DrawableObject obj in selectedObjects)
            {
                obj.DrawSelectionBack(g);
            }
        }

        public void DrawSelections(Graphics g, Matrix transform)
        {
            //Draw drawing object
            if (DrawingObject != null) DrawingObject.Draw(g, transform.Elements[3]);
            //Drawing selections
            SelectionRectangles.Clear();
            foreach (DrawableObject obj in selectedObjects)
            {
                SelectionRectangles.AddRange(obj.DrawSelection(g, selectedObjectFocusedIndex == selectedObjects.IndexOf(obj)));
            }
            if (SelectionRectangles.Count > 0)
            {
                foreach(SelectionRectangle sre in SelectionRectangles)
                {
                    RectangleF rect = sre.Rectangle(transform.Elements.First());
                    if (sre.focus)
                    {
                        g.FillRectangle(Brushes.White, rect);
                        g.DrawRectangle(new Pen(Color.Blue, 0.001f), rect.X, rect.Y, rect.Width, rect.Height);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Blue, rect);
                    }
                }
            }
            if (InclusiveSelection)
            {
                Brush iselb = new SolidBrush(Color.FromArgb(20, Color.Blue));
                g.FillRectangle(iselb, SelectionArea);
            }
            g.DrawRectangle(Pens.Blue, SelectionArea);
            //draw DrawPoint
            if (OnObject != null && ((Doing == MouseDoing.Resizing && OnTransition != null) || Doing == MouseDoing.Drawing ||
                (Doing == MouseDoing.Nothing && CursorType == CursorTypes.Paint && (DrawingObjectType == DrawableObject.ObjectType.SimpleTransition || DrawingObjectType == DrawableObject.ObjectType.SuperTransition))))
            {
                try
                {
                    if(DrawingObjectType == DrawableObject.ObjectType.SimpleTransition)
                    {
                        if (OnObject is State || OnObject is SuperState || OnObject is Alias || OnObject is End || OnObject is Abort)
                        {
                            g.DrawEllipse(Pens.Red, StartDrawPoint.X - 2, StartDrawPoint.Y - 2, 4, 4);
                        }
                        else if (OnObject is Origin)
                        {
                            var origin = (Origin)OnObject;
                            g.DrawEllipse(Pens.Red, origin.Location.X - Origin.selectionRadio, origin.Location.Y - Origin.selectionRadio, Origin.selectionRadio * 2, Origin.selectionRadio * 2);
                        }
                    }
                    else if (DrawingObjectType == DrawableObject.ObjectType.SuperTransition)
                    {
                        if (OnObject is SuperState || OnObject is Nested)
                        {
                            g.DrawEllipse(Pens.Red, StartDrawPoint.X - 2, StartDrawPoint.Y - 2, 4, 4);
                        }
                        else if (OnObject is Origin)
                        {
                            var origin = (Origin)OnObject;
                            g.DrawEllipse(Pens.Red, origin.Location.X - Origin.selectionRadio, origin.Location.Y - Origin.selectionRadio, origin.SelectionRadio * 2, Origin.selectionRadio * 2);
                        }
                    }
                    else
                    {
                        if (OnObject is State || OnObject is SuperState || OnObject is Alias || OnObject is End || OnObject is Abort)
                        {
                            g.DrawEllipse(Pens.Red, StartDrawPoint.X - 2, StartDrawPoint.Y - 2, 4, 4);
                        }
                        else if (OnObject is Origin)
                        {
                            var origin = (Origin)OnObject;
                            g.DrawEllipse(Pens.Red, origin.Location.X - Origin.selectionRadio, origin.Location.Y - Origin.selectionRadio, Origin.selectionRadio * 2, Origin.selectionRadio * 2);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error MouseTool.DrawSelections:" + e.Message);
                }
            }
        }
    }
}
