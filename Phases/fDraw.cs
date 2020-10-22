using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Phases.DrawableObjects;
using Phases.Actions;
using Phases.BasicObjects;
using Phases.CodeGeneration;
using Phases.Expresions;
using Phases.Simulation;
using Phases.Variables;
using System.Drawing.Imaging;

namespace Phases
{
    public partial class fDraw : Form
    {
        readonly float[] zoomScales = { 15, 20, 25, 30, 40, 50, 75, 100, 125, 150, 200, 300, 400, 500, 600, 800, 1000 };
        readonly int startZoomScaleIndex = 7;

        private MouseTool mouse;
        private bool blockMouseTool = false;
        private double dAngle = 0d;
        private string FileName = "";
        private Matrix DrawTransform = new Matrix();
        private Matrix ShadowTransform = new Matrix();
        private float DrawScale = 1.0f;
        private float ShadowScale = 0.2f;
        public const float DrawScrollValue = 0.1f;
        private Size PictureSize;
        private PhasesBook book;
        private DialogResult saveResult;
        private AppInterface appInterface;
        private CodeGenerationProfile profile;
        private VirtualMachine Simulation = null;
        private bool simulationMode = false;
        private SimulationState simulationStatus;
        private SignalsDraw signalsDraw;
        private MouseEventArgs lastMouseState;
        private Keys ForceStraightLineKey = Keys.Control;
        private Keys ForceStateCircleKey = Keys.Control;
        private Keys ForceSnapCursorKey = Keys.Shift;

        public fDraw()
        {
            InitializeComponent();

            //Initialize ui
            leftTabControl.TabPages.Remove(variablesTabPage);
            simTools.Visible = false;
            logsViewToolStripMenuItem.Checked = false;
            undoToolStripMenuItem.Tag = btUndo;
            redoToolStripMenuItem.Tag = btRedo;
            saveToolStripMenuItem.Tag = btSave;
            pasteToolStripMenuItem.Tag = btPaste;
            copyToolStripMenuItem.Tag = btCopy;
            cutToolStripMenuItem.Tag = btCut;
            deleteToolStripMenuItem.Tag = btDelete;

            //Add KeyPress event to propertyGrid
            propertyGrid.SelectedObject = null;
            propertyGrid.Controls[2].Controls[1].KeyPress += controls_KeyPress;

            //Add KeyPress event to pictureBox
            pBox.PreviewKeyDown += PBox_PreviewKeyDown;

            //Create interface object
            appInterface = new AppInterface(pBox, hScroll, vScroll, tvObjects, mouse);

            //Load sheet
            book = new PhasesBook(appInterface, DrawingSheet.DefaultSize);
            PictureSize = pBox.Size;

            //Initialize draw and mouse tools
            mouse = new MouseTool(book.SelectedSheet.Sketch, pBox);

            //load file data
            profile = new CodeGenerationProfile();
            profile.ProjectName = "MyMachine";
            profile.Path = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents");

            //Adjust sheet preview
            AdjustSheetView();
        }

        private void fDraw_Load(object sender, EventArgs e)
        {
            // License check
            if (DateTime.Now.Year >= 2021)
            {
                MessageBox.Show("License expired, please request a new one.", "License expired!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(3);
            }

#if DEBUG   //debug ui
            debugToolStripMenuItem.Visible = true;
#endif

            //Zoom indicators and controls
            zoomStripComboBox.Items.AddRange(zoomScales.ToList().ConvertAll(scale => scale.ToString() + " %").ToArray());
            zoomStripComboBox.SelectedIndex = startZoomScaleIndex;
            lbZoom.Text = "Zoom: " + zoomStripComboBox.Text;

            //Size = Properties.Settings.Default.WindowSize;
            //WindowState = Properties.Settings.Default.WindowState;

            //Initialize signals draw controler
            signalsDraw = new SignalsDraw(ioSim);

            // Load code generation languages
            // TODO
        }

        private void AdjustSheetView()
        {
            ShadowTransform.Scale(ShadowScale, ShadowScale);
            CenterSheet();
            SetViewToPosition(pBox, new Point(hScroll.Value, vScroll.Value), DrawTransform);
            SetViewToPosition(pShadow, Point.Empty, ShadowTransform);
            CalculateScrolls();
        }

        private void pBox_MouseDown(object sender, MouseEventArgs e)
        {
            pBox.Focus();
            mouse.Location = Util.ScalePoint(e.Location, DrawTransform);
            if(e.Button == MouseButtons.Middle)
            {
                mouse.Scrolling = true;
                mouse.ScrollPoint = mouse.Location;
                pBox.Cursor = Cursors.SizeAll;
            }
            else if(e.Button == MouseButtons.Right)
            {
                mouse.SecundaryClickPoint = mouse.Location;
            }
            else if(e.Button == MouseButtons.Left)
            {
                if (mouse.SnapLocation == null)
                    mouse.FirstPoint = mouse.Location;
                else
                {
                    mouse.FirstPoint = mouse.SnapLocation.Value;
                    mouse.SnapLocation = null;
                }
                switch (mouse.Doing)
                {
                    case MouseTool.MouseDoing.Nothing:
                        Transition oTrans;
                        switch (mouse.CursorType)
                        {
                            case MouseTool.CursorTypes.Default:
                                //Sumar a la seleccion
                                if (ModifierKeys == Keys.Control)
                                {
                                    if (mouse.OnObject != null)
                                    {
                                        mouse.AddToSelection(mouse.OnObject);
                                        pBox.Refresh();
                                    }
                                    else if (mouse.OnTransition != null)
                                    {
                                        mouse.AddToSelection(mouse.OnTransition);
                                        pBox.Refresh();
                                    }
                                }
                                else
                                {
                                    if (mouse.OnTransition != null && !(mouse.OnObject is Origin))
                                    {
                                        mouse.ClearSelection();
                                        mouse.AddToSelection(mouse.OnTransition);
                                        propertyGrid.SelectedObject = mouse.OnTransition;
                                        if (!simulationMode)
                                        {
                                            if (mouse.OnTransition.IsTextSelectable(mouse.Location))
                                            {
                                                mouse.Doing = MouseTool.MouseDoing.MovingText;
                                            }
                                            else
                                            {
                                                mouse.Doing = MouseTool.MouseDoing.Moving;
                                            }
                                            mouse.MovingPoint = mouse.Location;
                                        }
                                        RefreshSelection(true);
                                    }
                                    else if (mouse.OnObject != null)
                                    {
                                        mouse.ClearSelection();
                                        mouse.AddToSelection(mouse.OnObject);
                                        propertyGrid.SelectedObject = mouse.OnObject;
                                        if (!simulationMode)
                                        {
                                            if (mouse.OnObject.IsTextSelectable(mouse.Location))
                                            {
                                                mouse.Doing = MouseTool.MouseDoing.MovingText;
                                            }
                                            else
                                            {
                                                mouse.Doing = MouseTool.MouseDoing.Moving;
                                            }
                                            mouse.MovingPoint = mouse.Location;
                                        }
                                        RefreshSelection(true);
                                    }
                                    else
                                    {
                                        mouse.ClearSelection();
                                        if (!simulationMode)
                                        {
                                            mouse.Doing = MouseTool.MouseDoing.Selecting;
                                            mouse.SelectionArea.Location = mouse.FirstPoint;
                                            mouse.SelectionArea.Size = Size.Empty;
                                        }
                                    }
                                }
                                pBox.Refresh();
                                break;
                            case MouseTool.CursorTypes.Move:
                                //Add to selection
                                if (ModifierKeys == Keys.Control)
                                {
                                    if (mouse.OnObject != null)
                                    {
                                        mouse.RemoveFromSelection(mouse.OnObject);
                                        pBox.Refresh();
                                    }
                                    else if (mouse.OnTransition != null)
                                    {
                                        mouse.RemoveFromSelection(mouse.OnTransition);
                                        pBox.Refresh();
                                    }
                                }
                                else
                                {
                                    if ((mouse.OnTransition != null && mouse.OnTransition.IsTextSelectable(mouse.Location))
                                        || (mouse.OnObject != null && mouse.OnObject.IsTextSelectable(mouse.Location)))
                                    {
                                        mouse.Doing = MouseTool.MouseDoing.MovingText;
                                    }
                                    else
                                    {
                                        mouse.Doing = MouseTool.MouseDoing.Moving;
                                        mouse.SelectionFocus(mouse.OnObject);
                                    }
                                    if (mouse.OnTransition != null)
                                    {
                                        propertyGrid.SelectedObject = mouse.OnTransition;
                                    }
                                    else if (mouse.OnObject != null)
                                    {
                                        propertyGrid.SelectedObject = mouse.OnObject;
                                    }
                                    mouse.MovingPoint = mouse.Location;
                                }
                                break;
                            case MouseTool.CursorTypes.Resize:
                                mouse.Doing = MouseTool.MouseDoing.Resizing;
                                mouse.MovingPoint = mouse.Location;
                                break;
                            case MouseTool.CursorTypes.Paint:
                                switch (mouse.DrawingObjectType)
                                {
                                    case DrawableObject.ObjectType.Text:
                                        mouse.OnObject = new Text(book.SelectedSheet.Sketch, new Rectangle(mouse.FirstPoint, Size.Empty));
                                        mouse.DrawingObject = mouse.OnObject;
                                        mouse.Doing = MouseTool.MouseDoing.Drawing;
                                        mouse.ClearSelection();
                                        break;
                                    case DrawableObject.ObjectType.Equation:
                                        mouse.OnObject = new Equation(book.SelectedSheet.Sketch, new Rectangle(mouse.FirstPoint, Size.Empty));
                                        mouse.DrawingObject = mouse.OnObject;
                                        mouse.Doing = MouseTool.MouseDoing.Drawing;
                                        mouse.ClearSelection();
                                        break;
                                    case DrawableObject.ObjectType.Origin:
                                    case DrawableObject.ObjectType.End:
                                    case DrawableObject.ObjectType.Alias:
                                    case DrawableObject.ObjectType.Abort:
                                    case DrawableObject.ObjectType.Relation:
                                        mouse.OnObject = Link.Create(mouse.DrawingObjectType, book.SelectedSheet.Sketch, mouse.FirstPoint);
                                        mouse.DrawingObject = mouse.OnObject;
                                        mouse.Doing = MouseTool.MouseDoing.Drawing;
                                        mouse.ClearSelection();
                                        break;
                                    case DrawableObject.ObjectType.SimpleState:
                                    case DrawableObject.ObjectType.StateAlias:
                                    case DrawableObject.ObjectType.SuperState:
                                    case DrawableObject.ObjectType.Nested:
                                        mouse.OnObject = State.Create(mouse.DrawingObjectType, book.SelectedSheet.Sketch, new Rectangle(mouse.FirstPoint, Size.Empty));
                                        mouse.DrawingObject = mouse.OnObject;
                                        mouse.Doing = MouseTool.MouseDoing.Drawing;
                                        mouse.ClearSelection();
                                        break;
                                    case DrawableObject.ObjectType.SimpleTransition:
                                        if (mouse.OnObject == null)
                                        {
                                            oTrans = new SimpleTransition(book.SelectedSheet.Sketch, new Point[] { mouse.FirstPoint, mouse.FirstPoint, mouse.FirstPoint, mouse.FirstPoint }, null);
                                        }
                                        else
                                        {
                                            if (mouse.OnObject is Origin)
                                            {
                                                Origin origin = (Origin)mouse.OnObject;
                                                if (origin.OutTransitions.Length == 0)
                                                {
                                                    oTrans = new SimpleTransition(book.SelectedSheet.Sketch, new Point[] { mouse.StartDrawPoint, mouse.OnObject.OutDir(mouse.StartDrawPoint, out dAngle), mouse.FirstPoint, mouse.FirstPoint }, mouse.OnObject);
                                                }
                                                else
                                                {
                                                    oTrans = new SimpleTransition(book.SelectedSheet.Sketch, new Point[] { mouse.FirstPoint, mouse.FirstPoint, mouse.FirstPoint, mouse.FirstPoint }, null);
                                                }
                                            }
                                            else
                                            {
                                                oTrans = new SimpleTransition(book.SelectedSheet.Sketch, new Point[] { mouse.StartDrawPoint, mouse.OnObject.OutDir(mouse.StartDrawPoint, out dAngle), mouse.FirstPoint, mouse.FirstPoint }, mouse.OnObject);
                                                oTrans.StartAngle = dAngle;
                                            }
                                        }
                                        mouse.DrawingObject = oTrans;
                                        mouse.Doing = MouseTool.MouseDoing.Drawing;
                                        mouse.ClearSelection();
                                        pBox_MouseMove(sender, e);
                                        break;
                                    case DrawableObject.ObjectType.SuperTransition:
                                        if (mouse.OnObject == null || mouse.OnObject is SimpleState || mouse.OnObject is Alias || mouse.OnObject is StateAlias)
                                        {
                                            oTrans = new SuperTransition(book.SelectedSheet.Sketch, new Point[] { mouse.FirstPoint, mouse.FirstPoint, mouse.FirstPoint, mouse.FirstPoint }, null);
                                        }
                                        else
                                        {
                                            if (mouse.OnObject is Origin)
                                            {
                                                Origin origin = (Origin)mouse.OnObject;
                                                if (origin.OutTransitions.Length == 0)
                                                {
                                                    oTrans = new SuperTransition(book.SelectedSheet.Sketch, new Point[] { mouse.StartDrawPoint, mouse.OnObject.OutDir(mouse.StartDrawPoint, out dAngle), mouse.FirstPoint, mouse.FirstPoint }, mouse.OnObject);
                                                }
                                                else
                                                {
                                                    oTrans = new SuperTransition(book.SelectedSheet.Sketch, new Point[] { mouse.FirstPoint, mouse.FirstPoint, mouse.FirstPoint, mouse.FirstPoint }, null);
                                                }
                                            }
                                            else
                                            {
                                                oTrans = new SuperTransition(book.SelectedSheet.Sketch, new Point[] { mouse.StartDrawPoint, mouse.OnObject.OutDir(mouse.StartDrawPoint, out dAngle), mouse.FirstPoint, mouse.FirstPoint }, mouse.OnObject);
                                                oTrans.StartAngle = dAngle;
                                            }
                                        }
                                        mouse.DrawingObject = oTrans;
                                        mouse.Doing = MouseTool.MouseDoing.Drawing;
                                        mouse.ClearSelection();
                                        pBox_MouseMove(sender, e);
                                        break;
                                }
                                pBox.Refresh();
                                break;
                        }
                        break;
                }
            }
        }

        private void pBox_MouseUp(object sender, MouseEventArgs e)
        {
            mouse.Location = Util.ScalePoint(e.Location, DrawTransform);
            if(e.Button == MouseButtons.Middle)
            {
                mouse.Scrolling = false;
            }
            else if(e.Button == MouseButtons.Left)
            {
                switch (mouse.Doing)
                {
                    case MouseTool.MouseDoing.Drawing:
                        mouse.ClearSelection();
                        mouse.AddToSelection(mouse.DrawingObject);
                        Point newSize;
                        switch (mouse.DrawingObjectType)
                        {
                            case DrawableObject.ObjectType.SimpleTransition:
                            case DrawableObject.ObjectType.SuperTransition:
                                var oTrans = mouse.DrawingObject as Transition;
                                oTrans.Name = book.SelectedSheet.NextObjectName(oTrans.GetFormName());
                                if (mouse.OnObject != null && !(mouse.OnObject is Origin))
                                {
                                    oTrans.EndObject = mouse.OnObject;
                                }
                                oTrans.SizeCheckAndFix();
                                break;
                            case DrawableObject.ObjectType.Origin:
                                var origin = mouse.DrawingObject as Origin;
                                origin.Name = book.SelectedSheet.NextObjectName(origin.GetFormName());
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.End:
                            case DrawableObject.ObjectType.Alias:
                            case DrawableObject.ObjectType.Abort:
                                var link = mouse.DrawingObject as Link;
                                link.Name = link.GetFormName();
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.StateAlias:
                                var sAlias = mouse.DrawingObject as State;
                                sAlias.Name = sAlias.GetFormName();
                                newSize = new Point(0, 0);
                                sAlias.ResizeCheck(ref newSize, MouseTool.ResizingTypes.Right_Bottom);
                                sAlias.Resize(newSize, MouseTool.ResizingTypes.Right_Bottom);
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.SimpleState:
                            case DrawableObject.ObjectType.SuperState:
                            case DrawableObject.ObjectType.Nested:
                                var oState = mouse.DrawingObject as State;
                                oState.Name = book.SelectedSheet.NextObjectName(oState.GetFormName());
                                newSize = new Point(0, 0);
                                oState.ResizeCheck(ref newSize, MouseTool.ResizingTypes.Right_Bottom);
                                oState.Resize(newSize, MouseTool.ResizingTypes.Right_Bottom);
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.Text:
                                var oText = mouse.DrawingObject as Text;
                                oText.Name = book.SelectedSheet.NextObjectName(oText.GetFormName());
                                oText.Description = oText.Name;
                                newSize = new Point(TextRenderer.MeasureText(oText.Description, DrawableObject.font).Width, 0);
                                oText.ResizeCheck(ref newSize, MouseTool.ResizingTypes.Right_Bottom);
                                oText.Resize(newSize, MouseTool.ResizingTypes.Right_Bottom);
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.Relation:
                                var oRelation = mouse.DrawingObject as Relation;
                                oRelation.Name = book.SelectedSheet.NextObjectName(oRelation.GetFormName());
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.Equation:
                                var oEquation = mouse.DrawingObject as Equation;
                                oEquation.Name = book.SelectedSheet.NextObjectName(oEquation.GetFormName());
                                newSize = new Point(0, 0);
                                oEquation.ResizeCheck(ref newSize, MouseTool.ResizingTypes.Right_Bottom);
                                oEquation.Resize(newSize, MouseTool.ResizingTypes.Right_Bottom);
                                pBox.Refresh();
                                break;
                            default:
                                throw new Exception("Unhandled draw.");
                        }
                        propertyGrid.SelectedObject = mouse.DrawingObject;
                        mouse.DrawingObject = null;
                        mouse.OnObject = null;
                        mouse.Doing = MouseTool.MouseDoing.Nothing;
                        if(!blockMouseTool) btMouseTool_Click(btToolMouse, null);
                        AddAction(RecordableAction.ActionTypes.Create);   //Add action for undo/redo
                        pBox.Refresh();
                        RefreshSelection(true);
                        break;
                    case MouseTool.MouseDoing.Selecting:
                        RefreshSelection(mouse.SelectionResult(mouse.FirstPoint, mouse.Location));
                        mouse.SelectionArea = Rectangle.Empty;
                        mouse.Doing = MouseTool.MouseDoing.Nothing;
                        pBox.Refresh();
                        break;
                    case MouseTool.MouseDoing.Moving:
                        if (mouse.OnObject != null) mouse.OnObject.Moved();
                        mouse.Doing = MouseTool.MouseDoing.Nothing;
                        if (mouse.FirstPoint != mouse.Location)
                        {
                            AddAction(RecordableAction.ActionTypes.Move); //Add action for undo/redo
                        }
                        else if(mouse.OnObject is SuperState super && Control.ModifierKeys == Keys.Shift)
                        {
                            var list = super.ContainedObjects;
                            list.Add(mouse.OnObject);
                            mouse.SetSelection(list, list.IndexOf(mouse.OnObject));
                        }
                        else if(mouse.OnObject is Text text)
                        {
                            //TODO: Edit text
                        }
                        pBox.Refresh();
                        break;
                    case MouseTool.MouseDoing.MovingText:
                        mouse.Doing = MouseTool.MouseDoing.Nothing;
                        if (mouse.FirstPoint != mouse.Location)
                        {
                            AddAction(RecordableAction.ActionTypes.MoveText); //Add action for undo/redo
                        }
                        pBox.Refresh();
                        break;
                    case MouseTool.MouseDoing.Resizing:
                        mouse.Doing = MouseTool.MouseDoing.Nothing;
                        mouse.OnObject = null;
                        AddAction(RecordableAction.ActionTypes.Resize); //Add action for undo/redo
                        pBox.Refresh();
                        break;
                }
            }
        }

        private void pBox_MouseMove(object sender, MouseEventArgs e)
        {
            lastMouseState = e;
            mouse.Location = Util.ScalePoint(e.Location, DrawTransform);
            if (mouse.Scrolling)
            {
                ScrollOffset(mouse.Location.X - mouse.ScrollPoint.X, mouse.Location.Y - mouse.ScrollPoint.Y);
            }
            else
                switch (mouse.Doing)
                {
                    case MouseTool.MouseDoing.Nothing:
                        mouse.PreviousObject = mouse.OnObject;
                        mouse.OnObject = book.SelectedSheet.Sketch.GetOnObject(mouse.Location);
                        mouse.OnTransition = book.SelectedSheet.Sketch.OnTransition(mouse.Location);
                        if (mouse.CursorType == MouseTool.CursorTypes.Paint)
                        {
                            if (ModifierKeys.HasFlag(ForceSnapCursorKey) ^ mnuSnapToGrid.Checked)
                            {
                                mouse.SnapLocation = Util.SnapPoint(mouse.Location, DrawingSheet.gridSeparation);
                            }
                            else
                            {
                                mouse.SnapLocation = null;
                            }
                            pBox.Invalidate();
                        }
                        else if (!simulationMode)
                        {
                            pBox.Cursor = mouse.Moving(mouse.Location, DrawTransform);
                        }
                        switch (mouse.DrawingObjectType)
                        {
                            case DrawableObject.ObjectType.SimpleTransition:
                                if (mouse.OnObject != null)
                                {
                                    if (mouse.OnObject is Origin)
                                    {
                                        Origin origin = (Origin)mouse.OnObject;
                                        if (origin.OutTransitions.Length == 0)
                                        {
                                            mouse.StartDrawPoint = origin.Location;
                                        }
                                        else
                                        {
                                            mouse.StartDrawPoint = Point.Empty;
                                            mouse.OnObject = null;
                                        }
                                    }
                                    else if (mouse.OnObject is End || mouse.OnObject is Abort || mouse.OnObject is Relation)
                                    {
                                        mouse.StartDrawPoint = Point.Empty;
                                        mouse.OnObject = null;
                                    }
                                    else
                                    {
                                        mouse.OnObject.Intersect(mouse.Location, ref mouse.StartDrawPoint, ref dAngle);
                                    }
                                }
                                else
                                {
                                    mouse.StartDrawPoint = Point.Empty;
                                }
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.SuperTransition:
                                if (mouse.OnObject != null)
                                {
                                    if (mouse.OnObject is Origin)
                                    {
                                        Origin origin = (Origin)mouse.OnObject;
                                        if (origin.OutTransitions.Length == 0)
                                        {
                                            mouse.StartDrawPoint = origin.Location;
                                        }
                                        else
                                        {
                                            mouse.StartDrawPoint = Point.Empty;
                                            mouse.OnObject = null;
                                        }
                                    }
                                    else if (mouse.OnObject is End || mouse.OnObject is Abort)
                                    {
                                        mouse.StartDrawPoint = Point.Empty;
                                        mouse.OnObject = null;
                                    }
                                    else
                                    {
                                        mouse.OnObject.Intersect(mouse.Location, ref mouse.StartDrawPoint, ref dAngle);
                                    }
                                }
                                else
                                {
                                    mouse.StartDrawPoint = Point.Empty;
                                }
                                pBox.Refresh();
                                break;
                        }
                        break;
                    case MouseTool.MouseDoing.Drawing:
                        if (ModifierKeys.HasFlag(ForceSnapCursorKey) ^ mnuSnapToGrid.Checked)
                        {
                            mouse.Location = Util.SnapPoint(mouse.Location, DrawingSheet.gridSeparation);
                        }
                        mouse.PreviousObject = mouse.OnObject;
                        mouse.OnObject = book.SelectedSheet.Sketch.GetOnObject(mouse.Location);
                        switch (mouse.DrawingObjectType)
                        {
                            case DrawableObject.ObjectType.SimpleTransition:
                            case DrawableObject.ObjectType.SuperTransition:
                                if (mouse.OnObject != null)
                                {
                                    if (mouse.OnObject is Origin || mouse.OnObject is Relation)
                                    {
                                        mouse.OnObject = null;
                                    }
                                    else
                                    {
                                        Transition transition = (Transition)mouse.DrawingObject;
                                        double prev = transition.EndAngle;
                                        mouse.OnObject.Intersect(mouse.Location, ref mouse.StartDrawPoint, ref transition.EndAngle);
                                        if (mouse.StartDrawPoint.X > 1000 || mouse.StartDrawPoint.X < -1000) System.Diagnostics.Debugger.Break();
                                        mouse.DrawingObject.DrawingRectangle(mouse.FirstPoint, mouse.StartDrawPoint);
                                        transition.OutDir(mouse.OnObject.OutDir(mouse.StartDrawPoint, out dAngle), 2);
                                    }
                                }
                                else
                                {
                                    mouse.DrawingObject.DrawingRectangle(mouse.FirstPoint, mouse.Location);
                                    if (mouse.DrawingObject is Transition trans) trans.ForceStraight = ModifierKeys.HasFlag(ForceStraightLineKey);
                                }
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.SimpleState:
                                var state = mouse.DrawingObject as SimpleState;
                                Point endPoint;
                                if (ModifierKeys.HasFlag(ForceStateCircleKey)) endPoint = Util.GetDiagonal(mouse.FirstPoint, mouse.Location);
                                else endPoint = mouse.Location;
                                mouse.DrawingObject.DrawingRectangle(mouse.FirstPoint, endPoint);
                                state.ForceCircle = ModifierKeys.HasFlag(ForceStateCircleKey);
                                pBox.Refresh();
                                break;
                            case DrawableObject.ObjectType.Origin:
                            case DrawableObject.ObjectType.Relation:
                            case DrawableObject.ObjectType.End:
                            case DrawableObject.ObjectType.Alias:
                            case DrawableObject.ObjectType.Abort:
                            case DrawableObject.ObjectType.StateAlias:
                            case DrawableObject.ObjectType.SuperState:
                            case DrawableObject.ObjectType.Nested:
                            case DrawableObject.ObjectType.Text:
                            case DrawableObject.ObjectType.Equation:
                                mouse.DrawingObject.DrawingRectangle(mouse.FirstPoint, mouse.Location);
                                pBox.Refresh();
                                break;
                        }
                        break;
                    case MouseTool.MouseDoing.Selecting:
                        mouse.SelectionArea = Util.GetRectangle(mouse.FirstPoint, mouse.Location);
                        mouse.InclusiveSelection = mouse.FirstPoint.X > mouse.Location.X;
                        RefreshSelection(mouse.SelectionResult(mouse.FirstPoint, mouse.Location));
                        pBox.Refresh();
                        break;
                    case MouseTool.MouseDoing.Moving:
                        if (ModifierKeys.HasFlag(ForceSnapCursorKey) ^ mnuSnapToGrid.Checked)
                        {
                            if (mouse.SelectionFocusState == null)
                            {
                                mouse.Location = Util.SnapPoint(mouse.FirstPoint, mouse.Location, DrawingSheet.gridSeparation);
                            }
                            else
                            {
                                mouse.Location = Util.SnapPoint(mouse.SelectionFocusState, mouse.FirstPoint, mouse.Location, DrawingSheet.gridSeparation);
                            }
                        }
                        mouse.MoveObjects(mouse.Location);
                        ScrollOffset(Util.GetOutOffset(GetRectangleView(), mouse.Location));
                        pBox.Refresh();
                        break;
                    case MouseTool.MouseDoing.MovingText:
                        mouse.MoveText(mouse.Location);
                        pBox.Refresh();
                        break;
                    case MouseTool.MouseDoing.Resizing:
                        if (ModifierKeys.HasFlag(ForceSnapCursorKey) ^ mnuSnapToGrid.Checked)
                        {
                            mouse.Location = Util.SnapGripPoint(mouse.Location, DrawingSheet.gridSeparation);
                        }
                        mouse.ResizeObjects(mouse.Location);
                        pBox.Refresh();
                        break;
                }
            RefreshStatusBar(mouse.Location);
        }

        private void pBox_DoubleClick(object sender, EventArgs e)
        {
            if (mouse.OnObject is Nested)
            {
                var nested = (Nested)mouse.OnObject;
                var sheet = book.Sheets.Find(sh => sh.Name == nested.PointingTo);
                if(sheet != null)
                {
                    book.SelectedSheet = sheet;
                    mouse.ClearSelection();
                    mouse.draw = book.SelectedSheet.Sketch;
                    tvObjects.SelectedNode = book.SelectedSheet.sheetTree;
                }
            }
        }

        //propertyGrid, tvObjects
        private void controls_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '\x1b':    //Escape
                    pBox.Focus();
                    e.Handled = true;
                    break;
            }
        }

        private void PBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Escape:
                    switch (mouse.Doing)
                    {
                        case MouseTool.MouseDoing.Nothing:
                            blockMouseTool = false;
                            if (mouse.CursorType != MouseTool.CursorTypes.Default) btMouseTool_Click(btToolMouse, null);
                            break;
                        case MouseTool.MouseDoing.Drawing:
                            mouse.Doing = MouseTool.MouseDoing.Nothing;
                            mouse.DrawingObject = null;
                            pBox.Refresh();
                            break;
                        case MouseTool.MouseDoing.Moving:
                        case MouseTool.MouseDoing.Resizing:
                        case MouseTool.MouseDoing.MovingText:
                            mouse.Doing = MouseTool.MouseDoing.Nothing;
                            book.SelectedSheet.Sketch.CancelAction(RecordableAction.ActionTypes.Move, mouse.ChangingObjects);
                            pBox.Refresh();
                            break;
                    }
                    break;
                case Keys.Delete:
                    btDelete_Click(pBox, null);
                    break;
            }
        }

        private void RefreshStatusBar(Point Location)
        {
            lbPosX.Text = "Position: " + Location.ToString();
            if (mouse.OnTransition != null)
                lbObject.Text = "Object: " + mouse.OnTransition.Name;
            else if (mouse.OnObject != null)
                lbObject.Text = "Object: " + mouse.OnObject.Name;
            else
                lbObject.Text = "Object: Nothing";
        }

        private void CenterSheet()
        {
            ScrollTo(0, 0);
        }

        private void ScrollTo(int horiz, int vert)
        {
            hScroll.Value = Util.Adjust(horiz, hScroll.Minimum, hScroll.Maximum - hScroll.LargeChange + 1);
            vScroll.Value = Util.Adjust(vert, vScroll.Minimum, vScroll.Maximum - vScroll.LargeChange + 1);
        }

        private void ScrollTo(Point point)
        {
            ScrollTo(point.X, point.Y);
        }

        private void ScrollOffset(int offsetX, int offsetY)
        {
            ScrollTo(hScroll.Value - offsetX, vScroll.Value - offsetY);
        }

        private void ScrollOffset(Point point)
        {
            ScrollOffset(point.X, point.Y);
        }

        static internal void SetViewToPosition(PictureBox pic, Point position, Matrix transform)
        {
            var point = Util.ScalePoint(new Point(pic.Width / 2, pic.Height / 2), transform);
            point.Offset(-position.X, -position.Y);
            transform.Translate(point.X, point.Y);
        }

        private Point GetViewCenterPoint()
        {
            return Util.ScalePoint(new Point(pBox.Width / 2, pBox.Height / 2), DrawTransform);
        }

        private void CalculateScrolls()
        {
            var corner1 = Util.ScalePoint(Point.Empty, ShadowTransform);
            var corner2 = Util.ScalePoint(new Point(pShadow.Size), ShadowTransform);
            hScroll.Minimum = corner1.X;
            hScroll.Maximum = corner2.X + hScroll.LargeChange - 1;
            vScroll.Minimum = corner1.Y;
            vScroll.Maximum = corner2.Y + vScroll.LargeChange - 1;
        }

        private void pBox_MouseWheel(object sender, MouseEventArgs e)
        {
            mouse.Location = Util.ScalePoint(e.Location, DrawTransform);
            if (ModifierKeys == Keys.Control)
            {
                ZoomScroll(e.Location, e.Delta > 0);
                var center = GetViewCenterPoint();
                ScrollTo(center);
                RefreshStatusBar(mouse.Location);
            }
            else
            {
                mouse.Location.Y -= e.Delta / 4;
                switch (mouse.Doing)
                {
                    case MouseTool.MouseDoing.Moving:
                        mouse.MoveObjects(mouse.Location);
                        break;
                }
                ScrollOffset(0, e.Delta / 4);
            }
        }

        private void pShadow_MouseWheel(object sender, MouseEventArgs e)
        {
            ZoomScroll(Util.Middle(pBox.Size), e.Delta > 0);
            pBox.Refresh();
            pShadow.Refresh();
        }

        private void hvScrolls_ValueChanged(object sender, EventArgs e)
        {
            SetViewToPosition(pBox, new Point(hScroll.Value, vScroll.Value), DrawTransform);
            pBox.Refresh();
            pShadow.Refresh();
            lbFocus.Text = hScroll.Value.ToString() + ", " + vScroll.Value.ToString();
        }

        private void pBox_Resize(object sender, EventArgs e)
        {
            Size Offset = Size.Subtract(pBox.Size, PictureSize);
            DrawTransform.Translate((float)Offset.Width / 2f, (float)Offset.Height / 2f);
            PictureSize = pBox.Size;
        }

        private bool zoomLock = false;
        private void zoomStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (zoomLock) return;
            float newScale = zoomScales[zoomStripComboBox.SelectedIndex] / 100;
            AdjustScale(Util.Middle(pBox.Size), newScale);
            pBox.Refresh();
            pShadow.Refresh();
        }

        private void ZoomScroll(Point fromPoint, bool zoomIn)
        {
            zoomLock = true;
            if (zoomIn)
            {
                if (zoomStripComboBox.SelectedIndex < zoomStripComboBox.Items.Count - 1) zoomStripComboBox.SelectedIndex++;
            }
            else
            {
                if (zoomStripComboBox.SelectedIndex > 0) zoomStripComboBox.SelectedIndex--;
            }
            zoomLock = false;
            float newScale = zoomScales[zoomStripComboBox.SelectedIndex] / 100;
            AdjustScale(fromPoint, newScale);
        }
        
        private void AdjustScale(Point fromPoint, float newScale)
        {
            if (newScale != DrawScale)
            {
                var location = Util.Adjust(fromPoint, Util.UnscalePoint(Util.ScalePoint(Point.Empty, ShadowTransform), DrawTransform), Util.UnscalePoint(Util.ScalePoint(new Point(pShadow.Size), ShadowTransform), DrawTransform));
                float adjust = newScale / DrawScale;
                DrawScale = newScale;

                // Translate mouse point to origin
                DrawTransform.Translate(-location.X, -location.Y, MatrixOrder.Append);

                // Scale view
                DrawTransform.Scale(adjust, adjust, MatrixOrder.Append);

                // Translate origin back to original mouse point.
                DrawTransform.Translate(location.X, location.Y, MatrixOrder.Append);
            }
            lbZoom.Text = "Zoom: " + zoomScales[zoomStripComboBox.SelectedIndex].ToString() + " %";
        }

        private void pBox_Paint(object sender, PaintEventArgs e)
        {
            DrawToGraphics(e.Graphics);
        }

        private void DrawToGraphics(Graphics g)
        {
            //Scaling
            g.Transform = DrawTransform;
            g.SmoothingMode = SmoothingMode.HighQuality;

            //Draw sheet
            book.SelectedSheet.Draw(g);
            book.SelectedSheet.DrawFeatures(g, DrawScale);

            ////Visual center circle
            //Point center = Util.ScalePoint(new Point(pBox.Width / 2, pBox.Height / 2), DrawTransform);
            //Size size = Util.ScaleSize(new Size(10, 10), DrawScale);
            //e.Graphics.DrawEllipse(Pens.LightGreen, center.X - size.Width, center.Y - size.Height, size.Width * 2, size.Height * 2);

            Pen pen = new Pen(Color.Black, 1.8f);

            //Draw objects
            book.SelectedSheet.Sketch.PaintShadow(g, new DrawAttributes(Pens.WhiteSmoke, DrawScale, true));
            mouse.DrawSelectionsBack(g);
            book.SelectedSheet.Sketch.Paint(g, new DrawAttributes(pen, DrawScale));
            mouse.DrawSelections(g, DrawTransform);

            //Draw snap cursor
            if (mouse.SnapLocation != null)
            {
                const int crossSize = 15;
                g.DrawLine(Pens.Black, mouse.SnapLocation.Value.X - crossSize, mouse.SnapLocation.Value.Y, mouse.SnapLocation.Value.X + crossSize, mouse.SnapLocation.Value.Y);
                g.DrawLine(Pens.Black, mouse.SnapLocation.Value.X, mouse.SnapLocation.Value.Y - crossSize, mouse.SnapLocation.Value.X, mouse.SnapLocation.Value.Y + crossSize);
            }
        }

        private Rectangle GetRectangleView() => new Rectangle(Util.ScalePoint(Point.Empty, DrawTransform), Util.ScaleSize(pBox.Size, DrawScale));

        private void pShadow_Paint(object sender, PaintEventArgs e)
        {
            //Scaling
            e.Graphics.Transform = ShadowTransform;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //Draw Sheet
            book.SelectedSheet.Draw(e.Graphics);

            //Draw objects
            book.SelectedSheet.Sketch.PaintShadow(e.Graphics, new DrawAttributes(Pens.Black, ShadowScale, true));

            //Draw rectangle view
            Rectangle rect = GetRectangleView();
            e.Graphics.DrawRectangle(Pens.LightGray, rect);
            //Draw center cross
            e.Graphics.DrawLine(Pens.Gray, rect.X + rect.Width / 2 - 50, rect.Y + rect.Height / 2, rect.X + rect.Width / 2 + 50, rect.Y + rect.Height / 2);
            e.Graphics.DrawLine(Pens.Gray, rect.X + rect.Width / 2, rect.Y + rect.Height / 2 - 50, rect.X + rect.Width / 2, rect.Y + rect.Height / 2 + 50);

            //Draw new rectangle view
            if (drawShadowView)
            {
                Point corner = ShadowPosition;
                corner.Offset(-rect.Width / 2, -rect.Height / 2);
                Rectangle srect = new Rectangle(corner, rect.Size);
                e.Graphics.DrawRectangle(Pens.Blue, srect);
            }
        }

        private bool drawShadowView = false;
        private Point ShadowPosition = Point.Empty;
        private void pShadow_MouseMove(object sender, MouseEventArgs e)
        {
            ShadowPosition = Util.ScalePoint(Util.Adjust(e.Location, new Rectangle(Point.Empty, pShadow.Size)), ShadowTransform);

            if(e.Button == MouseButtons.Left)
            {
                ScrollTo(ShadowPosition);
            }

            pShadow.Refresh();

            //Show location
            RefreshStatusBar(ShadowPosition);
        }

        private void pShadow_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                drawShadowView = true;
                ScrollTo(ShadowPosition);
            }
        }

        private void pShadow_MouseUp(object sender, MouseEventArgs e)
        {
            drawShadowView = false;
            pShadow.Refresh();
        }

        private void pShadow_MouseEnter(object sender, EventArgs e)
        {
            
        }

        private void pShadow_MouseLeave(object sender, EventArgs e)
        {

        }

        private void btMouseTool_Click(object sender, EventArgs e)
        {
            ToolStripButton[] bts = {btToolMouse, btToolOrigin, btToolEnd, btToolAlias, btToolEpAlias, btToolAbort, btToolState, btToolTransition, btToolSuperState, btToolSuperTransition, btToolNested, btToolText };
            ToolStripButton bt = (ToolStripButton)sender;
            blockMouseTool = false;
            foreach (ToolStripButton btn in bts)
            {
                btn.Checked = btn == bt;
            }
            mouse.DrawingObjectType = (DrawableObject.ObjectType)Array.IndexOf(Enum.GetNames(typeof(DrawableObject.ObjectType)), bt.Tag);
            if(bt == btToolMouse) mouse.CursorType = MouseTool.CursorTypes.Default;
            else mouse.CursorType = MouseTool.CursorTypes.Paint;
            if (e != null)
            {
                mouse.ClearSelection();
            }
            pBox.Refresh();
            pBox.Focus();
        }

        private void btMouseTool_DoubleClick(object sender, EventArgs e)
        {
            blockMouseTool = true;
        }

        private bool DoAction(RecordableAction.ActionTypes actionType)
        {
            bool save = true;
            switch (actionType)
            {
                case RecordableAction.ActionTypes.AddSheet:
                    book.CreateChildSheet();
                    save = false;
                    break;
                case RecordableAction.ActionTypes.AddModel:
                    book.CreateModel();
                    save = false;
                    break;
                case RecordableAction.ActionTypes.DeleteSheet:
                    book.DeleteActiveChildSheet();
                    save = false;
                    break;
                case RecordableAction.ActionTypes.VariablesChanged:
                    byte[] before = book.Variables.Serialize();
                    fVariables fvar;
                    if (book.SelectedSheet is ModelSheet model)
                    {
                        fvar = new fVariables(model.Variables);
                        if (fvar.ShowDialog() == DialogResult.OK) book.ModelVariablesChanged(before); else save = false;
                    }
                    else
                    {
                        fvar = new fVariables(book.Variables);
                        if (fvar.ShowDialog() == DialogResult.OK) book.VariablesChanged(before); else save = false;
                    }
                    break;
                case RecordableAction.ActionTypes.Create:
                    book.AddDrawAction(actionType, book.SelectedSheet, mouse.ChangingObjects, mouse.SelectedObjects, mouse.SelectionFocusIndex);
                    break;
                case RecordableAction.ActionTypes.MoveText:
                    var list = new List<DrawableObject>();
                    if (mouse.OnTransition != null)
                    {
                        list.Add(mouse.OnTransition);
                    }
                    else
                    {
                        list.Add(mouse.OnObject);
                    }
                    book.AddDrawAction(actionType, book.SelectedSheet, list, mouse.SelectedObjects, mouse.SelectionFocusIndex);
                    break;
                case RecordableAction.ActionTypes.Paste:
                case RecordableAction.ActionTypes.PropertyChanged:
                    book.AddDrawAction(actionType, book.SelectedSheet, mouse.ChangingObjects, mouse.SelectedObjects, mouse.SelectionFocusIndex);
                    break;
                case RecordableAction.ActionTypes.Cut:
                case RecordableAction.ActionTypes.Remove:
                default:
                    book.AddDrawAction(actionType, book.SelectedSheet, mouse.ChangingObjects, mouse.SelectedObjects, mouse.SelectionFocusIndex);
                    break;
            }
            CheckDiagram();
            return save;
        }

        private void AddAction(RecordableAction.ActionTypes actionType)
        {
            if (!DoAction(actionType)) return;
            //Update buttons
            undoToolStripMenuItem.Enabled = true;
            btUndo.ToolTipText = book.UndoText();
            btRedo.ToolTipText = book.RedoText();
            redoToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = true;
            pShadow.Refresh();
            propertyGrid.Refresh();
            if (viewer != null && !viewer.IsDisposed) viewer.RefreshActions();
        }

        private void btUndo_Click(object sender, EventArgs e)
        {
            undoToolStripMenuItem.Enabled = book.Undo(mouse);
            btUndo.ToolTipText = book.UndoText();
            btRedo.ToolTipText = book.RedoText();
            redoToolStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = !book.IsSavedAction();
            pBox.Refresh();
            pShadow.Refresh();
            propertyGrid.Refresh();
        }

        private void btRedo_Click(object sender, EventArgs e)
        {
            redoToolStripMenuItem.Enabled = book.Redo(mouse);
            btUndo.ToolTipText = book.UndoText();
            btRedo.ToolTipText = book.RedoText();
            undoToolStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = !book.IsSavedAction();
            pBox.Refresh();
            pShadow.Refresh();
            propertyGrid.Refresh();
        }

        private void RefreshSelection(bool selectedState)
        {
            /// Edit menu
            //Cut
            cutToolStripMenuItem.Enabled = selectedState;
            //Copy
            copyToolStripMenuItem.Enabled = selectedState;
            //Delete
            deleteToolStripMenuItem.Enabled = selectedState;

            /// Format menu
            // Size
            sizeToolStripMenuItem.Enabled = mouse.SelectedObjects.Count(obj => !(obj is Transition)) >= 2;
            //Order
            orderToolStripMenuItem.Enabled = selectedState;
        }

        private void btCut_Click(object sender, EventArgs e)
        {
            Clipboard.SetData("Phases.Copy", mouse.SerializeSelection());
            AddAction(RecordableAction.ActionTypes.Cut);
            //book.SelectedSheet.draw.RemoveObjects(mouse.SelectedObjects);
            mouse.ClearSelection();
            pBox.Refresh();
            RefreshSelection(false);
            pasteToolStripMenuItem.Enabled = true;
        }

        private void btCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetData("Phases.Copy", mouse.SerializeSelection());
            pasteToolStripMenuItem.Enabled = true;
        }

        private void fDraw_Activated(object sender, EventArgs e)
        {
            pasteToolStripMenuItem.Enabled = Clipboard.GetData("Phases.Copy") != null;
        }

        private void btPaste_Click(object sender, EventArgs e)
        {
            var paste = (byte[])Clipboard.GetData("Phases.Copy");
            if (paste == null) return;
            if (mouse.DeserializeSelection(paste, sender == pasteContextMenu, mouse.Location, book.SelectedSheet.NextObjectName))
            {
                AddAction(RecordableAction.ActionTypes.Paste);
            }
            else
            {
                MessageBox.Show("Data corrupted.", "Error pasting");
            }
            pBox.Refresh();
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (propertyGrid.ContainsFocus) return;
            AddAction(RecordableAction.ActionTypes.Remove);
            //book.SelectedSheet.draw.RemoveObjects(mouse.SelectedObjects);
            mouse.ClearSelection();
            pBox.Refresh();
            propertyGrid.SelectedObject = null;
            RefreshSelection(false);
        }

        private void SaveToFile(string path)
        {
#if !DEBUG
            try
            {
#endif
                File.WriteAllBytes(path, book.Serialize());
                book.MarkSavedAction();
                saveToolStripMenuItem.Enabled = false;

#if !DEBUG
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message);
            }
#endif
        }

        private bool ReadFromFile(string path)
        {
            book = new PhasesBook(appInterface, DrawingSheet.DefaultSize);
            mouse = new MouseTool(book.SelectedSheet.Sketch, pBox);
            byte[] data;
            bool res;
            try
            {
                data = File.ReadAllBytes(path);
                res = book.Deserialize(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error openning file: " + ex.Message);
                return false;
            }
            return res;
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (FileName == "")
            {
                btSaveAs_Click(null, null);
            }
            else
            {
                SaveToFile(FileName);
                saveResult = DialogResult.OK;
            }
        }

        private void btOpen_Click(object sender, EventArgs e)
        {
            if (!book.IsSavedAction())
            {
                var result = MessageBox.Show("The current document has unsaved changes, do you want to save it before open a new one?", "Save document?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    btSave_Click(null, null);
                    if (saveResult != DialogResult.OK) return;
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
            }
            if (dialogOpen.ShowDialog() == DialogResult.OK)
            {
                if (ReadFromFile(dialogOpen.FileName))
                {
                    FileName = dialogOpen.FileName;
                    Text = "Phases FSM - " + Path.GetFileName(FileName);
                    profile.ProjectName = Path.GetFileNameWithoutExtension(FileName);
                    profile.Path = Path.GetDirectoryName(FileName);

                    mouse.draw = book.SelectedSheet.Sketch;
                    propertyGrid.SelectedObject = book.SelectedSheet;
                    undoToolStripMenuItem.Enabled = false;
                    redoToolStripMenuItem.Enabled = false;
                    saveToolStripMenuItem.Enabled = false;
                    msgList.Items.Clear();
                    pBox.Refresh();
                    pShadow.Refresh();

                    //language settings
                    if(book.Language == "Cottle")
                    {
                        lbLanguage.Text = book.TargetLanguage;
                    }
                    else
                    {
                        lbLanguage.Text = book.Language;
                    }
                }
                else
                {
                    MessageBox.Show("The file is damaged.", "File error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void btNew_Click(object sender, EventArgs e)
        {
            if (!book.IsSavedAction())
            {
                var result = MessageBox.Show("The current document has unsaved changes, do you want to save it before create a new one?", "Save document?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    btSave_Click(null, null);
                    if (saveResult != DialogResult.OK) return;
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
            }
            propertyGrid.SelectedObject = null;
            book = new PhasesBook(appInterface, DrawingSheet.DefaultSize);
            mouse = new MouseTool(book.SelectedSheet.Sketch, pBox);
            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
            Text = "Phases FSM";
            FileName = "";
            msgList.Items.Clear();
            pShadow.Refresh();
            pBox.Refresh();
            saveToolStripMenuItem.Enabled = false;
        }

        private void ClearDraw()
        {
            book.SelectedSheet.Sketch.Clear(tvObjects);
            mouse = new MouseTool(book.SelectedSheet.Sketch, pBox);
            saveToolStripMenuItem.Enabled = false;
            pBox.Refresh();
            pShadow.Refresh();
        }

        private void btSaveAs_Click(object sender, EventArgs e)
        {
            saveResult = dialogSave.ShowDialog();
            if (saveResult == DialogResult.OK)
            {
                FileName = dialogSave.FileName;
                Text = "Phases FSM - " + Path.GetFileName(FileName);
                profile.ProjectName = Path.GetFileNameWithoutExtension(FileName);
                profile.Path = Path.GetDirectoryName(FileName);

                SaveToFile(FileName);
            }
        }

        private void btSelectAll_Click(object sender, EventArgs e)
        {
            mouse.SetSelection(book.SelectedSheet.Sketch.Objects);
            pBox.Refresh();
        }

        private void sameWidthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debugger.Break();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Value != e.OldValue) AddAction(RecordableAction.ActionTypes.PropertyChanged);
            pBox.Refresh();
        }

        private void fDraw_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!book.IsSavedAction())
            {
                var result = MessageBox.Show("The current document has unsaved changes, do you want to save it before close the application?", "Save document?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    btSave_Click(null, null);
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            Dispose();
        }

        private void generalToolStripMenuItem_EnabledChanged(object sender, EventArgs e)
        {
            var menu = (ToolStripMenuItem)sender;
            var button = (ToolStripButton)menu.Tag;
            button.Enabled = menu.Enabled;
        }

        private void centerSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CenterSheet();
            pBox.Refresh();
            pShadow.Refresh();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void gridOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem[] bts = { gridHideToolStripMenuItem, gridPointsToolStripMenuItem, gridSquaresToolStripMenuItem };
            var bt = (ToolStripMenuItem)sender;
            foreach (ToolStripMenuItem btn in bts)
            {
                btn.Checked = btn == bt;
            }
            DrawingSheet.GridStyle grid;
            Enum.TryParse(bt.Tag.ToString(), out grid);
            book.SelectedSheet.Grid = grid;
            pBox.Refresh();
        }

        private void restoreViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CenterSheet();
            AdjustScale(Util.Middle(pBox.Size), 1.0f);
            pBox.Refresh();
            pShadow.Refresh();
        }

        private void fDraw_ResizeEnd(object sender, EventArgs e)
        {
            //Properties.Settings.Default.WindowSize = Size;
        }

        private void fDraw_Resize(object sender, EventArgs e)
        {
            /*if(WindowState != Properties.Settings.Default.WindowState && WindowState != FormWindowState.Minimized)
            {
                Properties.Settings.Default.WindowState = WindowState;
            }*/
        }

        private void pBox_SizeChanged(object sender, EventArgs e)
        {
            pShadow.Refresh();
        }

        private void fDraw_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Properties.Settings.Default.Save();
        }

        private void tvObjects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvObjects.SelectedNode.Tag != null && tvObjects.SelectedNode.Tag is DrawableObject) //Selecting object
            {
                book.SelectedSheet = ((DrawableObject)tvObjects.SelectedNode.Tag).OwnerDraw.OwnerSheet;
                mouse.ClearSelection();
                mouse.draw = book.SelectedSheet.Sketch;
                mouse.AddToSelection((DrawableObject)tvObjects.SelectedNode.Tag);
                propertyGrid.SelectedObject = (DrawableObject)tvObjects.SelectedNode.Tag;
                pBox.Refresh();
            }
            else if (tvObjects.SelectedNode.Level == 0 && tvObjects.SelectedNode.Tag != null)   //Selecting sheet
            {
                book.SelectedSheet = (DrawingSheet)tvObjects.SelectedNode.Tag;
                mouse.ClearSelection();
                mouse.draw = book.SelectedSheet.Sketch;
                propertyGrid.SelectedObject = book.SelectedSheet;
                pBox.Refresh();
            }
        }

        private void tvObjects_DoubleClick(object sender, EventArgs e)
        {
            if (tvObjects.SelectedNode.Tag != null && tvObjects.SelectedNode.Tag is DrawableObject)
            {
                GoToObject((DrawableObject)tvObjects.SelectedNode.Tag);
            }
        }

        private void goToObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToObject((DrawableObject)tvObjects.SelectedNode.Tag);
        }

        internal void GoToObject(DrawableObject @object, bool select = false)
        {
            if (@object == null) return;
            if (select)
            {
                book.SelectedSheet = @object.OwnerDraw.OwnerSheet;
                mouse.ClearSelection();
                mouse.draw = book.SelectedSheet.Sketch;
                mouse.AddToSelection(@object);
                propertyGrid.SelectedObject = @object;
            }
            ScrollTo(@object.Center);
            pBox.Focus();
        }

        private void pictureContextMenu_Opening(object sender, CancelEventArgs e)
        {
            bool onObject = mouse.OnObject != null;
            openContextMenu.Visible = onObject && mouse.OnObject is Nested;
            separator1ContextMenu.Visible = onObject && mouse.OnObject is Nested;
            copyContextMenu.Visible = onObject;
            cutContextMenu.Visible = onObject;
            pasteContextMenu.Visible = pasteToolStripMenuItem.Enabled;
            deleteContextMenu.Visible = onObject;
            separator2ContextMenu.Visible = onObject || pasteToolStripMenuItem.Enabled;
        }

        private void centerViewContextMenu_Click(object sender, EventArgs e)
        {
            if (mouse.OnObject != null) ScrollTo(mouse.OnObject.Center);
            else ScrollTo(mouse.SecundaryClickPoint);
        }

        private void tvObjects_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            bool sheetOptions, groupOptions, objectOptions;
            tvObjects.SelectedNode = e.Node;
            tvObjects.Tag = e.Node;
            sheetOptions = e.Node.Level == 0;
            groupOptions = e.Node.Level == 1;
            objectOptions = e.Node.Level == 2;
            if (groupOptions) tvObjects.ContextMenuStrip = null;
            else tvObjects.ContextMenuStrip = treeContextMenu;
            createNewSheetTreeMenu.Visible = sheetOptions;
            deleteSheetTreeMenu.Visible = sheetOptions && book.SelectedSheet != book.Sheets[0];
            goToObjectToolStripMenuItem.Visible = objectOptions;
            toolStripSeparator9.Visible = objectOptions;
            deleteObjectTreeMenu.Visible = objectOptions;
        }

        private void createNewSheetTreeMenu_Click(object sender, EventArgs e)
        {
            AddAction(RecordableAction.ActionTypes.AddSheet);
        }

        private void createNewModelTreeMenu_Click(object sender, EventArgs e)
        {
            AddAction(RecordableAction.ActionTypes.AddModel);
        }

        private void btVariables_Click(object sender, EventArgs e)
        {
            AddAction(RecordableAction.ActionTypes.VariablesChanged);
        }

        private void deleteSheetTreeMenu_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete sheet cannot be undo, continue?", "Delete sheet?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            AddAction(RecordableAction.ActionTypes.DeleteSheet);
        }

#region "Diagram check"
        private void btChecks_Click(object sender, EventArgs e)
        {
            DrawCheck();
        }

        private void CheckDiagram()
        {
            msgList.Items.Clear();
            book.BuildData = new GeneratorData(book, profile, rtbSimLog);
            foreach (CheckMessage msg in book.BuildData.MessagesList)
            {
                msgList.Items.Add(msg.GetListViewItem());
            }
            logsViewToolStripMenuItem.Checked = true;
       }

        private bool DrawCheck(bool promptMessages = true)
        {
            bool res = false;
            CheckDiagram();
            if (book.BuildData.ErrorsCount == 0 && book.BuildData.WarningsCount == 0)
            {
                lbStatus.ForeColor = Color.Black;
                lbStatus.Text = "Check successful without errors.";
                return true;
            }
            else
            {
                lbStatus.ForeColor = Color.DarkRed;
                lbStatus.Text = string.Format("Check finished with {0} errors and {1} warnings.", book.BuildData.ErrorsCount, book.BuildData.WarningsCount);
                if (book.BuildData.ErrorsCount == 0)
                {
                    res = true;
                }
                else if (promptMessages)
                {
                    //MessageBox.Show("There are errors in the diagram.", "The verification failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logsViewToolStripMenuItem.Checked = true;
                }
            }
            return res;
        }

        private void msgList_DoubleClick(object sender, EventArgs e)
        {
            if (msgList.SelectedItems.Count == 0) return;
            GoToObject((msgList.SelectedItems[0].Tag as CheckMessage).Object, true);
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (msgList.SelectedItems.Count == 0)
            {
                e.Cancel = true;
            }
            else
            {
                showObject2ToolStripMenuItem.Enabled = msgList.SelectedItems[0].SubItems[2].Text != "";
            }
        }

        private void showObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((fDraw)Owner).GoToObject((msgList.SelectedItems[0].Tag as CheckMessage).Object, true);
        }

        private void showObject2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((fDraw)Owner).GoToObject((msgList.SelectedItems[0].Tag as CheckMessage).Object2, true);
        }
#endregion

        private void btGenerateCode_Click(object sender, EventArgs e)
        {
            if (!DrawCheck()) return;

            ICodeGenerationProject project;

            //Create project
            if(book.Language == "Cottle")
            {
                string path = Path.Combine(book.ScriptsFolder, "cottle.ini");
                if (!File.Exists(path))
                {
                    MessageBox.Show("Scripts source folder not found.", "Code generator error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    project = new CodeGeneration.Interpreter.Project(book.BuildData, book.ScriptsFolder);
                }
            }
            else
            {
                MessageBox.Show("Languaje not supported.", "Code generator error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Generate code
            bool res = project.GenerateCode();

            if (res)
            {
                MessageBox.Show("Code was generated and is saved on this file path.", "Code generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btGenConfig_Click(object sender, EventArgs e)
        {
            ConfigurationToolStripMenuItem_Click(null, null);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog(this);
        }

        ActionsViewer viewer;
        private void actionsViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer = new ActionsViewer(book.Actions);
            viewer.Show(this);
        }

        private void drawStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DrawStateViewer viewer = new DrawStateViewer(book, mouse);
            viewer.ShowDialog(this);
        }

        private void CodGenLanguageSelection(object sender, EventArgs e)
        {
            CottleConfigForm configForm = new CottleConfigForm();
            List<string> paths = new List<string>();
            foreach(string dir in Directory.EnumerateDirectories(Environment.CurrentDirectory))
            {
                string name = Path.GetFileName(dir);
                if (name.EndsWith(".cottle"))
                {
                    configForm.languagesList.Items.Add(name.Substring(0, name.Length - 7));
                    paths.Add(dir);
                }
            }
            if(profile.Path != "")
            {
                foreach (string dir in Directory.EnumerateDirectories(profile.Path))
                {
                    string name = Path.GetFileName(dir);
                    if (name.EndsWith(".cottle"))
                    {
                        configForm.languagesList.Items.Add(name.Substring(0, name.Length - 7));
                        paths.Add(dir);
                    }
                }
            }
            if (configForm.ShowDialog(this) == DialogResult.Cancel) return;
            book.Language = "Cottle";
            book.TargetLanguage = configForm.languagesList.Text;
            book.ScriptsFolder = paths[configForm.languagesList.SelectedIndex];
            lbLanguage.Text = book.TargetLanguage;

            saveToolStripMenuItem.Enabled = true;
        }

        private void SimulationMode(bool enable)
        {
            simulationStatus = SimulationState.EndOfCycle;

            //general
            simulationMode = enable;
            btSimulate.Enabled = !enable;
            btEmulate.Enabled = !enable;
            simTools.Visible = enable;

            //Tabs
            if (enable)
            {
                leftTabControl.TabPages.Remove(objectsTabPage);
                leftTabControl.TabPages.Add(variablesTabPage);
            }
            else
            {
                leftTabControl.TabPages.Remove(variablesTabPage);
                leftTabControl.TabPages.Add(objectsTabPage);
            }

            //control buttons
            btPreviousPage.Enabled = book.SelectedSheet != book.Sheets.First();
            btNextPage.Enabled = book.SelectedSheet != book.Sheets.Last();
            lbCurrentPage.Text = book.SelectedSheet.Name;

            btPlay.Enabled = enable;
            btFastPlay.Enabled = enable;
            btStop.Enabled = false;
            btSimOpen.Enabled = true;
            btPause.Enabled = false;
            btSimSave.Enabled = false;
            btCycleStep.Enabled = enable;
            btSmallStep.Enabled = enable;

            //menus
            fIleToolStripMenuItem.Enabled = !enable;
            editToolStripMenuItem.Enabled = !enable;
            formatToolStripMenuItem.Enabled = !enable;

            //sub-menus
            iOTableToolStripMenuItem.Enabled = !enable;
            checkToolStripMenuItem.Enabled = !enable;
            generateCodeToolStripMenuItem.Enabled = !enable;

            //buttons
            btMouseTool_Click(btToolMouse, new EventArgs());
            standardTools.Visible = !enable;

            //panels
            propertyGrid.Enabled = !enable;
            dgEntradas.Enabled = enable;
            dgVariables.Enabled = enable;

            //specific operations
            if (enable)
            {
                prepareSimulation();
                leftTabControl.SelectedTab = variablesTabPage;
                logsViewToolStripMenuItem.Checked = true;
                if (buttomTabControl.SelectedTab == messagesTabPage) buttomTabControl.SelectedTab = simLogTabPage;
            }
            else
            {
                leftTabControl.SelectedTab = objectsTabPage;
                Simulation.Dispose();
                Simulation = null;
            }
            pBox.Refresh();
        }

        private void prepareSimulation()
        {
            int idx;
            signalsDraw.VariablesStatus = new VariablesStatusLog();
            // Adding variables and states to track
            foreach (BooleanInput variable in book.BuildData.Variables.BooleanInputs)
            {
                signalsDraw.VariablesStatus.Histories.Add(new VariableHistory(variable.Name, variable.DefaultValue));
            }
            foreach (Variable variable in book.BuildData.Variables.EventInputs)
            {
                signalsDraw.VariablesStatus.Histories.Add(new VariableHistory(variable.Name));
            }
            foreach (BooleanOutput variable in book.BuildData.Variables.BooleanOutputs)
            {
                signalsDraw.VariablesStatus.Histories.Add(new VariableHistory(variable.Name, variable.DefaultValue));
            }
            foreach (Variable variable in book.BuildData.Variables.EventOutputs)
            {
                signalsDraw.VariablesStatus.Histories.Add(new VariableHistory(variable.Name));
            }
            foreach (BooleanFlag variable in book.BuildData.Variables.BooleanFlags)
            {
                signalsDraw.VariablesStatus.Histories.Add(new VariableHistory(variable.Name, variable.DefaultValue));
            }
            foreach (MessageFlag variable in book.BuildData.Variables.MessageFlags)
            {
                signalsDraw.VariablesStatus.Histories.Add(new VariableHistory(variable.Name));
            }
            foreach (BasicObjectsTree tree in book.BuildData.Trees)
            {
                foreach (BasicMachine mach in tree.SuperStatesList())
                {
                    if ((mach.State as State).Track)
                    {
                        signalsDraw.VariablesStatus.Histories.Add(new VariableHistory(mach.Name, false));
                    }
                }
                foreach (BasicState state in tree.StatesList())
                {
                    if ((state.State as State).Track)
                    {
                        signalsDraw.VariablesStatus.Histories.Add(new VariableHistory(state.Name, false));
                    }
                }
            }
            Simulation = new VirtualMachine(book.BuildData, signalsDraw.VariablesStatus);

            //clear simulation log
            rtbSimLog.Clear();

            //input variables
            dgEntradas.Rows.Clear();
            foreach (BooleanInput var in book.Variables.BooleanInputs)
            {
                dgEntradas.Rows.Add("B", var.Name, var.DefaultValue, var.DefaultValue);
                book.BuildData.Store[var.Name] = var.DefaultValue;
            }
            foreach (Variable var in book.Variables.EventInputs)
            {
                dgEntradas.Rows.Add("E", var.Name, false, false);
                book.BuildData.Store[var.Name] = false;
            }
            //Other variables
            dgVariables.Rows.Clear();
            foreach (BooleanOutput var in book.Variables.BooleanOutputs)
            {
                book.BuildData.Store[var.Name] = var.DefaultValue;
                idx = dgVariables.Rows.Add("O", var.Name, "");
                dgVariables.Rows[idx].Cells[0].ToolTipText = "Boolean Output";
            }
            foreach (BooleanFlag var in book.Variables.BooleanFlags)
            {
                book.BuildData.Store[var.Name] = var.DefaultValue;
                idx = dgVariables.Rows.Add("F", var.Name, "");
                dgVariables.Rows[idx].Cells[0].ToolTipText = "Boolean Flag";
            }
            foreach (CounterFlag var in book.Variables.CounterFlags)
            {
                book.BuildData.Store[var.Name] = var.DefaultValue;
                idx = dgVariables.Rows.Add("C", var.Name, "");
                dgVariables.Rows[idx].Cells[0].ToolTipText = "Counter Flag";
            }
            foreach (MessageFlag var in book.Variables.MessageFlags)
            {
                book.BuildData.Store[var.Name] = false;
                idx = dgVariables.Rows.Add("M", var.Name, "");
                dgVariables.Rows[idx].Cells[0].ToolTipText = "Message";
            }
            foreach (EventOutput var in book.Variables.EventOutputs)
            {
                book.BuildData.Store[var.Name] = false;
                idx = dgVariables.Rows.Add("E", var.Name, "");
                dgVariables.Rows[idx].Cells[0].ToolTipText = "Output Event";
            }
            foreach (BasicObjectsTree tree in book.BuildData.Trees)
            {
                idx = dgVariables.Rows.Add("R", tree.Root.Name, "");
                dgVariables.Rows[idx].Cells[0].ToolTipText = "Root machine";
                foreach (BasicMachine mach in tree.SuperStatesList())
                {
                    idx = dgVariables.Rows.Add("SM", mach.Name, "");
                    dgVariables.Rows[idx].Cells[0].ToolTipText = "Sub machine";
                }
            }
            foreach (BasicObjectsTree tree in book.BuildData.Trees)
            {
                foreach (BasicMachine mach in tree.SuperStatesList())
                {
                    if ((mach.State as State).Track)
                    {
                        idx = dgVariables.Rows.Add("SS", mach.Name, "");
                        dgVariables.Rows[idx].Cells[0].ToolTipText = "Super State";
                        book.BuildData.Store[mach.Name] = false;
                    }
                }
                foreach (BasicState state in tree.StatesList())
                {
                    if ((state.State as State).Track)
                    {
                        idx = dgVariables.Rows.Add("S", state.Name, "");
                        dgVariables.Rows[idx].Cells[0].ToolTipText = "State";
                        book.BuildData.Store[state.Name] = false;
                    }
                }
            }
        }

        private void btSimulate_Click(object sender, EventArgs e)
        {
            if (!DrawCheck()) return;
            emulationToolStripMenuItem.Enabled = false;
            SimulationMode(true);
            UpdateSimulationVariables();
        }

        private void btEmulate_Click(object sender, EventArgs e)
        {
            if (!DrawCheck()) return;
            simulationToolStripMenuItem.Enabled = false;
            SimulationMode(true);
            UpdateSimulationVariables();
        }

        private void SimulationPlay(bool enable)
        {
            simTimer.Enabled = enable;
            fastTimer.Enabled = false;
            btPause.Enabled = enable;
            btSimSave.Enabled = !enable;
            btPlay.Enabled = !enable;
            btFastPlay.Enabled = true;
            btStop.Enabled = true;
            btSimOpen.Enabled = false;
            btCycleStep.Enabled = !enable;
            btSmallStep.Enabled = !enable;
        }

        private void btPlay_Click(object sender, EventArgs e)
        {
            SimulationPlay(true);
        }

        private void FastSimulationPlay(bool enable)
        {
            fastTimer.Enabled = enable;
            simTimer.Enabled = false;
            btPause.Enabled = enable;
            btSimSave.Enabled = !enable;
            btPlay.Enabled = true;
            btStop.Enabled = true;
            btSimOpen.Enabled = false;
            btFastPlay.Enabled = !enable;
            btCycleStep.Enabled = !enable;
            btSmallStep.Enabled = !enable;
        }

        private void btFastPlay_Click(object sender, EventArgs e)
        {
            FastSimulationPlay(true);
        }

        private void btExitSimule_Click(object sender, EventArgs e)
        {
            if (simTimer.Enabled) SimulationPlay(false);
            if (fastTimer.Enabled) FastSimulationPlay(false);
            emulationToolStripMenuItem.Enabled = true;
            simulationToolStripMenuItem.Enabled = true;
            SimulationMode(false);
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            if(simTimer.Enabled) SimulationPlay(false);
            if (fastTimer.Enabled) FastSimulationPlay(false);

            btStop.Enabled = false;
            btSimOpen.Enabled = true;
            btSimSave.Enabled = false;

            Simulation.Data.ResetMasterCounter();
            UpdateSimulationVariables();
            simulationStatus = SimulationState.EndOfCycle;
            Simulation.Dispose();
            prepareSimulation();
            pBox.Refresh();
            ioSim.Refresh();
        }

        private void btPause_Click(object sender, EventArgs e)
        {
            SimulationPlay(false);
            FastSimulationPlay(false);
        }

        private void ExecuteTimeStep()
        {
            if (simulationStatus == SimulationState.EndOfCycle)
            {
                CleanSimulationOutputEvents();
                LoadSimulationVariables();
            }
            int count = fastTimer.Enabled ? Convert.ToInt32(fastTimer.Tag) : 1;
            while (count > 0)
            {
                do
                {
                    simulationStatus = Simulation.MoveToNextStepState();
                } while (simulationStatus != SimulationState.EndOfCycle);
                count--;
            }
            UpdateSimulationVariables();
            CleanSimulationEvents();
            lbStatus.Text = Simulation.GetStatus();
            lbCurrentState.Text = Simulation.GetCurrentState(lbCurrentState.Text);
            lbCurrentTransition.Text = Simulation.GetCurrentTransition();
            pBox.Refresh();
            ioSim.Refresh();
        }

        private void btTimeStep_Click(object sender, EventArgs e)
        {
            btStop.Enabled = true;
            btSimOpen.Enabled = false;
            ExecuteTimeStep();
            btSimSave.Enabled = true;
        }

        private void ExecuteConditionStep()
        {
            if (simulationStatus == SimulationState.EndOfCycle)
            {
                CleanSimulationOutputEvents();
                LoadSimulationVariables();
            }
            simulationStatus = Simulation.MoveToNextStepState();
            UpdateSimulationVariables();
            if (simulationStatus == SimulationState.EndOfCycle)
            {
                CleanSimulationEvents();
            }
            lbCurrentState.Text = Simulation.GetCurrentState(lbCurrentState.Text);
            lbCurrentTransition.Text = Simulation.GetCurrentTransition();
            lbStatus.Text = Simulation.GetStatus();
            pBox.Refresh();
            ioSim.Refresh();
        }

        private void btConditionStep_Click(object sender, EventArgs e)
        {
            btStop.Enabled = true;
            btSimOpen.Enabled = false;
            ExecuteConditionStep();
            btSimSave.Enabled = simulationStatus == SimulationState.EndOfCycle;
        }

        private void LoadSimulationVariables()
        {
            if (signalsDraw.VariablesShadow != null)
            {
                foreach (DataGridViewRow row in dgEntradas.Rows)
                {
                    string varName = row.Cells[1].Value.ToString();
                    if (signalsDraw.VariablesShadow.HadHistoryChanged(varName, Simulation.Data.MasterCounter))
                    {
                        row.Cells[2].Value = !((bool)row.Cells[2].Value);
                    }
                }
            }
            foreach(DataGridViewRow row in dgEntradas.Rows)
            {
                if ((bool)row.Cells[3].Value != (bool)row.Cells[2].Value)
                {
                    string valType = row.Cells[0].Value.ToString();
                    string varName = row.Cells[1].Value.ToString();
                    string val = row.Cells[2].Value.ToString();
                    string current = row.Cells[3].Value.ToString();
                    row.Cells[3].Value = (bool)row.Cells[2].Value;
                    book.BuildData.Store[row.Cells[1].Value.ToString()] = (bool)row.Cells[2].Value;
                    switch (valType)
                    {
                        case "E":
                            rtbSimLog.AppendText(string.Format("{0}: Received Event ", Simulation.Data.MasterCounter));
                            rtbSimLog.SelectionColor = Color.Red;
                            rtbSimLog.AppendText(varName);
                            rtbSimLog.AppendText(Environment.NewLine);
                            signalsDraw.VariablesStatus.ChangeVariable(varName, Simulation.Data.MasterCounter);
                            break;
                        case "B":
                            rtbSimLog.AppendText(string.Format("{0}: Changed Input ", Simulation.Data.MasterCounter));
                            rtbSimLog.SelectionColor = Color.Blue;
                            rtbSimLog.AppendText(varName);
                            rtbSimLog.SelectionColor = Color.Black;
                            rtbSimLog.AppendText(string.Format(" from {0} to {1}.", current, val));
                            rtbSimLog.AppendText(Environment.NewLine);
                            signalsDraw.VariablesStatus.ChangeVariable(varName, Simulation.Data.MasterCounter);
                            break;
                    }
                }
            }
        }

        private void UpdateSimulationVariables()
        {
            foreach (DataGridViewRow row in dgVariables.Rows)
            {
                var cell = row.Cells[2];
                string varType = row.Cells[0].Value.ToString();
                string varName = row.Cells[1].Value.ToString();
                if (varType == "R" || varType == "SM") varName = Util.CounterName(varName);
                string current = cell.Value.ToString();
                if (current == "sent") current = "true";
                if (current == "") current = "false";

                // Get current variable name from Store
                string val = book.BuildData.Store[varName].AsString;
                if (val == "") val = "false";

                // master counter offset
                int offset = simulationStatus == SimulationState.EndOfCycle ? 0 : 1;

                if (current != val)
                {
                    if (varType == "E" || varType == "M")
                    {
                        cell.Value = val == "false" ? "" : "sent";
                    }
                    else
                    {
                        cell.Value = val;
                    }
                    if (current != "")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        switch (varType)
                        {
                            case "R":
                            case "SM":
                                break;
                            case "M":
                                if (val == "true")
                                {
                                    rtbSimLog.AppendText(string.Format("{0}: Sent message ", Simulation.Data.MasterCounter + offset));
                                    rtbSimLog.SelectionColor = Color.Orange;
                                    rtbSimLog.AppendText(varName);
                                    rtbSimLog.AppendText("." + Environment.NewLine);
                                }
                                break;
                            case "O":
                                rtbSimLog.AppendText(string.Format("{0}: Changed output ", Simulation.Data.MasterCounter + offset));
                                rtbSimLog.SelectionColor = Color.Blue;
                                rtbSimLog.AppendText(varName);
                                rtbSimLog.SelectionColor = Color.Black;
                                rtbSimLog.AppendText(string.Format(" from {0} to {1}.", current, val));
                                rtbSimLog.AppendText(Environment.NewLine);
                                signalsDraw.VariablesStatus.ChangeVariable(varName, Simulation.Data.MasterCounter + offset);
                                break;
                            case "F":
                            case "C":
                                rtbSimLog.AppendText(string.Format("{0}: Changed flag ", Simulation.Data.MasterCounter + offset));
                                rtbSimLog.SelectionColor = Color.Green;
                                rtbSimLog.AppendText(varName);
                                rtbSimLog.SelectionColor = Color.Black;
                                rtbSimLog.AppendText(string.Format(" from {0} to {1}.", current, val));
                                rtbSimLog.AppendText(Environment.NewLine);
                                signalsDraw.VariablesStatus.ChangeVariable(varName, Simulation.Data.MasterCounter + offset);
                                break;
                            case "E":
                                rtbSimLog.AppendText(string.Format("{0}: Output Event ", Simulation.Data.MasterCounter));
                                rtbSimLog.SelectionColor = Color.Green;
                                rtbSimLog.AppendText(varName);
                                rtbSimLog.AppendText(Environment.NewLine);
                                signalsDraw.VariablesStatus.ChangeVariable(varName, Simulation.Data.MasterCounter + offset);
                                break;
                            case "S":
                            case "SS":
                                signalsDraw.VariablesStatus.ChangeVariable(varName, Simulation.Data.MasterCounter + offset);
                                break;
                        }
                    }
                }
                else if(row.DefaultCellStyle.BackColor != Color.White)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }
            }
        }

        private void CleanSimulationEvents()
        {
            foreach (DataGridViewRow row in dgEntradas.Rows)
            {
                if (row.Cells[0].Value.ToString() == "E" && (bool)row.Cells[3].Value == true)
                {
                    row.Cells[2].Value = false;
                    row.Cells[3].Value = false;
                }
            }
            if (signalsDraw.VariablesShadow != null)
            {
                if (Simulation.Data.MasterCounter == Math.Max(signalsDraw.VariablesShadow.GetTimeMax(), signalsDraw.VariablesShadow.MaxTimeDraw))
                {
                    btPause_Click(btStop, null);
                    return;
                }
            }
        }

        private void CleanSimulationOutputEvents()
        {
            foreach (DataGridViewRow row in dgVariables.Rows)
            {
                if (row.Cells[0].Value.ToString() == "E" && row.Cells[2].Value.ToString() == "sent")
                {
                    row.Cells[2].Value = "";
                    book.BuildData.Store[row.Cells[1].Value.ToString()] = false;
                }
            }
        }

        private void logsViewToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer2.Panel2Collapsed = !logsViewToolStripMenuItem.Checked;
        }

        private void btCloseLogView_Click(object sender, EventArgs e)
        {
            logsViewToolStripMenuItem.Checked = false;
        }

        private void dgVariables_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dgVariables.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgEntradas_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dgEntradas.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgEntradas_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex != 2) return;
            var row = dgEntradas.Rows[e.RowIndex];
            string valType = row.Cells[0].Value.ToString();
            if (valType == "E")
            {
                DataGridViewCell cell = dgEntradas.Rows[e.RowIndex].Cells[2];
                cell.ReadOnly = (bool)cell.Value;
            }
        }

        private void simTimer_Tick(object sender, EventArgs e)
        {
            ExecuteConditionStep();
        }

        private void fastTimer_Tick(object sender, EventArgs e)
        {
            ExecuteTimeStep();
        }

        private void btNextPage_Click(object sender, EventArgs e)
        {
            if(book.SelectedSheet != book.Sheets.Last()) book.SelectedSheet = book.Sheets[book.Sheets.IndexOf(book.SelectedSheet) + 1];
            btPreviousPage.Enabled = book.SelectedSheet != book.Sheets.First();
            btNextPage.Enabled = book.SelectedSheet != book.Sheets.Last();
            lbCurrentPage.Text = book.SelectedSheet.Name;
            pBox.Refresh();
        }

        private void btPreviousPage_Click(object sender, EventArgs e)
        {
            if (book.SelectedSheet != book.Sheets.First()) book.SelectedSheet = book.Sheets[book.Sheets.IndexOf(book.SelectedSheet) - 1];
            btPreviousPage.Enabled = book.SelectedSheet != book.Sheets.First();
            btNextPage.Enabled = book.SelectedSheet != book.Sheets.Last();
            lbCurrentPage.Text = book.SelectedSheet.Name;
            pBox.Refresh();
        }

        private void SlowSimSpeed_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem selected = (ToolStripMenuItem)sender;
            foreach(ToolStripMenuItem item in btSlowSpeed.DropDownItems)
            {
                item.Checked = item == selected;
            }
            switch (selected.Text)
            {
                case "Slow":
                    simTimer.Interval = 400;
                    break;
                case "Normal":
                    simTimer.Interval = 200;
                    break;
                case "Fast":
                    simTimer.Interval = 100;
                    break;
                case "Very fast":
                    simTimer.Interval = 50;
                    break;
                case "Fastest":
                    simTimer.Interval = 10;
                    break;
            }
        }

        private void FastSimSpeed_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem selected = (ToolStripMenuItem)sender;
            foreach (ToolStripMenuItem item in btFastSpeed.DropDownItems)
            {
                item.Checked = item == selected;
            }
            switch (selected.Text)
            {
                case "Slow":
                    fastTimer.Interval = 200;
                    fastTimer.Tag = 1;
                    break;
                case "Normal":
                    fastTimer.Interval = 100;
                    fastTimer.Tag = 1;
                    break;
                case "Fast":
                    fastTimer.Interval = 50;
                    fastTimer.Tag = 1;
                    break;
                case "Very fast":
                    fastTimer.Interval = 40;
                    fastTimer.Tag = 10;
                    break;
                case "Fastest":
                    fastTimer.Interval = 40;
                    fastTimer.Tag = 50;
                    break;
            }
        }

        private void stateCADFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(dialogImport.ShowDialog(this) == DialogResult.OK)
            {
                book = new PhasesBook(appInterface, DrawingSheet.DefaultSize);
                mouse = new MouseTool(book.SelectedSheet.Sketch, pBox);
                if(!book.ImportStateCADFile(File.ReadAllBytes(dialogImport.FileName)))
                {
                    MessageBox.Show("Error importing the file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                AdjustSheetView();
                pBox.Refresh();
                pShadow.Refresh();
            }
        }

        private void ioSim_Paint(object sender, PaintEventArgs e)
        {
            signalsDraw.Paint(e.Graphics);
        }

        private void btSimSave_Click(object sender, EventArgs e)
        {
            DialogResult result = saveSimDialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                StringBuilder str = new StringBuilder();
                str.Append("A " + signalsDraw.VariablesStatus.MaxTimeDraw + Environment.NewLine);
                foreach(VariableHistory history in signalsDraw.VariablesStatus.Histories)
                {
                    int ant = 0;
                    str.Append(history.Variable + " ");
                    if (history.IsEvent)
                    {
                        str.Append("3");
                    }
                    else
                    {
                        str.Append(history.InitialValue ? "1" : "0");
                    }
                    foreach(int value in history.History.Skip(1))
                    {
                        str.Append(" " + (value - ant).ToString());
                        ant = value;
                    }
                    str.AppendLine();
                }
                try
                {
                    File.WriteAllText(saveSimDialog.FileName, str.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file: " + ex.Message);
                }
            }
        }

        private void btSimOpen_Click(object sender, EventArgs e)
        {
            DialogResult result = openSimDialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                signalsDraw.VariablesShadow = new VariablesStatusLog();
                string[] lines;
                try
                {
                    lines = File.ReadAllLines(openSimDialog.FileName);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error opening file: " + ex.Message);
                    return;
                }
                if (lines.Count() == 0)
                {
                    MessageBox.Show("Error loading data file: File empty.");
                    return;
                }
                var head = lines.First().Split(' ');
                if (head.Count() < 2 || head.First() != "A")
                {
                    MessageBox.Show("Error loading data file: Unknown format.");
                    return;
                }
                if (int.TryParse(head[1], out int timeMax))
                {
                    signalsDraw.VariablesShadow.MaxTimeDraw = timeMax;
                }
                else
                {
                    MessageBox.Show("Error loading data file: Unknown format.");
                    return;
                }
                foreach (string line in lines.Skip(1))
                {
                    var words = line.Split(' ');
                    VariableHistory history;
                    if (words.Count() < 2) continue;
                    if (int.TryParse(words[1], out int initialValue))
                    {
                        if (initialValue == 3)
                        {
                            history = new VariableHistory(words.First());
                        }
                        else
                        {
                            history = new VariableHistory(words.First(), initialValue == 1);
                        }
                        signalsDraw.VariablesShadow.Histories.Add(history);
                    }
                    else
                    {
                        MessageBox.Show("Error loading data file: (initial value).");
                        return;
                    }
                    foreach (string word in words.Skip(2))
                    {
                        if (int.TryParse(word, out int increment))
                        {
                            history.AddIncrement(increment);
                        }
                        else
                        {
                            MessageBox.Show("Error loading data file.");
                            return;
                        }
                    }
                }
                btSimClose.Enabled = true;
                dgEntradas.Enabled = false;
                ioSim.Refresh();
            }
        }

        private void btSimClose_Click(object sender, EventArgs e)
        {
            signalsDraw.VariablesShadow = null;
            btSimClose.Enabled = false;
            dgEntradas.Enabled = true;
            ioSim.Refresh();
        }

        private void ConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!DrawCheck(false)) book.BuildData = null;
            CodeGeneratorConfig codeGeneratorConfig = new CodeGeneratorConfig(book.BuildData, book.ScriptsFolder);
            codeGeneratorConfig.ShowDialog();
            codeGeneratorConfig.Dispose();
        }

        private void pNGImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(book.SelectedSheet.Size.Width, book.SelectedSheet.Size.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bitmap);

            // Draw white background
            g.FillRectangle(Brushes.White, new Rectangle(Point.Empty, bitmap.Size));

            // Scaling
            Matrix pngTransform = new Matrix();
            pngTransform.Translate((float)book.SelectedSheet.Size.Width / 2f, (float)book.SelectedSheet.Size.Height / 2f);
            g.Transform = pngTransform;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // Draw objects
            book.SelectedSheet.Sketch.Paint(g, new DrawAttributes(Pens.Black, 1f));

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Portable Network Graphics (*.png)|*.png";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    bitmap.Save(dialog.FileName, ImageFormat.Png);
                }
            }
        }

        private Keys lastKeyDown = Keys.None;

        private void fDraw_KeyDown(object sender, KeyEventArgs e)
        {
            if (lastKeyDown == e.KeyCode) return;
            lastKeyDown = e.KeyCode;
            if (lastMouseState != null && pBox.Bounds.Contains(MousePosition))
            {
                pBox_MouseMove(pBox, lastMouseState);
            }
        }

        private void fDraw_KeyUp(object sender, KeyEventArgs e)
        {
            if (lastMouseState != null && pBox.Bounds.Contains(MousePosition))
            {
                pBox_MouseMove(pBox, lastMouseState);
            }
            if (lastKeyDown == e.KeyCode) lastKeyDown = Keys.None;
        }

        private void pBox_MouseLeave(object sender, EventArgs e)
        {
            mouse.SnapLocation = null;
            pBox.Invalidate();
        }
    }
}
